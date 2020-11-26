PUSHD converter
CALL excel_converter.exe --dir=.. --out=output

RMDIR /s/q ..\..\client\ClientShared\MasterDataType
ROBOCOPY output\class\client ..\..\client\ClientShared\MasterDataType
RMDIR /s/q ..\..\server\ServerShared\MasterDataType
ROBOCOPY output\class\server ..\..\server\ServerShared\MasterDataType

RMDIR /s/q ..\..\client\ClientShared\Table.cs
ROBOCOPY output\table\client\Table.cs ..\..\client\ClientShared\Table\Table.cs
RMDIR /s/q ..\..\server\ServerShared\Table.cs
ROBOCOPY output\table\server\Table.cs ..\..\server\ServerShared\Table\Table.cs

RMDIR /s/q ..\..\client\ClientShared\Json
ROBOCOPY output\json\client ..\..\client\ClientShared\Json
RMDIR /s/q ..\..\server\ServerShared\Json
RMDIR /s/q ..\..\server\TestServer\bin\Debug\net5.0\Json
ROBOCOPY output\json\server ..\..\server\TestServer\bin\Debug\net5.0\Json
POPD

PAUSE