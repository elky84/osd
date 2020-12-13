RMDIR "../bin/server" /S /Q
dotnet publish TestServer/TestServer.csproj -c Release -r win-x64 -p:PublishSingleFile=true -o ../bin/server