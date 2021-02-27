@ECHO OFF

PUSHD server
CALL build.bat
POPD

PUSHD bin\server
CALL TestServer.exe
PAUSE