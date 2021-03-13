import json

primitives = []
regex = {}
dsl = {}

with open('config.json', 'rt', encoding='utf8') as f:
    configs = json.load(f)

    primitives = configs['primitive_types']
    regex = configs['regex']
    dsl = configs['dsl']
    file = configs['file']