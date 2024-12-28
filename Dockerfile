FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY UserService/*.csproj UserService/
COPY UserService.Tests/*.csproj UserService.Tests/
RUN dotnet restore UserService/UserService.csproj 
RUN dotnet restore UserService.Tests/UserService.Tests.csproj


# copy and build app
COPY UserService/ UserService/
WORKDIR /source/UserService
RUN dotnet build -c release --no-restore

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
WORKDIR /source/UserService.Tests
COPY UserService.Tests/ .
ENTRYPOINT ["dotnet", "test", "--filter", "Category=Unit", "--logger:console;verbosity=detailed"]

FROM build AS publish
WORKDIR /source/UserService
RUN dotnet publish -c release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "UserService.dll"]