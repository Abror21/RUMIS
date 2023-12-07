#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

# install wkhtmltopdf with qt patch
RUN apt-get update -y && apt-get install -y libc6-dev libgdiplus libx11-dev && apt-get clean && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll
RUN apt-get install -y wget fontconfig xfonts-base xfonts-75dpi && apt-get clean
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.buster_amd64.deb
RUN dpkg -i wkhtmltox_0.12.6-1.buster_amd64.deb && ln -s /usr/local/bin/wkhtmltopdf /usr/bin/wkhtmltopdf

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Izm.Rumis/Izm.Rumis.Api/Izm.Rumis.Api.csproj", "Izm.Rumis.Api/"]
COPY ["Izm.Rumis/Izm.Rumis.Application/Izm.Rumis.Application.csproj", "Izm.Rumis.Application/"]
COPY ["Izm.Rumis/Izm.Rumis.Domain/Izm.Rumis.Domain.csproj", "Izm.Rumis.Domain/"]
COPY ["Izm.Rumis/Izm.Rumis.Infrastructure/Izm.Rumis.Infrastructure.csproj", "Izm.Rumis.Infrastructure/"]
COPY ["Izm.Rumis/Izm.Rumis.Logging/Izm.Rumis.Logging.csproj", "Izm.Rumis.Logging/"]
RUN dotnet restore "Izm.Rumis.Api/Izm.Rumis.Api.csproj"
COPY ["Izm.Rumis", "./"]
WORKDIR "/src/Izm.Rumis.Api"
RUN dotnet build "Izm.Rumis.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Izm.Rumis.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Izm.Rumis.Api.dll"]