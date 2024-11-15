FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./CheckerHttpEvents/CheckerHttpEvents.csproj ./CheckerHttpEvents/
COPY ./MessageUpdater/MessageUpdater.csproj ./MessageUpdater/
COPY ./.env ./

RUN dotnet restore ./MessageUpdater/MessageUpdater.csproj
RUN dotnet restore ./CheckerHttpEvents/CheckerHttpEvents.csproj

COPY ./MessageUpdater/. ./MessageUpdater/
COPY ./CheckerHttpEvents/. ./CheckerHttpEvents/
COPY ./Documentation ./Documentation
COPY ./README.md .

RUN dotnet publish ./MessageUpdater/MessageUpdater.csproj -c Release -o /app/publish && \
    dotnet publish ./CheckerHttpEvents/CheckerHttpEvents.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/publish ./
COPY --from=build-env /app/.env ./

ENTRYPOINT ["dotnet", "MessageUpdater.dll"]