FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY UserService/UserService.csproj ./UserService/

RUN dotnet restore UserService/UserService.csproj

COPY UserService/. ./UserService/

RUN dotnet publish UserService/UserService.csproj -c Release -o /app/out/UserService

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/UserService ./UserService

EXPOSE 80

ENTRYPOINT ["dotnet", "UserService/UserService.dll"]
