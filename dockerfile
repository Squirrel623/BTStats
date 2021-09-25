FROM mcr.microsoft.com/dotnet/sdk:3.1

WORKDIR /app

COPY . .

WORKDIR /app/BTStatsCore/wwwsrc

FROM node:16

RUN yarn install

RUN yarn build

WORKDIR /app

RUN dotnet restore

RUN dotnet build

WORKDIR /app/BTStatsCore

VOLUME /logs
ENV LogDir /logs
VOLUME /serverLogs
ENV ServerLogsDir /serverLogs
ENV ServePort 80

ENTRYPOINT dotnet run --configuration Release