FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Recommendo.Api/Recommendo.Api.csproj", "Recommendo.Api/"]
RUN dotnet restore "Recommendo.Api/Recommendo.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Recommendo.Api"
RUN dotnet build "Recommendo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Recommendo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recommendo.Api.dll"]
