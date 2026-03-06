FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["companyOSINT.Web/companyOSINT.Web.csproj", "companyOSINT.Web/"]
COPY ["companyOSINT.Application/companyOSINT.Application.csproj", "companyOSINT.Application/"]
COPY ["companyOSINT.Infrastructure/companyOSINT.Infrastructure.csproj", "companyOSINT.Infrastructure/"]
COPY ["companyOSINT.Domain/companyOSINT.Domain.csproj", "companyOSINT.Domain/"]
RUN dotnet restore "companyOSINT.Web/companyOSINT.Web.csproj"
COPY . .
WORKDIR "/src/companyOSINT.Web"
RUN dotnet build "./companyOSINT.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./companyOSINT.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "companyOSINT.Web.dll"]
