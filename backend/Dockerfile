FROM mcr.microsoft.com/dotnet/sdk:6.0.403 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish src/ShiftPlanner.Api/ShiftPlanner.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:6.0.11-bullseye-slim
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "ShiftPlanner.Api.dll"]
