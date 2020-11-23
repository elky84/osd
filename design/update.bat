PUSHD converter
CALL excel_converter.exe --dir=.. --out=output

RMDIR /s/q ..\..\client\ClientShared\Table
ROBOCOPY output\class\client ..\..\client\ClientShared\Table
RMDIR /s/q ..\..\server\ServerShared\Table
ROBOCOPY output\class\server ..\..\server\ServerShared\Table

RMDIR /s/q ..\..\client\ClientShared\Json
ROBOCOPY output\json\client ..\..\client\ClientShared\Json
RMDIR /s/q ..\..\server\ServerShared\Json
ROBOCOPY output\json\server ..\..\server\ServerShared\Json
POPD

PAUSE