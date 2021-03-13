#-*- encoding: utf-8 -*-

import extractor
import converter
import logger
import config
import re
from resources import resources

def supportedType(type):
    groupType = extractor.groupType(type)
    if groupType:
        return supportedType(groupType)

    type = converter.remove(type, relation=False, percentage=False, unknown=False)
    type = converter.pureSchema(type)

    if any([type.startswith(x) for x in converter.CUSTOM_KEYWORDS.values()]):
        return True

    if type in resources.enumDict:
        return True

    if type in config.primitives:
        return True

    listType = extractor.listType(type)
    if listType:
        return supportedType(listType)

    return False

def supportedTypeConst():
    for constName, constSet in resources.constDict.items():
        logger.currentColumnName(constName)
        for name, x in constSet.items():
            logger.currentColumnName(name)
            if not supportedType(x['type']):
                raise Exception(f"{x['type']}은 지원되지 않는 타입입니다.")

def supportedTypeDict():
    for name, schemaSet in resources.schemaDict.items():
        logger.currentTableName(name)

        for x in schemaSet:
            logger.currentColumnName(x['name'])

            if not supportedType(x['type']):
                raise Exception(f"{x['type']}은 지원되지 않는 타입입니다.")

def multipleDefinedIndex():
    for name, schemaSet in resources.schemaDict.items():
        logger.currentTableName(name)
        primaryKey = extractor.primaryKey(schemaSet)
        indexKey = extractor.indexKey(schemaSet)

        if primaryKey and indexKey:
            stringify = ', '.join([x['name'] for x in (primaryKey, indexKey)])
            raise Exception(f'기본키와 그룹키는 중복으로 사용할 수 없습니다. ({stringify})')

def conflictIndex(schemaSet, dataSet):
    if not schemaSet or not dataSet:
        return True

    id = extractor.primaryKey(schemaSet)
    if not id:
        return True

    id = id['name']
    dataBuffer = {}
    for data in dataSet:
        if data[id] in dataBuffer:
            raise Exception(f"키({id})의 값 {data[id]}가 중복됩니다.")

        dataBuffer[data[id]] = data

    return True

def relationship():
    for schemaSet in resources.schemaDict.values():
        relationTypeSet = [x['type'] for x in schemaSet if x['type'].startswith('$')]
        if not relationTypeSet:
            continue

        for relationType in relationTypeSet:
            try:
                extractor.relationshipType(relationType)
            except Exception as e:
                return str(e)

    return True

def dataType(pureSchemaSet, dataSet):
    dataTypeSet = {x['name']: converter.remove(x['type'], nullable=False, relation=False) for x in pureSchemaSet}
    for data in dataSet:
        for name, value in data.items():
            logger.currentColumnName(name)
            converter.cast(dataTypeSet[name], value)
    return True

def dataTypeDict(callback=None):
    progress = 0
    size = len(resources.pureSchemaDict)
    for name, schemaSet in resources.pureSchemaDict.items():
        logger.currentTableName(name)

        dataType(schemaSet, resources.dataDict[name])

        progress = progress + 1
        percentage = int((progress * 100) / size)

        if callback:
            callback(name, percentage)

def dataTypeConst():
    for constName, constSet in resources.constDict.items():
        logger.currentTableName(constName)

        for name, data in constSet.items():
            logger.currentColumnName(name)
            converter.cast(data['type'], data['value'])

def linked(dataType, values):
    dataType = converter.remove(dataType, nullable=False, key=True, relation=False)

    if not isRelation(dataType):
        return

    tableName = converter.remove(dataType, relation=True)
    if '.' in tableName:
        tableName, key = tableName.split('.')
    else:
        key = (extractor.primaryKey(resources.schemaDict[tableName]) or extractor.indexKey(resources.schemaDict[tableName]))['name']
    idList = [x[key] for x in resources.dataDict[tableName]]

    if type(values) is not list:
        values = [values]

    for value in values:
        if not value:
            if isNullable(dataType) or isNullable(converter.pureSchema(dataType)):
                continue

        if value not in idList:
            raise Exception(f"{value}는 {tableName} 테이블에 존재하는 데이터가 아닙니다.")

def linkedSet():
    for name in resources.schemaDict:
        logger.currentTableName(name)
        for schema in resources.schemaDict[name]:
            logger.currentColumnName(schema['name'])

            values = [x[schema['name']] for x in resources.dataDict[name]]
            linked(schema['type'], values)

def const():
    for name, dataSet in resources.dataDict.items():
        logger.currentTableName(name)

        for data in dataSet:
            for columnName, value in data.items():
                logger.currentColumnName(columnName)
                if not value:
                    continue

                for constTableName, constValue in extractor.constValue(value):
                    if constTableName not in resources.constDict:
                        raise Exception(f"{constTableName}은 Const 테이블에 정의되지 않았습니다.")

                    if constValue not in resources.constDict[constTableName]:
                        raise Exception(f"{constValue}는 {constTableName}에 정의된 값이 아닙니다.")

def dslFunction(dsl):
    if not dsl:
        return

    dsl = dsl.replace(' ', '')
    dslList = re.split('&|\n', dsl)
    for dsl in dslList:
        matched = re.match(config.regex['dsl'], dsl)
        if not matched:
            raise Exception(f'{dsl}은 올바른 DSL 구문이 아닙니다.')

        groups = matched.groupdict()
        header, parameters = groups['header'], groups['parameters']
        if header not in config.dsl:
            raise Exception(f'{header}는 지원하지 않는 DSL 포맷입니다. ({dsl})')

        parameters = [x.strip() for x in parameters.split(',')]
        if len(parameters) < len(config.dsl[header]):
            for i in range(len(config.dsl[header])):
                if len(parameters) > i:
                    continue

                if 'default' in config.dsl[header][i]:
                    parameters.append(config.dsl[header][i]['default'])

        if len(parameters) != len(config.dsl[header]):
            message = f"{header}({', '.join([x['desc'] for x in config.dsl[header]])})"
            raise Exception(f'{header}의 파라미터 갯수가 잘못되었습니다. ({dsl})\n올바른 문법 : {message}')

        for i in range(len(parameters)):
            dataType = config.dsl[header][i]['type']
            value = parameters[i]

            try:
                converter.cast(dataType, value)
            except:
                raise Exception(f"{header}의 형식이 올바르지 않습니다. {value}는 {dataType}로 변환될 수 없습니다. ({dsl})")

            try:
                if type(value) is str and '{' in value and '}' in value:
                    value = None
                linked(dataType, value)
            except:
                raise Exception(f"{header} 파라미터 중 {value}는 {converter.remove(dataType)} 테이블에 존재하지 않는 값입니다. ({dsl})")

def dslFunctions():
    for name, schemaSet in resources.schemaDict.items():
        logger.currentTableName(name)

        for schema in schemaSet:
            columnName = schema['name']
            logger.currentColumnName(columnName)
            if schema['type'] != 'dsl':
                continue

            for data in resources.dataDict[name]:
                dslFunction(data[columnName])

def isPrimaryKey(type):
    return type.startswith(converter.CUSTOM_KEYWORDS['key'])

def isNullable(type):
    if type.endswith(converter.CUSTOM_KEYWORDS['nullable']):
        return True

    if type == 'string':
        return True

    return False

def isRelation(type):
    type = converter.remove(type, relation=False)
    return type.startswith(converter.CUSTOM_KEYWORDS['relation'])

def isInteger(type):
    type = converter.remove(type, percentage=False, unknown=False)
    return type.startswith(converter.CUSTOM_KEYWORDS['unknown']) or type.startswith(converter.CUSTOM_KEYWORDS['percentage'])