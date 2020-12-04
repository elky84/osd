import os
import shutil
import re
import jinja2
import argparse

flat_type_dict = {'StringOffset': 'string'}

def convert_type(type):
    global flat_type_dict
    for f, c in flat_type_dict.items():
        type = type.replace(f, c)
    return type
    
def extract(regex, code):
    result = re.search(regex, code)
    return {} if not result else result.groupdict()

def extract_all(regex, code):
    regex = re.compile(regex)
    return [{} if not x else x.groupdict() for x in regex.finditer(code)]

def allocate_code(type, this, param):
    if type in offsetFuncDict:
        return offsetFuncDict[type](param)

    groups = extract(r'List<(?P<type>\b\w+)>', type)
    if groups:
        generic = groups['type']

        middle = ''
        if generic in offsetFuncDict:
            middle = f".Select(x => {offsetFuncDict[generic]('x')})"
        return f'Create{this.capitalize()}Vector(builder, {this}{middle}.ToArray())'

def createStringCode(param):
    return f"builder.CreateString({param})"

offsetFuncDict = {'string': createStringCode}


def method(code):
    matchedTypeDict = {'String': 'string', 'Vector': 'List'}

    loader = jinja2.FileSystemLoader('templates')
    env = jinja2.Environment(loader=loader)
    method_template = env.get_template('method.txt')

    groups = extract_all(r'Create(?P<name>(\w(?!Block))+)\(FlatBufferBuilder builder, (?P<params>[a-zA-Z_\[\]]+).+\) {', code)
    for x in groups:
        params = [convert_type(x) for x in x['params'].split(', ')]
        x['params'] = ', '.join(params)

    groups = extract(r'public static Offset<.+> Create(?P<name>.+)\(FlatBufferBuilder builder,\s*(?P<params>[\w\s=,().]*)\)', code)
    name = groups['name']

    parsed_params = [{'type': x['type'], 'name': x['name'], 'pure name': x['name'].replace('Offset', '')} for x in re.finditer(r'(?P<type>\b\w+) (?P<name>\b\w+) = .+', groups['params'])]
    for parsed in parsed_params:
        parsed['type'] = parsed['type'].replace('Offset', '')
        if parsed['type'] in matchedTypeDict:
            parsed['type'] = matchedTypeDict[parsed['type']]

        if parsed['type'] == 'List':
            groups = extract(rf"\spublic (?P<type>\b\w+) {parsed['pure name'].capitalize()}\(int j\)", code)
            parsed['type'] = f"List<{groups['type']}>"


    parameters = ', '.join([f"{x['type']} {x['pure name']}" for x in parsed_params])

    arguments = ', '.join([f"{x['name']}" for x in parsed_params])

    offsets = [(x, allocate_code(x['type'], x['pure name'], x['pure name'])) for x in parsed_params]
    offsets = [f"  var {x['name']} = {code};" for (x, code) in offsets if code]
    offsets = '\n'.join(offsets)

    code = method_template.render({'flatbName': name,
                                   'flatbLowerName': name.lower(),
                                   'parameters': parameters,
                                   'arguments': arguments,
                                   'offsets': offsets})
    code = code.split('\n')
    code = [f'  {x}' for x in code]
    code = '\n'.join(code)
    return code


if __name__ == '__main__':
    try:
        parser = argparse.ArgumentParser(description='Excel table converter')
        parser.add_argument('--dir', default='../Protocols')
        args = parser.parse_args()

        os.makedirs(args.dir, exist_ok=True)

        for file in [os.path.join(args.dir, f) for f in os.listdir(args.dir) if os.path.isfile(os.path.join(args.dir, f)) and f.endswith('.cs')]:

            data = None
            with open(file, 'r', encoding='utf8') as f:
                data = f.read()
                code = method(data)
                data = f"using System.Linq;\n{data[:-4]}\n{code}\n{data[-4:]}"

            with open(file, 'w', encoding='utf8') as f:
                f.write(data)

    except Exception as e:
        print(str(e))
