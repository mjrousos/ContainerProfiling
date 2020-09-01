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

### Install dotnet-trace, dotnet-counters, and dotnet-dump
RUN dotnet tool install --tool-path /tools dotnet-trace
RUN dotnet tool install --tool-path /tools dotnet-counters
RUN dotnet tool install --tool-path /tools dotnet-dump
RUN dotnet tool install --tool-path /tools dotnet-gcdump

# These steps just get a matching crossgen onto the image for later profiling use
#   WORKDIR /temp
#   RUN dotnet new console
#   RUN dotnet publish --self-contained -r linux-x64

# Publish
FROM build AS publish
RUN dotnet publish "AKSPerf.csproj" -c Release -o /app/publish

# Copy crossgen next to the framework-dependent publish output
#   RUN cp ~/.nuget/packages/microsoft.netcore.app.runtime.linux-x64/3.1.7/tools/crossgen /app/publish

FROM base AS final

### Copy diagnostics tools
WORKDIR /tools
COPY --from=publish /tools .

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AKSPerf.dll"]