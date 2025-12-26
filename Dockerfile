# Base stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Python in the runtime image
RUN apt-get update && \
    apt-get install -y python3 python3-pip && \
    rm -rf /var/lib/apt/lists/*

# SDK stage for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/DynamicBalanceEngine.Backend.csproj", "backend/"]
RUN dotnet restore "backend/DynamicBalanceEngine.Backend.csproj"
COPY . .
WORKDIR "/src/backend"
RUN dotnet build "DynamicBalanceEngine.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DynamicBalanceEngine.Backend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy python scripts to the container
COPY --from=build /src/python_scripts /app/python_scripts
# Install python requirements
RUN pip3 install -r /app/python_scripts/requirements.txt --break-system-packages

ENTRYPOINT ["dotnet", "DynamicBalanceEngine.Backend.dll"]
