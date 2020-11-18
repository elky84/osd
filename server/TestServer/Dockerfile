FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["TestServer/TestServer.csproj", "TestServer/"]
COPY ["ServerShared/ServerShared.csproj", "ServerShared/"]
COPY ["../shared/MasterDataServer/MasterDataServer.csproj", "../shared/MasterDataServer/"]
COPY ["../shared/NetworkShared/NetworkShared.csproj", "../shared/NetworkShared/"]
COPY ["../shared/Protocols/Protocols.csproj", "../shared/Protocols/"]
COPY ["../shared/SharedUtility/SharedUtility.csproj", "../shared/SharedUtility/"]
COPY ["../shared/MasterDataTypes/MasterDataTypes.csproj", "../shared/MasterDataTypes/"]
RUN dotnet restore "TestServer/TestServer.csproj"
COPY . .
WORKDIR "/src/TestServer"
RUN dotnet build "TestServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestServer.dll"]