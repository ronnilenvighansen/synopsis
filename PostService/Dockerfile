FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY PostService/PostService.csproj ./PostService/

RUN dotnet restore PostService/PostService.csproj

COPY PostService/. ./PostService/

RUN dotnet publish PostService/PostService.csproj -c Release -o /app/out/PostService

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/PostService ./PostService

COPY PostService/appsettings.PostService.json /app/appsettings.PostService.json

EXPOSE 80

ENTRYPOINT ["dotnet", "PostService/PostService.dll"]
