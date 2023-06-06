FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Subscriber.csproj", "./"]
RUN dotnet restore "Subscriber.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Subscriber.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Subscriber.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
ARG KAFKA_SERVER
ENV KAFKA_SERVER=${KAFKA_SERVER}
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Subscriber.dll"]
