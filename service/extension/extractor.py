import re

def extract(regex, code):
    result = re.search(regex, code)
    return {} if not result else result.groupdict()

def extract_all(regex, code):
    regex = re.compile(regex)
    return [{} if not x else x.groupdict() for x in regex.finditer(code)]

def contents(code):
    group = extract(r'(?P<contents>public struct (?P<name>\b\w+) : IFlatbufferObject\s?{[\s\S.]+};)', code)

    return group['name'], group['contents']

def root(code):
    COMMON_FLATBUFFER_TYPES = {'StringOffset': 'string', 'VectorOffset': 'List'}

    groups = extract(r'public static Offset<.+> Create(?P<name>.+)\(FlatBufferBuilder builder,\s*(?P<params>[\w\s=,().<>]*)\)', code)
    name = groups['name']

    pairs = re.finditer(r'(?P<type>[\w=,().<>]+) (?P<name>\b\w+) = .+', groups['params'])
    parameters = [{'type': x['type'], 'name': x['name'], 'pure name': x['name'].replace('Offset', '')} for x in pairs]
    for parameter in parameters:
        if parameter['type'] in COMMON_FLATBUFFER_TYPES:
            parameter['type'] = COMMON_FLATBUFFER_TYPES[parameter['type']]

        if parameter['type'] == 'List':
            groups = extract(rf"\spublic (?P<type>\b\w+) {parameter['pure name'].capitalize()}\(int j\)", code)
            parameter['type'] = f"List<{groups['type']}>"

        match = extract(r'Offset<(?P<type>\b\w+)>', parameter['type'])
        if match:
            parameter['type'] = match['type']

    return name, parameters