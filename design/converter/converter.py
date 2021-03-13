import datetime
import extractor
import validator
import re
import config
from resources import resources

CUSTOM_KEYWORDS = {'key': '*', 'nullable': '?', 'relation': '$', 'percentage': '%', 'unknown': '~'}

def remove(type, nullable=True, key=True, relation=True, percentage=True, unknown=True):
    if key:
        converted = extractor.groupType(type)
        if converted:
            type = converted

        type = type.replace(CUSTOM_KEYWORDS['key'], '')

    if nullable:
        type = type.replace(CUSTOM_KEYWORDS['nullable'], '')

    if relation:
        type = type.replace(CUSTOM_KEYWORDS['relation'], '')

    if percentage:
        type = type.replace(CUSTOM_KEYWORDS['percentage'], '')

    if unknown: # 이거뭐임
        type = type.replace(CUSTOM_KEYWORDS['unknown'], '')

    return type

def cast(dataType, value):
    baseType = remove(dataType, relation=False)

    if value == '' or value is None:
        if validator.isNullable(dataType) or baseType == 'string':
            return value

        if extractor.listType(dataType) is not None:
            return []

        raise Exception(f'{dataType} 타입은 빈 데이터를 가질 수 없습니다.')

    if validator.isRelation(dataType):
        return cast(extractor.relationshipType(dataType), value)
    
    if validator.isInteger(dataType):
        baseType = 'int'

    if baseType == 'string':
        return value

    if baseType == 'int' or baseType == 'long':
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
            if type(value) is datetime.datetime:
                return value.strftime('%Y-%m-%d %H:%M:%S')

            datetime.datetime.strptime(value, '%Y-%m-%d %H:%M:%S')
            return value
        except:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

    if baseType == 'TimeSpan':
        try:
            if type(value) is datetime.datetime or type(value) is datetime.time:
                return value.strftime('%H:%M:%S')

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

    if baseType == 'Dsl':
        funcName, parameters = extractor.dsl(value)
        for i in range(len(config.dsl[funcName])):
            if len(parameters) > i:
                continue

            parameters.append(config.dsl[funcName][i]['default'])

        return {'Type': funcName, 'Parameters': parameters}

    if baseType == 'Point':
        match = re.match(config.regex['point'], value)
        if not match:
            raise Exception(f'{baseType} 형식으로 변환될 수 없는 데이터가 존재합니다. ({value})')

        group = match.groupdict()
        return {'x': int(group['x']), 'y': int(group['y'])}


    if baseType in resources.enumDict:
        if value.lower() not in [x.lower() for x in resources.enumDict[baseType]]:
            raise Exception(f"{value}는 {baseType}에 존재하지 않습니다.")
        return value

    converted = extractor.listType(dataType)
    if converted:
        if type(value) is str:
            value = value.replace(' ', '')
            value = [cast(converted, x) for x in value.split('\n')]
        else:
            value = [cast(converted, value)]
            
        return value

    converted = extractor.groupType(dataType)
    if converted:
        value = cast(converted, value)
        return value

    raise Exception(f'{baseType}는 올바른 타입이 아닙니다.')

def pureSchema(dataType):
    converted = extractor.groupType(dataType)
    if converted:
        return f"({pureSchema(converted)})"

    isKey = validator.isPrimaryKey(dataType)

    if validator.isRelation(dataType):
        dataType = remove(extractor.relationshipType(dataType), nullable=False)
        if isKey:
            dataType = CUSTOM_KEYWORDS['key'] + dataType

    if dataType == 'dsl':
        dataType = 'List<Dsl>'
        
    if validator.isInteger(dataType):
        dataType = 'int'

    converted = extractor.arrayType(dataType)
    if converted:
        converted = pureSchema(converted)
        dataType = f'List<{converted}>'

    if isKey:
        return CUSTOM_KEYWORDS['key'] + remove(dataType)
    else:
        return dataType

def pureSchemaSet(schemaSet):
    convertedSet = []
    for x in schemaSet:
        name, dataType, usage = x['name'], x['type'], x['usage']
        convertedSet.append({'name': name, 'type': pureSchema(dataType), 'usage': usage})
    return convertedSet

def pureSchemaDict():
    result = {}
    for name, schemaSet in resources.schemaDict.items():
        result[name] = pureSchemaSet(schemaSet)

    return result

def convert(usage, name):
    schemaSet, dataSet = resources.pureSchemaDict[name], resources.dataDict[name]
    schemaSet = {x['name']: x for x in schemaSet}
    result = []
    for data in dataSet:
        dataBuffer = {}
        for name, value in data.items():
            if schemaSet[name]['usage'] not in (usage, 'common'):
                continue

            dataBuffer[name] = cast(schemaSet[name]['type'], value)
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

def const():
    for dataSet in resources.dataDict.values():
        for data in dataSet:
            for columnName, value in data.items():
                for constTableName, constValue in extractor.constValue(value):
                    data[columnName] = data[columnName].replace(f'Const:{constTableName}:{constValue}', str(resources.constDict[constTableName][constValue]['value']))