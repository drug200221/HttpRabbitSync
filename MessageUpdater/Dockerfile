FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./CheckerHttpEvents/CheckerHttpEvents.csproj ./CheckerHttpEvents/
COPY ./MessageUpdater/MessageUpdater.csproj ./MessageUpdater/

RUN dotnet restore ./MessageUpdater/MessageUpdater.csproj
RUN dotnet restore ./CheckerHttpEvents/CheckerHttpEvents.csproj

COPY ./MessageUpdater/. ./MessageUpdater/
COPY ./CheckerHttpEvents/. ./CheckerHttpEvents/
COPY ./Documentation ./Documentation
COPY ./README.md .

RUN dotnet publish ./MessageUpdater/MessageUpdater.csproj -c Debug -o /app/publish
RUN dotnet publish ./CheckerHttpEvents/CheckerHttpEvents.csproj -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/publish ./
ENTRYPOINT ["dotnet", "MessageUpdater.dll"]