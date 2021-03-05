#-*- encoding: utf-8 -*-

import extractor
import converter
import logger
import config

def supportedType(type, schemaDict, enumDict):
    groupType = extractor.groupType(type)
    if groupType:
        return supportedType(groupType, schemaDict, enumDict)

    type = converter.remove(type, relation=False, percentage=False, unknown=False)
    if isRelation(type):
        type = extractor.relationshipType(type, schemaDict)
    type = converter.pureSchema(type, schemaDict)

    groupType = extractor.groupType(type)
    if groupType:
        return supportedType(groupType, schemaDict, enumDict)

    if any([type.startswith(x) for x in config.customs.values()]):
        return True

    if type in enumDict:
        return True

    if type in config.primitives:
        return True

    listType = extractor.listType(type)
    if listType:
        return supportedType(listType, schemaDict, enumDict)

    return False

def supportedTypeConst(constDict, schemaDict, enumDict):
    for constName, constSet in constDict.items():
        logger.currentColumnName(constName)
        for name, x in constSet.items():
            logger.currentColumnName(name)
            if not supportedType(x['type'], schemaDict, enumDict):
                raise Exception(f"{x['type']}은 지원되지 않는 타입입니다.")


def supportedTypeDict(schemaDict, enumDict):
    for name, schemaSet in schemaDict.items():
        logger.currentTableName(name)

        for x in schemaSet:
            logger.currentColumnName(x['name'])

            if not supportedType(x['type'], schemaDict, enumDict):
                raise Exception(f"{x['type']}은 지원되지 않는 타입입니다.")

def multipleDefinedIndex(schemaDict):
    for name, schemaSet in schemaDict.items():
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

def relationship(schemaSetDict):
    for schemaSet in schemaSetDict.values():
        relationTypeSet = [x['type'] for x in schemaSet if x['type'].startswith('$')]
        if not relationTypeSet:
            continue

        for relationType in relationTypeSet:
            try:
                extractor.relationshipType(relationType, schemaSetDict)
            except Exception as e:
                return str(e)

    return True

def dataType(pureSchemaSet, dataSet, schemaDict, enumDict):
    dataTypeSet = {x['name']: converter.remove(x['type'], nullable=False, relation=False) for x in pureSchemaSet}
    for data in dataSet:
        for name, value in data.items():
            logger.currentColumnName(name)
            converter.cast(dataTypeSet[name], value, schemaDict, enumDict)
    return True

def dataTypeDict(pureSchemaDict, dataDict, enumDict, callback=None):
    progress = 0
    size = len(pureSchemaDict)
    for name, schemaSet in pureSchemaDict.items():
        logger.currentTableName(name)

        dataType(schemaSet, dataDict[name], pureSchemaDict, enumDict)

        progress = progress + 1
        percentage = int((progress * 100) / size)

        if callback:
            callback(name, percentage)

def dataTypeConst(constDict, schemaDict, enumDict):
    for constName, constSet in constDict.items():
        logger.currentTableName(constName)

        for name, data in constSet.items():
            logger.currentColumnName(name)
            converter.cast(data['type'], data['value'], schemaDict, enumDict)

def isPrimaryKey(type):
    return type.startswith(config.customs['key'])

def isNullable(type):
    return type.endswith(config.customs['nullable'])

def isRelation(type):
    type = converter.remove(type, relation=False)
    return type.startswith(config.customs['relation'])

def isInteger(type):
    type = converter.remove(type, percentage=False, unknown=False)
    return type.startswith(config.customs['unknown']) or type.startswith(config.customs['percentage'])