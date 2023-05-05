#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/OnlineSales/OnlineSales.csproj", "src/OnlineSales/"]
RUN dotnet restore "src/OnlineSales/OnlineSales.csproj"
COPY . .
WORKDIR "/src/src/OnlineSales"
RUN dotnet build "OnlineSales.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OnlineSales.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=1m --timeout=10s --retries=3 CMD wget --spider --tries=1 --no-verbose http://localhost:80/api/version || exit 1
ENTRYPOINT ["dotnet", "OnlineSales.dll"]
