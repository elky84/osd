#-*- encoding: utf-8 -*-

import extractor
import converter
import validator
import generator
import os
import json
import argparse
import shutil
import zlib
import logger
import sys
import config
from resources import resources


def onLoadExcelFile(name, percentage):
    print(f'[{percentage}%] {name} 파일을 읽었습니다.')

def onConvertPureType(name, percentage):
    print(f'[{percentage}%] {name}의 타입을 변환했습니다.')

def onValidDataType(name, percentage):
    print(f'[{percentage}%] {name}의 타입을 검증했습니다.')

def execute(args):
    if os.path.isdir(args.out):
        shutil.rmtree(args.out)

    usages = ('client', 'server')

    # Const 엑셀 파일 로드
    resources.constDict = extractor.loadConsts(args.dir)

    # Enum 엑셀 파일 로드
    resources.enumDict = extractor.loadEnums(args.dir)

    # 엑셀 파일 로드
    resources.schemaDict, resources.dataDict = extractor.loads(args.dir, onLoadExcelFile)

    # 데이터에 포함된 const 값 검증
    validator.const()

    # 키 검증
    validator.multipleDefinedIndex()

    # 지원 타입 검증
    validator.supportedTypeDict()
    validator.supportedTypeConst()

    # const 값 파싱
    converter.const()

    validator.linkedSet()

    validator.dslFunctions()
    
    # C# 타입 변환
    resources.pureSchemaDict = converter.pureSchemaDict()

    # 데이터 타입 검증
    validator.dataTypeDict()
    validator.dataTypeConst()

    # 파이썬 타입 변환
    dataSet = {x: {} for x in usages}
    for name in resources.schemaDict:
        logger.currentTableName(name)

        for usage in usages:
            dataSet[usage][name] = converter.convert(usage, name)

    # 관계 타입 검증
    for name in resources.schemaDict:
        logger.currentTableName(name)

        for usage in usages:
            if dataSet[usage][name]:
                validator.conflictIndex(resources.schemaDict[name], dataSet[usage][name])
        
    validator.relationship()


    # 딕셔너리 형태로 변경 가능하면 변경
    for name, schemaSet in resources.schemaDict.items():
        logger.currentTableName(name)

        for usage in usages:
            if dataSet[usage][name]:
                dataSet[usage][name] = converter.toDictionary(schemaSet, dataSet[usage][name])


    for constName, constSet in resources.constDict.items():
        for name, data in constSet.items():
            # 관계타입이라면 변경
            data['type'] = converter.pureSchema(data['type'])

            # const 타입이 enum인 경우 변경
            if data['type'] in resources.enumDict:
                data['value'] = f"{data['type']}.{data['value']}"


    # C# 마스터데이터 코드 생성
    output = f'{args.out}/class'
    progress = 0
    size = len(resources.pureSchemaDict)
    for name, schemaSet in resources.pureSchemaDict.items():
        name = name.split('_')[1] if '_' in name else name
        logger.currentTableName(name)

        for usage in ('common', 'server', 'client'):
            os.makedirs(f'{output}/{usage}', exist_ok=True)
            code = generator.binaryClassStringify(name, usage)
            if not code:
                continue

            with open(f'{output}/{usage}/{name}.cs', 'w', encoding='utf8') as f:
                f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {name}.cs 코드 생성')

    # C# const 코드 생성
    output = f'{args.out}/const'
    progress = 0
    size = len(resources.constDict)

    for constName, constSet in resources.constDict.items():
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)
            code = generator.constStringify(constName, constSet, usage)
            with open(f'{output}/{usage}/{constName}.cs', 'w', encoding='utf8') as f:
                f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {constName}.cs 코드 생성')


    # C# Enum 코드 생성
    output = f'{args.out}/enum'
    progress = 0
    size = len(resources.enumDict)
    
    os.makedirs(f'{output}/', exist_ok=True)
    for enumName, enumSet in resources.enumDict.items():
        flags = enumName.startswith('f_')
        enumName = enumName[2:] if flags else enumName
        code = generator.enumStringify(enumName, enumSet, flags)
        with open(f'{output}/{enumName}.cs', 'w', encoding='utf8') as f:
            f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {enumName}.cs 코드 생성')

    # DSL 타입 Enum 코드 생성
    enumName = "DslFunctionType"
    enumSet = {x: None for x in config.dsl}
    code = generator.enumStringify(enumName, enumSet, False)
    with open(f'{output}/{enumName}.cs', 'w', encoding='utf8') as f:
        f.write(code)

    # DSL 파라미터 클래스 코드 생성
    output = f'{args.out}/dsl'
    progress = 0
    size = len(config.dsl)

    os.makedirs(f'{output}/', exist_ok=True)
    for funcName, parameters in config.dsl.items():
        code = generator.dslParametersStringify(funcName, parameters)
        with open(f'{output}/{funcName}.cs', 'w', encoding='utf8') as f:
            f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {funcName}.cs 코드 생성')



    # C# 테이블 바인딩 코드 생성
    output = f'{args.out}/bind'
    for usage in usages:
        os.makedirs(f'{output}/{usage}', exist_ok=True)
        code = generator.bindStringify(usage)
        
        with open(f'{output}/{usage}/Table.cs', 'w', encoding='utf8') as f:
            f.write(code)

    # JSON 파일 생성
    output = f'{args.out}/json'
    progress = 0
    size = len(resources.schemaDict)
    for usage in usages:
        os.makedirs(f'{output}/{usage}', exist_ok=True)

    for name in resources.schemaDict:
        fileName = name.split('_')[1] if '_' in name else name
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)

            if not dataSet[usage][name]:
                continue

            with open(f"{output}/{usage}/{fileName}.json", 'w', encoding='utf8') as f:
                f.write(json.dumps(dataSet[usage][name], indent=2, ensure_ascii=False))

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {fileName}.json 코드 생성')


    crc32Table = {'client': {}, 'server': {}}
    # 엑셀 파일 경로에 있는 json 파일은 클라/서버 공용으로 사용됨
    for file in [x for x in os.listdir(args.dir) if x.endswith('.json')]:
        with open(os.path.join(args.dir, file), 'rb+') as f:
            data = f.read()
            for usage in usages:
                crc32Table[usage][file] = zlib.crc32(data)

    
    # JSON 파일 CRC 계산
    for usage in usages:
        root = f'{output}/{usage}'
        for file in [x for x in os.listdir(root) if x.endswith('.json')]:
            with open(os.path.join(root, file), 'rb+') as f:
                data = f.read()
                crc32Table[usage][file] = zlib.crc32(data)

        with open(f'{root}/Crc.txt', 'w', encoding='utf8') as f:
            f.write(json.dumps(crc32Table[usage], ensure_ascii=False))


    # Resolver
    output = f'{args.out}/resolver'
    os.makedirs(output, exist_ok=True)
    with open(os.path.join(output, 'CustomMessagePackResolver.cs'), 'w', encoding='utf8') as f:
        f.write(generator.resolverStringify('client'))

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Excel table converter')
    parser.add_argument('--dir', default='../')
    parser.add_argument('--out', default='output')
    parser.add_argument('--debug', default='true')
    args = parser.parse_args()

    if args.debug.lower() == 'true':
        execute(args)
    else:
        try:
            os.system('color')
            execute(args)
            sys.exit(0)
        except Exception as e:
            logger.error(e)