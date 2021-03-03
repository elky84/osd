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
    constDict = extractor.loadConsts(args.dir)

    # Enum 엑셀 파일 로드
    enumDict = extractor.loadEnums(args.dir)

    # 엑셀 파일 로드
    schemaDict, dataDict = extractor.loads(args.dir, onLoadExcelFile)

    # 키 검증
    validator.multipleDefinedIndex(schemaDict)

    # 지원 타입 검증
    validator.supportedTypeDict(schemaDict, enumDict)
    validator.supportedTypeConst(constDict, schemaDict, enumDict)
    
    # C# 타입 변환
    pureSchemaDict = converter.pureSchemaDict(schemaDict)

    # 데이터 타입 검증
    validator.dataTypeDict(pureSchemaDict, dataDict, enumDict)
    validator.dataTypeConst(constDict, pureSchemaDict, enumDict)

    # 파이썬 타입 변환
    dataSet = {x: {} for x in usages}
    for name in schemaDict:
        logger.currentTableName(name)

        for usage in usages:
            dataSet[usage][name] = converter.convert(usage, name, pureSchemaDict, dataDict, enumDict)

    # 관계 타입 검증
    for name in schemaDict:
        logger.currentTableName(name)

        for usage in usages:
            if dataSet[usage][name]:
                validator.conflictIndex(schemaDict[name], dataSet[usage][name])
        
    validator.relationship(schemaDict)


    # 딕셔너리 형태로 변경 가능하면 변경
    for name, schemaSet in schemaDict.items():
        logger.currentTableName(name)

        for usage in usages:
            if dataSet[usage][name]:
                dataSet[usage][name] = converter.toDictionary(schemaSet, dataSet[usage][name])

    for constName, constSet in constDict.items():
        for name, data in constSet.items():
            # 관계타입이라면 변경
            data['type'] = converter.pureSchema(data['type'], schemaDict)

            # const 타입이 enum인 경우 변경
            if data['type'] in enumDict:
                data['value'] = f"{data['type']}.{data['value']}"


    # C# 마스터데이터 코드 생성
    output = f'{args.out}/class'
    progress = 0
    size = len(pureSchemaDict)
    for name, schemaSet in pureSchemaDict.items():
        logger.currentTableName(name)

        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)
            code = generator.classStringify(name, schemaSet, enumDict, usage)
            if not code:
                continue

            with open(f'{output}/{usage}/{name}.cs', 'w', encoding='utf8') as f:
                f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {name}.cs 코드 생성')

    # C# Enum 코드 생성
    output = f'{args.out}/enum'
    progress = 0
    size = len(enumDict)
    
    os.makedirs(f'{output}/', exist_ok=True)
    for enumName, enumSet in enumDict.items():
        code = generator.enumStringify(enumName, enumSet)
        with open(f'{output}/{enumName}.cs', 'w', encoding='utf8') as f:
            f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {enumName}.cs 코드 생성')


    # C# const 코드 생성
    output = f'{args.out}/const'
    progress = 0
    size = len(constDict)

    for constName, constSet in constDict.items():
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)
            code = generator.constStringify(constName, constSet, usage)
            with open(f'{output}/{usage}/{constName}.cs', 'w', encoding='utf8') as f:
                f.write(code)

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {constName}.cs 코드 생성')




    # C# 테이블 바인딩 코드 생성
    output = f'{args.out}/bind'
    for usage in usages:
        os.makedirs(f'{output}/{usage}', exist_ok=True)
        code = generator.bindStringify(pureSchemaDict, usage)
        
        with open(f'{output}/{usage}/Table.cs', 'w', encoding='utf8') as f:
            f.write(code)

    # JSON 파일 생성
    output = f'{args.out}/json'
    progress = 0
    size = len(schemaDict)
    for usage in usages:
        os.makedirs(f'{output}/{usage}', exist_ok=True)

    for name in schemaDict:
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)

            if not dataSet[usage][name]:
                continue

            with open(f"{output}/{usage}/{name}.json", 'w', encoding='utf8') as f:
                f.write(json.dumps(dataSet[usage][name], indent=2, ensure_ascii=False))

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {name}.json 코드 생성')


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