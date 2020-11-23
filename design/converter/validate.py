import openpyxl

KEYWORDS = ['abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char', 'checked', 'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do', 'double', 'else', 'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed', 'float', 'for', 'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal', 'is', 'lock', 'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override', 'params', 'private', 'protected', 'public',
            'readonly', 'ref', 'return', 'sbyte', 'sealed', 'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch', 'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe', 'ushort', 'using', 'virtual', 'void', 'volatile', 'while', 'add', 'alias', 'ascending', 'async', 'await', 'by', 'descending', 'dynamic', 'equals', 'from', 'get', 'global', 'group', 'into', 'join', 'let', 'nameof', 'notnull', 'on', 'orderby', 'partial', 'remove', 'select', 'set']


def assert_string(x):
    return x


def assert_double(x):
    return float(x)


def assert_number(x):
    return int(x)


def assert_unumber(x):
    x = int(x)
    if x < 0:
        raise Exception()
    return x


def assert_bool(x):
    if type(x) == bool:
        return x

    if x.lower() == 'true':
        return True

    if x.lower() == 'false':
        return False

    raise Exception()


PRIMITIVE_CONVERTER = {
    'string': assert_string,
    'bool': assert_bool,
    'double': assert_double,
    'float': assert_double,
    'int': assert_number,
    'uint': assert_unumber
}


def assert_header(name, type, dest):
    if not name:
        raise Exception(f'name cannot be empty')

    if name in KEYWORDS:
        raise Exception(f'name cannot be {name}')

    if not type:
        raise Exception(f'type cannot be empty')

    if type.endswith('?'):
        type = type[:-1]
    if type not in PRIMITIVE_CONVERTER:
        raise Exception(f'type cannot be {type}. it is not primitive type.')

    if not dest:
        raise Exception(f'dest cannot be empty')

    if dest not in ['server', 'client', 'common']:
        raise Exception(
            f'dest cannot be {dest}. dest only allowed in [server, client, common]')


def assert_data(type, value):
    if type.endswith('?') and (value == None or value == ''):
        return None

    if type.endswith('?'):
        type = type[:-1]

    if type not in PRIMITIVE_CONVERTER:
        raise Exception('invalid primitive type.')

    try:
        return PRIMITIVE_CONVERTER[type](value)
    except:
        raise Exception(f'{value} cannot be {type} type.')
