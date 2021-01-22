import json

primitives = []
customs = {}
regex = {}

with open('config.json', 'rt', encoding='utf8') as f:
    configs = json.load(f)

    primitives = configs['primitive']
    customs = configs['custom']
    regex = configs["regex"]