PUSHD converter
CALL app.exe --dir=.. --out=output

RMDIR /s/q ..\..\shared\NetworkShared\MasterDataType
ROBOCOPY output\class\common ..\..\shared\NetworkShared\MasterDataType /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\..\client\ClientShared\MasterDataType
ROBOCOPY output\class\client ..\..\client\ClientShared\MasterDataType /E /NFL /NDL /NJH /NJS /nc /ns /np
RMDIR /s/q ..\..\server\ServerShared\MasterDataType
ROBOCOPY output\class\server ..\..\server\ServerShared\MasterDataType /E /NFL /NDL /NJH /NJS /nc /ns /np

DEL /Q /F ..\..\client\ClientShared\Table\Table.cs
COPY output\bind\client\Table.cs ..\..\client\ClientShared\Table\Table.cs >NUL
DEL /Q /F ..\..\server\ServerShared\Table\Table.cs
COPY output\bind\server\Table.cs ..\..\server\ServerShared\Table\Table.cs >NUL

RMDIR /s/q ..\..\client\ClientShared\json
ROBOCOPY output\json\client ..\..\client\ClientShared\json /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\..\server\ServerShared\json
ROBOCOPY output\json\server ..\..\server\ServerShared\json /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\..\shared\NetworkShared\Enum
ROBOCOPY output\enum ..\..\shared\NetworkShared\Enum /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\..\client\ClientShared\Const
ROBOCOPY output\const\client ..\..\client\ClientShared\Const /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\..\client\ServerShared\Const
ROBOCOPY output\const\server ..\..\server\ServerShared\Const /E /NFL /NDL /NJH /NJS /nc /ns /np

POPD
ROBOCOPY ..\client\UnityClient\Assets\Resources\MapFile ..\server\ServerShared\Resources\Map *.json /E /NFL /NDL /NJH /NJS /nc /ns /np