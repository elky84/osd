[![Website](https://img.shields.io/website-up-down-green-red/http/shields.io.svg?label=elky-essay)](https://elky84.github.io)
<img src="https://img.shields.io/badge/made%20with-.NET6-blue.svg" alt="made with .NET6">

![GitHub forks](https://img.shields.io/github/forks/elky84/osd.svg?style=social&label=Fork)
![GitHub stars](https://img.shields.io/github/stars/elky84/osd.svg?style=social&label=Stars)
![GitHub watchers](https://img.shields.io/github/watchers/elky84/osd.svg?style=social&label=Watch)
![GitHub followers](https://img.shields.io/github/followers/elky84.svg?style=social&label=Follow)

![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![GitHub repo size in bytes](https://img.shields.io/github/repo-size/elky84/osd.svg)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/elky84/osd.svg)

# osd

## introduce

[![Youtube](https://img.youtube.com/vi/NjaW4AE28ik/0.jpg)](https://www.youtube.com/watch?v=NjaW4AE28ik)

### English

It is a 2D side view MMORPG game.

Implemented using C# .NET 6, Unity3D, DotNetty, FlatBuffers.

It is a state in which only basic movement, jumping, and attack are implemented. 

### Korean

2D 사이드 뷰 MMORPG 게임입니다.

C# .NET 6, Unity3D, DotNetty, FlatBuffers를 이용해 구현되어있습니다.

기본적인 이동, 점프, 공격만 구현되어있는 상태입니다.

## Usage

### English

* design directory (Excel files)
	* execute "update.bat" file to generate class and json
* service directory (protocols)
	* execute "update_protocol.bat" file to generate protocol classes.
* tiled directory (map file)
  * tiled data export to "client\UnityClient\Assets\Resources\MapFile"
  * The data is copied to the server execution folder through update.bat and ServerShared build process. 

### Korean

* design 디렉토리 (Excel 파일)
  * "update.bat" 파일을 실행하여 클래스 및 json 생성
* service 디렉토리 (프로토콜)
  * "update_protocol.bat" 파일을 실행하여 프로토콜 클래스를 생성합니다.
* tiled 디렉토리 (Map 파일)
   * tiled 데이터를 "client\UnityClient\Assets\Resources\MapFile"로 내보내기
   * 해당 데이터는 update.bat 및 ServerShared 빌드 과정을 통해 서버 실행 폴더에 복사됩니다. 
