import extractor
import jinja2

def constructor(name, type, param, method_dict):
    if type == 'string':
        return f'builder.CreateString({param})'

    group = extractor.extract(r'List<(?P<type>\b\w+)>', type)
    if group:
        code = constructor(None, group['type'], 'x', None)
        if code:
            code = f".Select(x => {code})"
        return f'Create{name.capitalize()}Vector(builder, {name}{code}.ToArray())'

    if type in method_dict:
        arguments = ', '.join([f"{name}.{x['name'].capitalize()}" for x in method_dict[type]])
        return f'FlatBuffers.Protocol.{type}.Create{type}(builder, {arguments})'

    return None

def property_str(parameters, method_dict):
    result = []
    for x in parameters:
        type = x['type']
        if type in method_dict:
            type = f'FlatBuffers.Protocol.{type}.Model'

        name = x['name'].lower()
        if name != 'offset' and name.endswith('offset'):
            name = name.replace('offset', '')

        result.append(f"  public {type} {name.capitalize()} {{ get; set; }}")

    return '\n'.join(result)

def binding_str(parameters):
    result = []
    for x in parameters:
        name = x['name'].lower()
        if name != 'offset' and name.endswith('offset'):
            name = name.replace('offset', '')

        result.append(f"    {name.capitalize()} = {name};")

    return '\n'.join(result)

def parameter_str(parameters, method_dict):
    result = []
    for x in parameters:
        type = f"FlatBuffers.Protocol.{x['type']}.Model" if x['type'] in method_dict else x['type']
        result.append(f"{type} {x['pure name']}")
    return ', '.join(result)

def argument_str(parameters):
    return ', '.join([f"{x['name']}" for x in parameters])

def offset_code(parameters, method_dict):
    offsets = [(x, constructor(x['pure name'], x['type'], x['pure name'], method_dict)) for x in parameters]
    offsets = [f"  var {x['name']} = {code};" for (x, code) in offsets if code]
    offsets = '\n'.join(offsets)

    return offsets