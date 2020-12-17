@ECHO OFF

ECHO %cd%

RMDIR /s/q ..\server\TestServer\bin\Debug\net5.0\Scripts
ROBOCOPY . ..\server\TestServer\bin\Debug\net5.0\Scripts "*.lua" /E /NFL /NDL /NJH /NJS /nc /ns /np

RMDIR /s/q ..\bin\server\Scripts
ROBOCOPY . ..\bin\server\Scripts "*.lua" /E /NFL /NDL /NJH /NJS /nc /ns /np
