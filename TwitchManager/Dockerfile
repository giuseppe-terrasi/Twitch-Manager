#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TwitchManager/TwitchManager.csproj", "TwitchManager/"]
COPY ["FFMEPGWrapper/FFMEPGWrapper.csproj", "FFMEPGWrapper/"]
RUN dotnet restore "./TwitchManager/TwitchManager.csproj"
COPY . .
WORKDIR "/src/TwitchManager"
RUN dotnet build "./TwitchManager.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TwitchManager.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#RUN apt update && apt install -y ffmpeg
ENTRYPOINT ["dotnet", "TwitchManager.dll"]