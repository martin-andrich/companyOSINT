FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root

# Install Chromium dependencies required by PuppeteerSharp
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    fonts-liberation \
    libasound2t64 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libatspi2.0-0 \
    libcairo2 \
    libcups2 \
    libdbus-1-3 \
    libdrm2 \
    libexpat1 \
    libgbm1 \
    libglib2.0-0 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libpango-1.0-0 \
    libx11-6 \
    libxcb1 \
    libxcomposite1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxkbcommon0 \
    libxrandr2 \
    wget \
    xdg-utils \
    && rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["companyOSINT.Worker/companyOSINT.Worker.csproj", "companyOSINT.Worker/"]
COPY ["companyOSINT.Domain/companyOSINT.Domain.csproj", "companyOSINT.Domain/"]
RUN dotnet restore "companyOSINT.Worker/companyOSINT.Worker.csproj"
COPY . .
WORKDIR "/src/companyOSINT.Worker"
RUN dotnet build "./companyOSINT.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./companyOSINT.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "companyOSINT.Worker.dll"]
