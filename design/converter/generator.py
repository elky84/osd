import jinja2
import extractor
import converter
import os
from resources import resources

def loadTemplateFiles(root):
    loader = jinja2.FileSystemLoader(root)
    env = jinja2.Environment(loader=loader)

    return {os.path.splitext(x)[0]:env.get_template(x) for x in os.listdir(root) if x.endswith('.txt')}

templates = loadTemplateFiles('templates')

def binaryClassStringify(name, usage):
    pureSchemaSet = resources.pureSchemaDict[name]

    if usage != 'common' and not [x for x in pureSchemaSet if x['usage'] in ('common', usage)]:
        return None

    properties = []
    key = extractor.primaryKey(pureSchemaSet) or extractor.indexKey(pureSchemaSet)

    for i, schema in enumerate(pureSchemaSet):
        if schema['usage'] != usage:
            continue

        if key and schema['name'] == key['name']:
            properties.append('[NetworkShared.Util.Table.Key]')

        type = converter.remove(schema['type'], nullable=False, key=True)
        # properties.append(f"[MessagePack.Key({i})]")
        properties.append(f"public {type} {schema['name']} {{ get; set; }}\n")

    properties = [x if x == '' else f"        {x}" for x in properties]

    if usage != 'common':
        name = f'{name} : MasterData.Common.{name}'
    return templates['binary_class'].render({'usage': usage.capitalize(), 'name': name, 'properties': '\n'.join(properties)}) + '\n'


def bindFormatStringify(name):
    pureSchemaSet = resources.pureSchemaDict[name]
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

def bindStringify(usage):
    codeLines = []
    for name, schemaSet in resources.pureSchemaDict.items():
        if not [x for x in schemaSet if x['usage'] in ('common', usage)]:
            continue

        for line in bindFormatStringify(name).split('\n'):
            codeLines.append(line)
        
    template = templates['bind'] if usage == 'server' else templates['bind_client']
    codeLines = [x if x == '' else f'    {x}' for x in codeLines] if usage == 'server' else codeLines

    codeLines = '\n'.join(codeLines)
    return template.render({'usage': usage.capitalize(), 'codeLines': codeLines})

def enumFormatStringify(value, desc, flags, shift):
    if flags:
        lines = templates['f_enum_format'].render({'value': value, 'shift': shift}) + '\n'
    else:
        lines = templates['enum_format'].render({'value': value}) + '\n'
    return lines

def enumStringify(enumName, enumSet, flags):
    codeLines = []
    shift = 0
    for name, desc in enumSet.items():
        for line in enumFormatStringify(name, desc, flags, shift).split('\n'):
            codeLines.append(line)
        shift += 1

    codeLines = [x if x == '' else f'        {x}' for x in codeLines]

    template = 'enum' if not flags else 'f_enum'

    return templates[template].render({'name': enumName, 'codeLines': '\n'.join(codeLines)}) + '\n'

def constStringify(constName, constSet, usage):
    codeLines = []
    for name, data in constSet.items():
        if not data['usage'] in ('common', usage):
            continue

        if data['type'] == 'string':
            value = f"\"{data['value']}\""""
        else:
            value = data['value']

        codeLines.append(f"public static readonly {data['type']} {name} = {value};")

    codeLines = [f'        {x}' for x in codeLines]
    return templates['const'].render({'name': constName, 'code': '\n'.join(codeLines)}) + '\n'

def resolverStringify(usage):
    properties = []
    index = 0
    for name, schemaSet in resources.pureSchemaDict.items():
        if not [x for x in schemaSet if x['usage'] in (usage, 'common')]:
            continue

        # properties.append(f"[MessagePack.Key({index})]")
        primaryKey = extractor.primaryKey(schemaSet)
        indexKey = extractor.indexKey(schemaSet)
        if primaryKey:
            baseType = converter.remove(primaryKey['type'], nullable=True, key=True)
            properties.append(f"public Dictionary<{baseType}, {name}> {name} {{ get; set; }}")
        elif indexKey:
            baseType = converter.remove(indexKey['type'], nullable=True, key=True)
            properties.append(f"public Dictionary<{baseType}, List<{name}>> {name} {{ get; set; }}")
        else:
            properties.append(f"public List<{name}> {name} {{ get; set; }}")

        index = index + 1

    properties = [x if x == '' else f"    {x}" for x in properties]
    return templates['resolver'].render({'properties': '\n'.join(properties)})

def dslParametersStringify(name, parameters):
    properties = [f"        public {converter.pureSchema(x['type'])} {x['name']} {{ get; set; }}" for x in parameters]

    code = []
    for i in range(len(parameters)):
        parameter = parameters[i]
        pureType = converter.pureSchema(parameter['type'])
        if pureType in resources.enumDict:
            code.append(f"{parameter['name']} = ({pureType})System.Enum.Parse(typeof({pureType}), parameters[{i}].ToString())")
        elif pureType == 'string':
            code.append(f"{parameter['name']} = parameters[{i}]?.ToString()")
        else:
            code.append(f"{parameter['name']} = ({pureType})System.Convert.ChangeType(parameters[{i}], typeof({pureType}))")

    code = [f'                {x}' for x in code]
    code = ',\n'.join(code)

    static_constructor = f"""
        public static {name} Parse(List<object> parameters)
        {{
            return new {name}
            {{
{code}
            }};
        }}"""


    code = [x['name'] for x in parameters]
    code = [f"                    {x}" for x in code]
    code = ',\n'.join(code)
    dsl_constructor = f"""
        public Dsl ToDsl()
        {{
            return new Dsl
            {{
                Type = DslFunctionType.{name},
                Parameters = new List<object>
                {{
{code}
                }}
            }};
        }}
"""



    return templates['dsl_parameters'].render({'name': name, 'properties': '\n'.join(properties), 'static_constructor': static_constructor, 'dsl_constructor': dsl_constructor})