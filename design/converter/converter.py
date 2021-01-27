import datetime
import extractor
import validator
import re
import config

def remove(type, nullable=True, key=True, relation=True, percentage=True, unknown=True):
    if key:
        converted = extractor.groupType(type)
        if converted:
            type = converted

        type = type.replace(config.customs['key'], '')

    if nullable:
        type = type.replace(config.customs['nullable'], '')

    if relation:
        type = type.replace(config.customs['relation'], '')

    if percentage:
        type = type.replace(config.customs['percentage'], '')

    if unknown: # 이거뭐임
        type = type.replace(config.customs['unknown'], '')

    return type

def cast(dtype, value, schemaDict, enumDict):
    baseType = remove(dtype, relation=False)

    if value == '' or value is None:
        if validator.isNullable(dtype) or baseType == 'string':
            return value

        raise Exception(f'{dtype} 타입은 빈 데이터를 가질 수 없습니다.')

    if validator.isRelation(dtype):
        return cast(extractor.relationshipType(dtype, schemaDict), value, schemaDict, enumDict)
    
    if validator.isInteger(dtype):
        baseType = 'int'

    if baseType == 'string':
        return value

    if baseType == 'int':
        try:
            return int(value)
        except:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'double' or baseType == 'float':
        try:
            return float(value)
        except:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'DateTime':
        try:
            datetime.datetime.strptime(value, '%Y-%m-%d %H:%M:%S')
            return value
        except:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'TimeSpan':
        try:
            if type(value) is datetime.time:
                return str(value)
            else:
                datetime.datetime.strptime(value, '%H:%M:%S')
                return value
        except:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'bool':
        if type(value) is bool:
            return value
        if value.lower() == 'true':
            return True
        if value.lower() == 'false':
            return False
        raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'Point':
        match = re.match(config.regex['point'], value)
        if not match:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

        group = match.groupdict()
        return {'x': int(group['x']), 'y': int(group['y'])}

    if baseType in enumDict:
        if value not in enumDict[baseType]:
            raise Exception(f'{value}는 {baseType}에 존재하지 않습니다.')
        return value

    converted = extractor.listType(dtype)
    if converted:
        value = value.replace(' ', '')
        value = [cast(converted, x, schemaDict, enumDict) for x in value.split(',')]
        return value

    converted = extractor.groupType(dtype)
    if converted:
        value = cast(converted, value, schemaDict, enumDict)
        return value

    raise Exception(f'{baseType}는 올바른 타입이 아닙니다.')

def pureSchema(type, schemaDict):
    converted = extractor.groupType(type)
    if converted:
        return f"({pureSchema(converted, schemaDict)})"

    isKey = validator.isPrimaryKey(type)

    if validator.isRelation(type):
        type = remove(extractor.relationshipType(type, schemaDict), nullable=False)
        if isKey:
            type = config.customs['key'] + type
        
    if validator.isInteger(type):
        type = 'int'

    converted = extractor.arrayType(type)
    if converted:
        converted = pureSchema(converted, schemaDict)
        type = f'List<{converted}>'

    if isKey:
        return config.customs['key'] + remove(type)
    else:
        return type

def pureSchemaSet(schemaSet, schemaDict):
    schemaDict = {x: y for x, y in schemaDict.items()}
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
            if schemaSet[name]['usage'] not in (usage, 'common'):
                continue

            dataBuffer[name] = cast(schemaSet[name]['type'], value, schemaDict, enumDict)
        if dataBuffer:
            result.append(dataBuffer)
    
    if not result:
        return None

    return result

def toDictionary(schemaSet, dataSet):
    primary = extractor.primaryKey(schemaSet)
    index = extractor.indexKey(schemaSet)
    if primary:
        return {data[primary['name']]:data for data in dataSet}
    elif index:
        groupNames = set([x[index['name']] for x in dataSet])
        return {groupName:[data for data in dataSet if data[index['name']] == groupName] for groupName in groupNames}
    else:
        return dataSet