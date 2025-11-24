FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/RebtelLibraryAPI.Domain/RebtelLibraryAPI.Domain.csproj", "src/RebtelLibraryAPI.Domain/"]
COPY ["src/RebtelLibraryAPI.Application/RebtelLibraryAPI.Application.csproj", "src/RebtelLibraryAPI.Application/"]
COPY ["src/RebtelLibraryAPI.Infrastructure/RebtelLibraryAPI.Infrastructure.csproj", "src/RebtelLibraryAPI.Infrastructure/"]
COPY ["src/RebtelLibraryAPI.API/RebtelLibraryAPI.API.csproj", "src/RebtelLibraryAPI.API/"]
RUN dotnet restore "./src/RebtelLibraryAPI.API/RebtelLibraryAPI.API.csproj"
COPY . .
WORKDIR "/src/src/RebtelLibraryAPI.API"
RUN dotnet build "RebtelLibraryAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "RebtelLibraryAPI.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RebtelLibraryAPI.API.dll"]