#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/OnlineSales/OnlineSales.csproj", "src/OnlineSales/"]
RUN dotnet restore "src/OnlineSales/OnlineSales.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "OnlineSales.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./src/OnlineSales/OnlineSales.csproj" -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish "./plugins/OnlineSales.Plugin.EmailSync/OnlineSales.Plugin.EmailSync.csproj" -c Release -o /app/publish/plugins/OnlineSales.Plugin.EmailSync /p:UseAppHost=false
RUN dotnet publish "./plugins/OnlineSales.Plugin.ReverseProxy/OnlineSales.Plugin.ReverseProxy.csproj" -c Release -o /app/publish/plugins/OnlineSales.Plugin.ReverseProxy /p:UseAppHost=false
RUN dotnet publish "./plugins/OnlineSales.Plugin.SendGrid/OnlineSales.Plugin.SendGrid.csproj" -c Release -o /app/publish/plugins/OnlineSales.Plugin.SendGrid /p:UseAppHost=false
RUN dotnet publish "./plugins/OnlineSales.Plugin.Sms/OnlineSales.Plugin.Sms.csproj" -c Release -o /app/publish/plugins/OnlineSales.Plugin.Sms /p:UseAppHost=false
RUN dotnet publish "./plugins/OnlineSales.Plugin.Vsto/OnlineSales.Plugin.Vsto.csproj" -c Release -o /app/publish/plugins/OnlineSales.Plugin.Vsto /p:UseAppHost=false

FROM base AS final
RUN apk add --no-cache icu-libs
RUN apk add --no-cache icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=1m --timeout=10s --retries=3 CMD wget --spider --tries=1 --no-verbose http://localhost:80/api/version || exit 1
ENTRYPOINT ["dotnet", "OnlineSales.dll"]
