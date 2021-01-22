import sys
from termcolor import colored, cprint

CURRENT_TABLE_NAME = None
CURRENT_COLUMN_NAME = None

def currentTableName(name):
    global CURRENT_TABLE_NAME
    global CURRENT_COLUMN_NAME

    CURRENT_TABLE_NAME = name
    CURRENT_COLUMN_NAME = None

def currentColumnName(name):
    global CURRENT_COLUMN_NAME

    CURRENT_COLUMN_NAME = name

def error(message):
    global CURRENT_TABLE_NAME
    global CURRENT_COLUMN_NAME

    if type(message) is not str:
        message = str(message)

    if not CURRENT_COLUMN_NAME:
        cprint(f'[{CURRENT_TABLE_NAME}] {message}', 'red')
    else:
        cprint(f'[{CURRENT_TABLE_NAME}:{CURRENT_COLUMN_NAME}] {message}', 'red')
    sys.exit(1)