FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./MessageUpdater/MessageUpdater.csproj ./MessageUpdater/
COPY ./CheckerHttpEvents/CheckerHttpEvents.csproj ./CheckerHttpEvents/

RUN dotnet restore ./MessageUpdater/MessageUpdater.csproj
RUN dotnet restore ./CheckerHttpEvents/CheckerHttpEvents.csproj

COPY ./MessageUpdater/. ./MessageUpdater/
COPY ./CheckerHttpEvents/. ./CheckerHttpEvents/
COPY ./Documentation ./Documentation
COPY ./README.md .

RUN dotnet publish ./CheckerHttpEvents/CheckerHttpEvents.csproj -c Release -o bin
RUN dotnet publish ./MessageUpdater/MessageUpdater.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/bin ./

ENTRYPOINT ["dotnet", "MessageUpdater.dll"]