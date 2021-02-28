import jinja2
import extractor
import converter
import os

def loadTemplateFiles(root):
    loader = jinja2.FileSystemLoader(root)
    env = jinja2.Environment(loader=loader)

    return {os.path.splitext(x)[0]:env.get_template(x) for x in os.listdir(root) if x.endswith('.txt')}

templates = loadTemplateFiles('templates')

def classStringify(name, pureSchemaSet, enumDict, usage):
    properties = []
    key = extractor.primaryKey(pureSchemaSet) or extractor.indexKey(pureSchemaSet)
    for schema in pureSchemaSet:
        if schema['usage'] not in ('common', usage):
            continue

        if key and schema['name'] == key['name']:
            properties.append('[Key]')

        baseType = converter.remove(schema['type'])
        if baseType in enumDict:
            properties.append('[JsonConverter(typeof(StringEnumConverter))]')

        type = converter.remove(schema['type'], nullable=False, key=True)
        properties.append(f"public {type} {schema['name']} {{ get; set; }}\n")

    properties = [x if x == '' else f"        {x}" for x in properties]
    if not properties:
        return None

    return templates['class'].render({'usage': usage.capitalize(), 'name': name, 'properties': '\n'.join(properties)}) + '\n'

def bindFormatStringify(name, pureSchemaSet):
    primary = extractor.primaryKey(pureSchemaSet)
    index = extractor.indexKey(pureSchemaSet)
    if primary:
        tableType = 'BaseDict'
        generic = f"<{converter.remove(primary['type'], nullable=False, key=True)}, {name}>"

    elif index:
        tableType = 'BaseDict'
        generic = f"<{converter.remove(index['type'], nullable=False, key=True)}, List<{name}>>"
        
    else:
        tableType = 'BaseList'
        generic = f"<{name}>"

    return templates['bind_format'].render({'name': name, 'tableType': tableType, 'generic': generic}) + '\n'

def bindStringify(pureSchemaDict, usage):
    codeLines = []
    for name, schemaSet in pureSchemaDict.items():
        if not [x for x in schemaSet if x['usage'] in ('common', usage)]:
            continue

        for line in bindFormatStringify(name, schemaSet).split('\n'):
            codeLines.append(line)
        
    codeLines = [x if x == '' else f'    {x}' for x in codeLines] if usage == 'server' else codeLines

    codeLines = '\n'.join(codeLines)
    return templates['bind'].render({'usage': usage.capitalize(), 'codeLines': codeLines})

def enumFormatStringify(value, desc):
    return templates['enum_format'].render({'value': value, 'desc': desc}) + '\n'

def enumStringify(enumName, enumSet):
    codeLines = []
    for name, desc in enumSet.items():
        for line in enumFormatStringify(name, desc).split('\n'):
            codeLines.append(line)

    codeLines = [x if x == '' else f'        {x}' for x in codeLines]
    return templates['enum'].render({'name': enumName, 'codeLines': '\n'.join(codeLines)}) + '\n'