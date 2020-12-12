import jinja2
import extractor
import converter

loader = jinja2.FileSystemLoader('templates')
env = jinja2.Environment(loader=loader)
templateClass = env.get_template('class.txt')
templateBindFormat = env.get_template('bind_format.txt')
templateBind = env.get_template('bind.txt')
templateEnumFormat = env.get_template('enum_format.txt')
templateEnum = env.get_template('enum.txt')

def classStringify(name, pureSchemaSet, enumDict, usage):
    properties = []
    for schema in pureSchemaSet:
        if schema['usage'] not in ('common', usage):
            continue

        if schema['type'].startswith('*'):
            properties.append('[Key]')

        type = converter.removeKeyword(schema['type'].replace('*', ''))
        baseType = type.replace('?', '')
        if baseType in enumDict:
            properties.append(f'[JsonConverter(typeof(JsonEnumConverter<{baseType}>))]')

        properties.append(f"public {type} {schema['name'].capitalize()} {{ get; set; }}")

    properties = [x if x == '' else f"    {x}" for x in properties]
    return templateClass.render({'name': name.capitalize(), 'properties': '\n'.join(properties)})


def bindFormatStringify(name, pureSchemaSet):
    id = extractor.primary(pureSchemaSet)
    if not id:
        tableType = 'BaseList'
        generic = f"<{name}>"
    else:
        tableType = 'BaseDict'
        generic = f"<{id['type'].replace('*', '')}, {name.capitalize()}>"

    return templateBindFormat.render({'name': name, 'tableType': tableType, 'generic': generic})

def bindStringify(pureSchemaDict, usage):
    codeLines = []
    for name, schemaSet in pureSchemaDict.items():
        if not [x for x in schemaSet if x['usage'] in ('common', usage)]:
            continue

        for line in bindFormatStringify(name, schemaSet).split('\n'):
            codeLines.append(line)
        
    codeLines = [x if x == '' else f"    {x}" for x in codeLines]
    codeLines = '\n'.join(codeLines)
    return templateBind.render({'usage': usage.capitalize(), 'codeLines': codeLines})

def enumFormatStringify(value, desc):
    return templateEnumFormat.render({'desc': desc, 'value': value})

def enumStringify(enumName, enumSet):
    codeLines = []
    for name, desc in enumSet.items():
        for line in enumFormatStringify(name, desc).split('\n'):
            codeLines.append(line)

    codeLines = [x if x == '' else f'        {x}' for x in codeLines]
    return templateEnum.render({'name': enumName, 'codeLines': '\n'.join(codeLines)})\