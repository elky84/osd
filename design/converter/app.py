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


def onLoadExcelFile(sheet, percentage):
    print(f'[{percentage}%] {sheet.title} 엑셀 시트를 읽었습니다.')

def onConvertPureType(name, percentage):
    print(f'[{percentage}%] {name}의 타입을 변환했습니다.')

def onValidDataType(name, percentage):
    print(f'[{percentage}%] {name}의 타입을 검증했습니다.')

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='Excel table converter')
    parser.add_argument('--dir', default='../')
    parser.add_argument('--out', default='output')
    args = parser.parse_args()

    if os.path.isdir(args.out):
        shutil.rmtree(args.out)

    usages = ('client', 'server')

    # Enum 엑셀 파일 로드
    enumDict = extractor.loadEnums(args.dir)

    # 엑셀 파일 로드
    schemaDict, dataDict = extractor.loads(args.dir, onLoadExcelFile)

    # 지원 타입 검증
    validator.supportedTypeDict(schemaDict, enumDict)
    
    # C# 타입 변환
    pureSchemaDict = converter.pureSchemaDict(schemaDict)

    # 데이터 타입 검증
    validator.dataTypeDict(pureSchemaDict, dataDict, enumDict)

    # 파이썬 타입 변환
    dataSet = {'server': {}, 'client': {}}
    for name, schema in schemaDict.items():
        for usage in usages:
            dataSet[usage][name] = converter.convert(usage, name, pureSchemaDict, dataDict, enumDict)

    # 관계 타입 검증
    validator.relationship(schemaDict)
    for name, schema in schemaDict.items():
        for usage in usages:
            if not dataSet[usage][name]:
                continue

            validator.conflictIndex(schemaDict[name], dataSet[usage][name])



    # C# 마스터데이터 코드 생성
    output = f'{args.out}/class'
    progress = 0
    size = len(pureSchemaDict)
    for name, schemaSet in pureSchemaDict.items():
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)
            code = generator.classStringify(name, schemaSet, enumDict, usage)

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

    for name, schema in schemaDict.items():
        for usage in usages:
            os.makedirs(f'{output}/{usage}', exist_ok=True)

            if not dataSet[usage][name]:
                continue

            with open(f"{output}/{usage}/{name}.json", 'w', encoding='utf8') as f:
                f.write(json.dumps(dataSet[usage][name], indent=2, ensure_ascii=False))

        progress = progress + 1
        percentage = int((progress * 100) / size)
        print(f'[{percentage}%] {name}.json 코드 생성')

    
    # JSON 파일 CRC 계산
    for usage in usages:
        crc32Table = {}
        root = f'{output}/{usage}'
        for file in [x for x in os.listdir(root) if x.endswith('.json')]:
            with open(os.path.join(root, file), 'rb+') as f:
                data = f.read()
                crc32Table[file] = zlib.crc32(data)

        with open(f'{root}/Crc.txt', 'w', encoding='utf8') as f:
            f.write(json.dumps(crc32Table, ensure_ascii=False))