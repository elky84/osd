PUSHD converter
CALL excel_converter.exe --dir=.. --out=output

RMDIR /s/q ..\..\client\ClientShared\MasterDataType
ROBOCOPY output\class\client ..\..\client\ClientShared\MasterDataType
RMDIR /s/q ..\..\server\ServerShared\MasterDataType
ROBOCOPY output\class\server ..\..\server\ServerShared\MasterDataType

RMDIR /s/q ..\..\client\ClientShared\Table
ROBOCOPY output\table\client ..\..\client\ClientShared\Table
RMDIR /s/q ..\..\server\ServerShared\Table
ROBOCOPY output\table\server ..\..\server\ServerShared\Table

RMDIR /s/q ..\..\client\ClientShared\Json
ROBOCOPY output\json\client ..\..\client\ClientShared\Json
RMDIR /s/q ..\..\server\ServerShared\Json
RMDIR /s/q ..\..\server\TestServer\bin\Debug\net5.0\Json
ROBOCOPY output\json\server ..\..\server\TestServer\bin\Debug\net5.0\Json
POPD

PAUSE