FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./CheckerHttpEvents/CheckerHttpEvents.csproj ./CheckerHttpEvents/

RUN dotnet restore ./CheckerHttpEvents/CheckerHttpEvents.csproj

COPY ./CheckerHttpEvents/. ./CheckerHttpEvents/
COPY ./Documentation ./Documentation
COPY ./README.md .

RUN dotnet publish ./CheckerHttpEvents/CheckerHttpEvents.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/publish ./

ENTRYPOINT ["dotnet", "CheckerHttpEvents.dll"]