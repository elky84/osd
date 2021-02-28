import os
import shutil
import re
import jinja2
import argparse
import extractor
import generator

METHOD_DICT = {}

def model(namespace, parameters):
    global METHOD_DICT

    loader = jinja2.FileSystemLoader('templates')
    env = jinja2.Environment(loader=loader)
    model_template = env.get_template('model.txt')

    code = model_template.render({'properties': generator.property_str(namespace, parameters, METHOD_DICT),
                                  'parameters': generator.parameter_str(namespace, parameters, METHOD_DICT),
                                  'binding': generator.binding_str(parameters)})
    
    code = code.split('\n')
    code = [f'  {x}' for x in code]
    code = '\n'.join(code)
    return code

def method(namespace, name, parameters):
    global METHOD_DICT

    loader = jinja2.FileSystemLoader('templates')
    env = jinja2.Environment(loader=loader)

    model_arguments = ', '.join([f"model.{generator.to_upper_camel(x['pure name'])}" for x in parameters])

    if parameters:
        template = env.get_template('method.txt')
        code = template.render({'flatbName': name,
                                'parameters': generator.parameter_str(namespace, parameters, METHOD_DICT),
                                'arguments': generator.argument_str(parameters),
                                'offsets': generator.offset_code(namespace, name, parameters, METHOD_DICT),
                                'modelArguments': model_arguments})
    else:
        template = env.get_template('method_without_params.txt')
        code = template.render({'flatbName': name})

    code = code.split('\n')
    code = [f'  {x}' for x in code]
    code = '\n'.join(code)
    return code


if __name__ == '__main__':
    loader = jinja2.FileSystemLoader('templates')
    env = jinja2.Environment(loader=loader)
    flatbuffer_template = env.get_template('flatbuffer.txt')

    parser = argparse.ArgumentParser(description='Excel table converter')
    parser.add_argument('--dir', default='../Protocols/Request')
    parser.add_argument('--namespace', default='FlatBuffers.Protocol.Unknown')
    args = parser.parse_args()

    os.makedirs(args.dir, exist_ok=True)

    files = [os.path.join(args.dir, f) for f in os.listdir(args.dir) if os.path.isfile(os.path.join(args.dir, f)) and f.endswith('.cs')]
    code_dict = {}
    METHOD_DICT = {}
    for file in files:
        with open(file, 'r', encoding='utf8') as f:
            _, contents = extractor.contents(f.read())
            code_dict[file] = contents

    for file in code_dict:
        name, parameters = extractor.root(code_dict[file])
        METHOD_DICT[name] = parameters

    for file in code_dict:
        data = code_dict[file]
        name = os.path.splitext(os.path.basename(file))[0]
        parameters = METHOD_DICT[name]

        model_code = model(args.namespace, parameters) if parameters else ''
        method_code = method(args.namespace, name, parameters)
        data = f"{data[:-3]}\n\n{model_code}\n\n{method_code}{data[-3:]}"
        data = '\n'.join([f'  {x}' for x in data.split('\n')])

        with open(file, 'w', encoding='utf8') as f:
            f.write(flatbuffer_template.render({'code': data, 'namespace': args.namespace}))
