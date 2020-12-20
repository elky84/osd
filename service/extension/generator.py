import extractor
import jinja2
import re

def to_upper_camel(name):
    name = re.split(r'(?<!^)(?=[A-Z])', name)
    name[0] = name[0].capitalize()
    name = ''.join(name)

    return name

def to_lower_camel(name):
    return ''.join(re.split(r'(?<!^)(?=[A-Z])', name))

def convert_type(type, method_dict):
    match = extractor.extract(r'List<(?P<type>\b\w+)>', type)
    if match and match['type'] in method_dict:
        return f"List<FlatBuffers.Protocol.{match['type']}.Model>"
    elif type in method_dict:
        return f'FlatBuffers.Protocol.{type}.Model'
    else:
        return type
    
def constructor(name, type, param, method_dict):
    if type == 'string':
        return f'builder.CreateString({name})'

    group = extractor.extract(r'List<(?P<type>\b\w+)>', type)
    if group:
        code = constructor('x', group['type'], 'x', method_dict)
        if code:
            code = f".Select(x => {code})"
        return f"Create{to_upper_camel(name)}Vector(builder, {name}{'' if not code else code}.ToArray())"

    if method_dict is not None and type in method_dict:
        arguments = []
        for x in method_dict[type]:
            code = constructor(f"{name}.{to_upper_camel(x['pure name'])}", x['type'], 'x', method_dict)
            if code:
                arguments.append(code)
            else:
                arguments.append(f"{name}.{to_upper_camel(x['pure name'])}")
        arguments = ', '.join(arguments)
        return f'FlatBuffers.Protocol.{type}.Create{type}(builder, {arguments})'

    return None

def property_str(parameters, method_dict):
    result = []
    for x in parameters:
        type = convert_type(x['type'], method_dict)

        name = to_upper_camel(x['name'])
        if name != 'Offset' and name.endswith('Offset'):
            name = name.replace('Offset', '')

        result.append(f"  public {type} {name} {{ get; set; }}")

    return '\n'.join(result)

def binding_str(parameters):
    result = []
    for x in parameters:
        name = to_lower_camel(x['name'])
        if name != 'Offset' and name.endswith('Offset'):
            name = name.replace('Offset', '')

        result.append(f"    {to_upper_camel(name)} = {name};")

    return '\n'.join(result)

def parameter_str(parameters, method_dict):
    result = []
    for x in parameters:
        type = convert_type(x['type'], method_dict)
        result.append(f"{type} {x['pure name']}")
    return ', '.join(result)

def argument_str(parameters):
    return ', '.join([f"{x['name']}" for x in parameters])

def offset_code(parameters, method_dict):
    offsets = [(x, constructor(x['pure name'], x['type'], x['pure name'], method_dict)) for x in parameters]
    offsets = [f"  var {x['name']} = {code};" for (x, code) in offsets if code]
    offsets = '\n'.join(offsets)

    return offsets