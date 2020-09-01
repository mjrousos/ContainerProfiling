
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.401-buster AS base


### PerfCollect installation
WORKDIR /src
RUN dotnet new console

# Restore with `-r linux-x64` to download the runtime package containing crossgen.
RUN dotnet restore -r linux-x64
RUN cp `find ~/.nuget/packages -name crossgen` .
# perfcollect expects to find crossgen along side libcoreclr.so
RUN cp /src/crossgen $(dirname `find /usr/share/dotnet/ -name libcoreclr.so`)

RUN apt-get update

WORKDIR /tools

RUN curl -OL http://aka.ms/perfcollect \
    && chmod a+x perfcollect

RUN ./perfcollect install


### dotnet-trace (etc.) installation
RUN dotnet tool install --global dotnet-trace
RUN dotnet tool install --global dotnet-counters
RUN dotnet tool install --global dotnet-dump


ENTRYPOINT ["/bin/bash"]