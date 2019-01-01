FROM microsoft/dotnet:2.1-sdk

WORKDIR /app

COPY . .

RUN npm install -g yarn@1.3

WORKDIR /app/BTStatsCore/wwwsrc

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
EXPOSE 80

ENTRYPOINT dotnet run --configuration Release