# Use the official .NET 8 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files
COPY ["Gateway/microservices_project.csproj", "Gateway/"]
COPY ["CommonShared/CommonShared.csproj", "CommonShared/"]

# Restore dependencies
RUN dotnet restore "Gateway/microservices_project.csproj"

# Copy the rest of the source code
COPY ["Gateway/", "Gateway/"]
COPY ["CommonShared/", "CommonShared/"]

# Build the application
RUN dotnet build "Gateway/microservices_project.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Gateway/microservices_project.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Remove conflicting appsettings files from CommonShared
RUN rm -f /app/publish/appsettings*.json

# Use the official .NET 8 runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "microservices_project.dll"]
