FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["companyOSINT.Import/companyOSINT.Import.csproj", "companyOSINT.Import/"]
COPY ["companyOSINT.Domain/companyOSINT.Domain.csproj", "companyOSINT.Domain/"]
RUN dotnet restore "companyOSINT.Import/companyOSINT.Import.csproj"
COPY . .
WORKDIR "/src/companyOSINT.Import"
RUN dotnet publish "./companyOSINT.Import.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "companyOSINT.Import.dll"]
