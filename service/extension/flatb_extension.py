import os
import shutil
import re
import jinja2
import argparse


def method(code):
    loader = jinja2.FileSystemLoader('templates')
    env = jinja2.Environment(loader=loader)
    method_template = env.get_template('method.txt')

    result = re.search(
        r'public static Offset<.+> Create(?P<name>.+)\(FlatBufferBuilder builder,\s*(?P<params>[\w\s=,().]*)\)', code)
    group_dict = result.groupdict()
    name = group_dict['name']

    parsed_params = [{'type': x['type'], 'name': x['name']} for x in re.finditer(
        r'(?P<type>\b\w+) (?P<name>\b\w+) = .+', group_dict['params'])]

    parameters = ', '.join([
        f"{x['type']} {x['name']}" for x in parsed_params])
    parameters = parameters.replace('StringOffset', 'string')
    parameters = parameters.replace('Offset', '')

    arguments = ', '.join([f"{x['name']}" for x in parsed_params])

    params_str = [x['name']
                  for x in parsed_params if x['type'] == 'StringOffset']
    stringCodes = [
        f"  var {x} = builder.CreateString({x.replace('Offset', '')});" for x in params_str]
    stringCodes = '\n'.join(stringCodes)

    code = method_template.render({'flatbName': name,
                                   'flatbLowerName': name.lower(),
                                   'parameters': parameters,
                                   'arguments': arguments,
                                   'stringCodes': stringCodes})
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
                data = f"{data[:-4]}\n{code}\n{data[-4:]}"

            with open(file, 'w', encoding='utf8') as f:
                f.write(data)

    except Exception as e:
        print(str(e))
