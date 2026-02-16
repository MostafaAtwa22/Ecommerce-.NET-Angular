FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

USER app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ecommerce.API/Ecommerce.API.csproj", "Ecommerce.API/"]
COPY ["Ecommerce.Core/Ecommerce.Core.csproj", "Ecommerce.Core/"]
COPY ["Ecommerce.Infrastructure/Ecommerce.Infrastructure.csproj", "Ecommerce.Infrastructure/"]

RUN dotnet restore "Ecommerce.API/Ecommerce.API.csproj"
COPY . .
WORKDIR "/src/Ecommerce.API"
RUN dotnet build "Ecommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Ecommerce.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.API.dll"]
