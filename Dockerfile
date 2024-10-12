# Base image
FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Mqtt-Kafka-Connector.csproj", "./"]
RUN dotnet restore "Mqtt-Kafka-Connector.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Mqtt-Kafka-Connector.csproj" -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish "Mqtt-Kafka-Connector.csproj" -c Release -o /app/publish

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mqtt-Kafka-Connector.dll"]
