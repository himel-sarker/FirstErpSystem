# ================================================================
# Dockerfile — Added By Himel Sarkar 10-06-2026
# LEARNING: Multi-stage build
# Stage 1 (Build) = Source code compile করে
# Stage 2 (Runtime) = শুধু compiled file রাখে (Image size ছোট হয়)
# ================================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# LEARNING: আগে .csproj copy করে restore করি
# এতে Docker layer caching কাজ করে — প্রতিবার full restore লাগে না
COPY *.csproj ./
RUN dotnet restore

# বাকি সব ফাইল copy করে publish করি
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Docker container-এ port 8080 এ চলবে
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FirstErpSystem.Api.dll"]
