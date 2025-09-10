FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/YetAnotherJira.Web/YetAnotherJira.Web.csproj", "src/YetAnotherJira.Web/"]
COPY ["src/YetAnotherJira.Infrastructure/YetAnotherJira.Infrastructure.csproj", "src/YetAnotherJira.Infrastructure/"]
COPY ["src/YetAnotherJira.Application/YetAnotherJira.Application.csproj", "src/YetAnotherJira.Application/"]
COPY ["src/YetAnotherJira.Domain/YetAnotherJira.Domain.csproj", "src/YetAnotherJira.Domain/"]
RUN dotnet restore "src/YetAnotherJira.Web/YetAnotherJira.Web.csproj"
COPY . .
WORKDIR "/src/src/YetAnotherJira.Web"
RUN dotnet build "./YetAnotherJira.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./YetAnotherJira.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YetAnotherJira.Web.dll"]
