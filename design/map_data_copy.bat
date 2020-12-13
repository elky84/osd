del /Q /F "..\server\ServerShared\Resources\Map\*.json"
robocopy "..\client\UnityClient\Assets\Resources\MapFile" "..\server\ServerShared\Resources\Map" "*.json" /E

pause