flatc --csharp -o Protocols protocol.fbs
ROBOCOPY Protocols ..\shared\NetworkShared\Protocols
RMDIR Protocols /s /q