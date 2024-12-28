FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY UserService/*.csproj UserService/
RUN dotnet restore UserService/UserService.csproj 


COPY UserService/ UserService/
WORKDIR /source/UserService
RUN dotnet build -c release --no-restore

# test
FROM build AS test
COPY UserService.Tests/*.csproj UserService.Tests/
RUN dotnet restore UserService.Tests/UserService.Tests.csproj

WORKDIR /source/UserService.Tests
COPY UserService.Tests/ .
ENTRYPOINT ["dotnet", "test", "--logger:console;verbosity=detailed"]


FROM build AS publish
WORKDIR /source/UserService
RUN dotnet publish -c release --no-build -o /app

# final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "UserService.dll"]