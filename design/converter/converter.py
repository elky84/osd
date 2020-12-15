import datetime
import extractor
import re

CUSTOM_KEYWORDS = ['$', '%', '~']

def removeKeyword(type):
    for kw in CUSTOM_KEYWORDS:
        type = type.replace(kw, '')

    return type

def cast(dtype, value, schemaDict, enumDict):
    baseType = removeKeyword(dtype.replace('?', '').replace('*', ''))

    if value == '' or value is None:
        if dtype.endswith('?') or baseType == 'string':
            return value

        raise Exception('{dtype} cannot be null or empty')

    if dtype.startswith('$'):
        return cast(extractor.relationshipType(dtype, schemaDict), value, schemaDict, enumDict)
    
    if dtype.startswith('~') or dtype.startswith('%'):
        baseType = 'int'

    if baseType == 'string':
        return value

    if baseType == 'int':
        return int(value)

    if baseType == 'double' or baseType == 'float':
        return float(value)

    if baseType == 'DateTime':
        datetime.datetime.strptime(value, '%Y-%m-%d %H:%M:%S')
        return value

    if baseType == 'TimeSpan':
        datetime.datetime.strptime(value, '%H:%M:%S')
        return value

    if baseType == 'bool':
        if type(value) is bool:
            return value

        if value.lower() == 'true':
            return True
        if value.lower() == 'false':
            return False
        raise Exception(f'{value} is not a boolean type.')

    if baseType == 'Point':
        result = re.search(r'^\((?P<x>\d),\s?(?P<y>\d)\)$', value)
        if not result:
            raise Exception(f'{value} cannot convert to point type.')

        group = result.groupdict()
        return {'x': int(group['x']), 'y': int(group['y'])}

    if baseType in enumDict:
        return value

    match = re.match(r'^List<(?P<type>\w+)>$', baseType)
    if match:
        value = value.replace(' ', '')
        value = [f'({x})' for x in re.compile(r'\),\(|\)|\(').split(value) if x] if '(' in value and ')' in value else value.split(',')
        value = [cast(match.groupdict()['type'], x, schemaDict, enumDict) for x in value]
        return value

    print(f'invalid type {baseType}')
    return value

def pureSchema(type, schemaDict):
    if type.startswith('$'):
        type = extractor.relationshipType(type, schemaDict).replace('*', '')
        
    if type.startswith('%') or type.startswith('~'):
        type = 'int'

    match = re.match(r'^\[(?P<type>\w*)\]$', type)
    if match:
        inner = match.groupdict()['type']
        inner = pureSchema(inner, schemaDict)
        type = f'List<{inner}>'

    return type

def pureSchemaSet(schemaSet, schemaDict):
    convertedSet = []
    for x in schemaSet:
        name, type, usage = x['name'], x['type'], x['usage']
        convertedSet.append({'name': name, 'type': pureSchema(type, schemaDict), 'usage': usage})
    return convertedSet

def pureSchemaDict(schemaDict):
    result = {}
    for name, schemaSet in schemaDict.items():
        result[name] = pureSchemaSet(schemaSet, schemaDict)

    return result

def convert(usage, name, schemaDict, dataDict, enumDict):
    schemaSet, dataSet = schemaDict[name], dataDict[name]
    schemaSet = {x['name']: x for x in schemaSet}
    result = []
    for data in dataSet:
        dataBuffer = {}
        for name, value in data.items():
            if schemaSet[name]['usage'] not in [usage, 'common']:
                continue

            dataBuffer[name] = cast(schemaSet[name]['type'], value, schemaDict, enumDict)
        if dataBuffer:
            result.append(dataBuffer)
    
    if not result:
        return None

    return result