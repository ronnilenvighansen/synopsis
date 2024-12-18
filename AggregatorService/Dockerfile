FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY AggregatorService/AggregatorService.csproj ./AggregatorService/
COPY UserService/UserService.csproj ./UserService/
COPY PostService/PostService.csproj ./PostService/

RUN dotnet restore AggregatorService/AggregatorService.csproj
RUN dotnet restore UserService/UserService.csproj
RUN dotnet restore PostService/PostService.csproj

COPY AggregatorService/. ./AggregatorService/
COPY UserService/. ./UserService/
COPY PostService/. ./PostService/

RUN dotnet publish AggregatorService/AggregatorService.csproj -c Release -o /app/out/AggregatorService
RUN dotnet publish UserService/UserService.csproj -c Release -o /app/out/UserService
RUN dotnet publish PostService/PostService.csproj -c Release -o /app/out/PostService

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/AggregatorService ./AggregatorService
COPY --from=build /app/out/UserService ./UserService
COPY --from=build /app/out/PostService ./PostService

COPY AggregatorService/appsettings.AggregatorService.json /app/appsettings.AggregatorService.json
COPY UserService/appsettings.UserService.json /app/appsettings.UserService.json
COPY PostService/appsettings.PostService.json /app/appsettings.PostService.json

EXPOSE 80

ENTRYPOINT ["dotnet", "AggregatorService/AggregatorService.dll"]
