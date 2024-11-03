FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY AuthService/*.csproj AuthService/
COPY AuthService.Tests/*.csproj AuthService.Tests/
RUN dotnet restore AuthService/AuthService.csproj 
RUN dotnet restore AuthService.Tests/AuthService.Tests.csproj


# copy and build app
COPY AuthService/ AuthService/
WORKDIR /source/AuthService
RUN dotnet build -c release --no-restore

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
WORKDIR /source/AuthService.Tests
COPY AuthService.Tests/ .
ENTRYPOINT ["dotnet", "test", "--filter", "Category=Unit", "--logger:console;verbosity=detailed"]

FROM build AS publish
WORKDIR /source/AuthService
RUN dotnet publish -c release --no-build -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AuthService.dll"]
