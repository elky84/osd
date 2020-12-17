RMDIR Protocols /s /q
flatc --csharp -o Protocols protocol.fbs
PUSHD extension
CALL flatb_extension.exe --dir=..\Protocols
POPD

RMDIR ..\shared\NetworkShared\Protocols /s /q
ROBOCOPY Protocols ..\shared\NetworkShared\Protocols
RMDIR Protocols /s /q