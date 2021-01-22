@ECHO OFF

RMDIR Protocols /s /q
flatc --csharp -o Protocols\Request request.fbs
flatc --csharp -o Protocols\Response response.fbs
flatc --csharp -o Protocols common.fbs
PUSHD extension
CALL flatb_extension.exe --dir=..\Protocols\Request --namespace FlatBuffers.Protocol.Request
CALL flatb_extension.exe --dir=..\Protocols\Response --namespace FlatBuffers.Protocol.Response
CALL flatb_extension.exe --dir=..\Protocols --namespace FlatBuffers.Protocol
POPD

RMDIR ..\shared\NetworkShared\Protocols /s /q
ROBOCOPY Protocols ..\shared\NetworkShared\Protocols *.* /E /njh /njs /ndl /nc /ns
RMDIR Protocols /s /q

pause