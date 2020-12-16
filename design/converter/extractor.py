import openpyxl as Excel
import jinja2
import os

def primary(schemaSet):
    id = [x for x in schemaSet if x['type'].startswith('*')]
    if not id:
        return None
    else:
        return id[0]

def load(sheet):
    schema = []
    data = []
    commentColumns = []
    for row in sheet[1]:
        columnName = row.value
        if columnName is None or columnName.startswith('!'):
            break

        if columnName.startswith('#'):
            commentColumns.append(row.column)
            continue

        dtype = sheet[2][row.column-1].value
        usage = sheet[3][row.column-1].value

        schema.append({'name': columnName, 'type': dtype, 'usage': usage})

    
    data = []
    for row in range(sheet.max_row - 3):
        additionalColumn = 0
        dataSet = {}
        for column, columnName in enumerate([x['name'] for x in schema]):
            while column+additionalColumn+1 in commentColumns:
                additionalColumn = additionalColumn + 1

            columnName = schema[column]['name']
            dataSet[columnName] = sheet.cell(row+4, column+1+additionalColumn).value

        if all([x is None for x in dataSet.values()]):
            continue

        data.append(dataSet)

    return schema, data

def loadEnum(path):
    workbook = Excel.load_workbook(path, data_only=True)
    enumSet = {}
    for sheet in workbook.worksheets:
        enumSet[sheet.title] = {}

        for row in range(sheet.max_row):
            name = sheet.cell(row+1, 1).value
            desc = sheet.cell(row+1, 2).value
            enumSet[sheet.title][name] = desc

    return enumSet

def loads(directory, callback=None):
    schemaDict = {}
    dataDict = {}
    files = [x for x in os.listdir(directory) if x.endswith('.xlsx') and not x.startswith('~') and not x.lower().startswith('enum.')]
    sheets = [sheet for file in files for sheet in Excel.load_workbook(os.path.join(directory, file), data_only=True).worksheets]
    size = len(sheets)
    progress = 0

    for sheet in [x for x in sheets if not x.title.startswith('#')]:
        schema, data = load(sheet)

        schemaDict[sheet.title] = schema
        dataDict[sheet.title] = data

        progress = progress + 1

        if callback:
            callback(sheet, int(progress * 100 / size))

    return schemaDict, dataDict

def loadEnums(directory, callback=None):
    files = [x for x in os.listdir(directory) if not x.startswith('~') and x.lower().startswith('enum.')]
    enumDict = {}
    for file in files:
        path = os.path.join(directory, file)

        for enumName, enumSet in loadEnum(path).items():
            if enumName in enumDict:
                raise 'zzasdqwe'
            enumDict[enumName] = enumSet

    return enumDict

def relationshipType(type, schemaSetDict):
    if not type.startswith('$'):
        return None

    schemaSetDict = {name.split('_')[1] if '_' in name else name: schemaSet for name, schemaSet in schemaSetDict.items()}
    type = type.replace('$', '').replace('?', '')
    splitted = type.split('.')
    if len(splitted) == 1:
        if type not in schemaSetDict:
            raise Exception(f'{type} is not contained any excel schema.')

        id = primary(schemaSetDict[type])
        if not id:
            raise Exception(f'{type} does not have primary key.')

        return id['type']
    elif len(splitted) == 2:
        namespace, member = splitted
        if namespace not in schemaSetDict:
            raise Exception(f'{namespace} is not valid schema.')

        member = [x for x in schemaSetDict[namespace] if x['name'] == member]
        if not member:
            raise Exception(f'{member} is not a member of {namespace}')

        member = member[0]
        return member['type']
    else:
        raise Exception(f'{type} cannot parse relationship type.')