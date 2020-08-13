#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Setup base image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.7-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.401-buster AS build
WORKDIR /src
COPY ["AKSPerf.csproj", "."]
RUN dotnet restore "AKSPerf.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "AKSPerf.csproj" -c Release -o /app/build

# These steps just get a matching crossgen onto the image for later profiling use
#   WORKDIR /temp
#   RUN dotnet new console
#   RUN dotnet publish --self-contained -r linux-x64

# Publish
FROM build AS publish
RUN dotnet publish "AKSPerf.csproj" -c Release -o /app/publish

# Copy cross gen next to the framework-dependent publish output
#   RUN cp ~/.nuget/packages/microsoft.netcore.app.runtime.linux-x64/3.1.7/tools/crossgen /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AKSPerf.dll"]