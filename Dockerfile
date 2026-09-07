# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first (better layer caching — dependencies
# only get restored again if these files change, not on every code edit)
COPY ChurnPrediction.sln .
COPY ChurnPrediction.ML/ChurnPrediction.ML.csproj ChurnPrediction.ML/
COPY ChurnPrediction.Api/ChurnPrediction.Api.csproj ChurnPrediction.Api/
COPY ChurnPrediction.Tests/ChurnPrediction.Tests.csproj ChurnPrediction.Tests/

RUN dotnet restore ChurnPrediction.Api/ChurnPrediction.Api.csproj

# Now copy everything else and publish
COPY . .
RUN dotnet publish ChurnPrediction.Api/ChurnPrediction.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# model.zip is included automatically since it's part of the publish
# output (its Copy to Output Directory setting carries through to
# dotnet publish), but this stage is where a missing-model bug would
# surface if that setting were ever wrong — check the image after build.

EXPOSE 8080
ENTRYPOINT ["dotnet", "ChurnPrediction.Api.dll"]