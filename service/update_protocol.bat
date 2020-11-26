flatc --csharp -o Protocols protocol.fbs
PUSHD extension
CALL flatb_extension.exe --dir=..\Protocols
POPD

ROBOCOPY Protocols ..\shared\NetworkShared\Protocols
RMDIR Protocols /s /q
PAUSE