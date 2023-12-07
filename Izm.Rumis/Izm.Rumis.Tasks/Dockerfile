#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Izm.Rumis/Izm.Rumis.Tasks/Izm.Rumis.Tasks.csproj", "Izm.Rumis.Tasks/"]
COPY ["Izm.Rumis/Izm.Rumis.Infrastructure/Izm.Rumis.Infrastructure.csproj", "Izm.Rumis.Infrastructure/"]
COPY ["Izm.Rumis/Izm.Rumis.Application/Izm.Rumis.Application.csproj", "Izm.Rumis.Application/"]
COPY ["Izm.Rumis/Izm.Rumis.Domain/Izm.Rumis.Domain.csproj", "Izm.Rumis.Domain/"]
COPY ["Izm.Rumis/Izm.Rumis.Logging/Izm.Rumis.Logging.csproj", "Izm.Rumis.Logging/"]
RUN dotnet restore "Izm.Rumis.Tasks/Izm.Rumis.Tasks.csproj"
COPY ["Izm.Rumis", "./"]
WORKDIR "/src/Izm.Rumis.Tasks"
RUN dotnet build "Izm.Rumis.Tasks.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Izm.Rumis.Tasks.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Izm.Rumis.Tasks.dll"]