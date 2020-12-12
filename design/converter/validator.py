import extractor
import converter

def supportedType(type, enumDict):
    PRIMITIVE_TYPES = ['int', 'double', 'float', 'string', 'bool', 'DateTime', 'TimeSpan']
    
    type = type.replace('*', '').replace('?', '')

    for customKeyword in converter.CUSTOM_KEYWORDS:
        if type.startswith(customKeyword):
            return True

    if type in enumDict:
        return True

    return type in PRIMITIVE_TYPES

def supportedTypeDict(schemaDict, enumDict):
    for name, schema in schemaDict.items():
        for type in [x['type'] for x in schema]:
            if not supportedType(type, enumDict):
                raise f'{type} not supported. ({name})'

def conflictIndex(schemaSet, dataSet):
    id = extractor.primary(schemaSet)
    if not id:
        return True

    id = id['name']
    dataBuffer = {}
    for data in dataSet:
        if data[id] in dataBuffer:
            return 'asd'

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

def dataType(pureSchemaSet, dataSet, enumDict):
    dataTypeSet = {x['name']: x['type'].replace('*', '') for x in pureSchemaSet}
    for data in dataSet:
        for name, value in data.items():
            try:
                converter.cast(dataTypeSet[name], value, enumDict)
            except Exception as e:
                return str(e)
    return True

def dataTypeDict(pureSchemaDict, dataDict, enumDict, callback=None):
    progress = 0
    size = len(pureSchemaDict)
    for name, schemaSet in pureSchemaDict.items():
        dataType(schemaSet, dataDict[name], enumDict)

        progress = progress + 1
        percentage = int((progress * 100) / size)

        if callback:
            callback(name, percentage)