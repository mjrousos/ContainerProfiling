# Profiling .NET Core Apps in Containers

## Tool Links

- [PerfView](https://github.com/microsoft/perfview/blob/master/documentation/Downloading.md)
- [Dotnet trace](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace) (uses EventPipe tracing)
- [PerfCollect](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/linux-performance-tracing.md) (uses Perf and LTTng)

## Docs and resources

- [.NET Core Linux Performance Analysis video](https://www.youtube.com/watch?v=e1ZaL2PenTI)
- [Old blog post on using PerfCollect from a sidecar container](https://devblogs.microsoft.com/dotnet/collecting-net-core-linux-container-cpu-traces-from-a-sidecar-container/)
- [Helpful GitHub comment on K8s sidecar containers](https://github.com/dotnet/diagnostics/issues/810#issuecomment-636844590)

## Steps to profile in a container with dotnet-trace

1. Build Docker image using mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim (based on Debian 10) as base.
    1. `docker build -t testapp -f Dockerfile ..`
2. Start docker container
    1. `docker run -d -p 8080:80 --rm testapp`
3. Exec into container
    1. `docker exec -it 94 /bin/bash`
4. Install .NET SDK.
    1. `apt-get update`
    2. `curl -OL https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb`
    3. `dpkg -i packages-microsoft-prod.deb`
    4. `apt-get install apt-transport-https`
    5. `apt-get update`
    6. `apt-get install dotnet-sdk-3.1`
5. Install dotnet trace
    1. `dotnet tool install --global dotnet-trace`
6. List processes to trace
    1. Curiously, dotnet trace isn't on the path, but can be access via full path
    2. `~/.dotnet/tools/dotnet-trace ps`
7. Collect trace
    1. `~/.dotnet/tools/dotnet-trace collect -p 1`
8. Exit container and copy file locally
    1. `docker cp 94e857:/app/trace.nettrace .`
9. Open with PerfView

## Steps to profile in a container with PerfCollect

1. **Environment variables must be set for PerfCollect to work.**
2. **The container to be profiled must be started privileged.**
3. **Note that Framework symbols won't resolve by default. There are two options:**
    1. **[Download crossgen](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/linux-performance-tracing.md#resolving-framework-symbols) from the .NET Core NuGet package. This is a pain, but can be done as part of building the Dockerfile.**
    2. **Force JIT'ing Framework assemblies. This will slow startup by 1-2 seconds, but may be ok for perf investigations and is simpler than messing with crossgen**
4. Build Docker image using mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim (based on Debian 10) as base.
    1. `docker build -t testapp -f Dockerfile ..`
5. Start the container privileged and with env vars set:
    1. `docker run -d -p 8080:80 --rm --privileged --cap-add ALL -e COMPlus_PerfMapEnabled=1 -e COMPlus_EnableEventLog=1 -e COMPlus_ZapDisable-1 testapp`
    2. Note that ZapDisable is only needed if Framework symbols are necessary and crossgen isn't present.
6. Exec into container
    1. `docker exec -it 94 /bin/bash`
7. `apt-get update`
8. Download perfcollect
    1. `curl -OL http://aka.ms/perfcollect`
9. Make the script executable
    1. `chmod +x perfcollect`
10. Install tracing libraries
    1. `./perfcollect install`
11. Collect
    1. `./perfcollect collect sampleTrace`
12. Exit container and copy file locally
    1. `docker cp 1cc:/app/sampleTrace.trace.zip .`
13. Open with PerfView

## Steps to profile from a sidecar with dotnet-trace

1. **There's an issue with dotnet-trace such that the sidecar and target containers must share their /tmp directories. Should be fixed for .NET 5. https://github.com/dotnet/diagnostics/issues/810**
2. Start docker container
    1. `docker run -d -p 8080:80 -v /tmp/container_sockets:/tmp --rm testapp`
3. Start (and exec into) the sidecar container with the same process space
    1. `docker run -it --pid=container:5f0 -v /tmp/container_sockets:/tmp sidecar`
4. Install dotnet-trace
    1. `dotnet tool install --global dotnet-trace`
5. List processes to trace
    1. Curiously, dotnet trace isn't on the path, but can be access via full path
    1. `~/.dotnet/tools/dotnet-trace ps`
6. Collect trace
    1. `~/.dotnet/tools/dotnet-trace collect -p 1`
7. Exit container and copy file locally
    1. `docker cp 94e857:/tools/trace.nettrace .`
8. Open with PerfView

## Steps to profile from a sidecar with PerfCollect

1. **Environment variables must be set in the target container for PerfCollect to work.**
2. **The sidecar container must run as privileged.**
3. **Note that Framework symbols won't resolve by default. There are two options:**
    1. **[Download crossgen](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/linux-performance-tracing.md#resolving-framework-symbols) from the .NET Core NuGet package. This is a pain, but can be done as part of building the sidecar's Dockerfile.**
    2. **Force JIT'ing Framework assemblies. This will slow startup by 1-2 seconds, but may be ok for perf investigations and is simpler than messing with crossgen.**
4. Start Docker container with tracing env vars
    1. `docker run -d -p 8080:80 --rm -e COMPlus_PerfMapEnabled=1 -e COMPlus_EnableEventLog=1 -e COMPlus_ZapDisable=1 testapp`
5. Start (and exec into) the privileged sidecar container with the same process space
    1. `docker run -it --privileged --cap-add ALL --pid=container:5f0 sidecar`
6. Collect
    1. `./perfcollect collect sampleTrace`
7. Exit container and copy file locally
    1. `docker cp 1cc:/app/sampleTrace.trace.zip .`
8. Open with PerfView

## Steps to profile in a K8s container with dotnet-trace

1. Deploy pod
    1. `kubectl apply -f pod.yaml`
1. Exec into container
    1. `kubectl exec -it testapp -c testapp -- /bin/bash`
1. Install .NET SDK.
    1. `apt-get update`
    2. `curl -OL https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb`
    3. `dpkg -i packages-microsoft-prod.deb`
    4. `apt-get install apt-transport-https`
    5. `apt-get update`
    6. `apt-get install dotnet-sdk-3.1`
1. Install dotnet trace
    1. `dotnet tool install --global dotnet-trace`
1. List processes to trace
    1. Curiously, dotnet trace isn't on the path, but can be access via full path
    2. `~/.dotnet/tools/dotnet-trace ps`
1. Collect trace
    1. `~/.dotnet/tools/dotnet-trace collect -p 1`
1. Exit container and copy file locally
    1. `kubectl cp testapp:/app/trace.nettrace ./Traces/trace.nettrace -c testapp`
1. Open with PerfView

## Steps to profile in K8s container from sidecar container with dotnet-trace

1. **There's an issue with dotnet-trace such that the sidecar and target containers must share their /tmp directories. Should be fixed for .NET 5. https://github.com/dotnet/diagnostics/issues/810**
2. Deploy pod
    1. `kubectl apply -f pod.yaml`
3. Exec into sidecar container
    1. `kubectl exec -it testapp -c sidecar -- /bin/bash`
4. Install dotnet-trace
    1. `dotnet tool install --global dotnet-trace`
5. List processes to trace
    1. Curiously, dotnet trace isn't on the path, but can be access via full path
    2. `~/.dotnet/tools/dotnet-trace ps`
6. Collect trace
    1. `~/.dotnet/tools/dotnet-trace collect -p 1`
7. Exit container and copy file locally
    1. `kubectl cp testapp:/tools/trace.nettrace ./Traces/trace.nettrace -c sidecar`
8. Open with PerfView

## Steps to profile in K8s container from sidecar container with PerfCollect

1. **Environment variables must be set in the target container for PerfCollect to work.**
2. **The sidecar container must run as privileged.**
3. **Note that Framework symbols won't resolve by default. There are two options:**
    1. **[Download crossgen](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/linux-performance-tracing.md#resolving-framework-symbols) from the .NET Core NuGet package. This is a pain, but can be done as part of building the sidecar's Dockerfile.**
    2. **Force JIT'ing Framework assemblies. This will slow startup by 1-2 seconds, but may be ok for perf investigations and is simpler than messing with crossgen.**
4. Deploy pod
    1. `kubectl apply -f pod.yaml`
5. Exec into sidecar container
    1. `kubectl exec -it testapp -c sidecar -- /bin/bash`
6. In AKS, it may be necessary to edit /usr/bin/perf to have a correct version (4.19). It used to be necessary to build perf manually, but that seems to not be the case anymore.
7. Collect
    1. `./perfcollect collect sampleTrace`
8. Exit container and copy file locally
    1. `kubectl cp testapp:/tools/sampleTrace6.trace.zip ./Traces/sampleTrace.trace.zip -c sidecar`
9. Open with PerfView

## Steps to profile in K8s container with PerfCollect

1. **Environment variables must be set for PerfCollect to work.**
2. **The container to be profiled must be started privileged.**
3. **Note that Framework symbols won't resolve by default. There are two options:**
    1. **[Download crossgen](https://github.com/dotnet/coreclr/blob/master/Documentation/project-docs/linux-performance-tracing.md#resolving-framework-symbols) from the .NET Core NuGet package. This is a pain, but can be done as part of building the Dockerfile.**
    2. **Force JIT'ing Framework assemblies. This will slow startup by 1-2 seconds, but may be ok for perf investigations and is simpler than messing with crossgen**
4. Deploy pod
    1. `kubectl apply -f pod.yaml`
5. Exec into container
    1. `kubectl exec -it testapp -c testapp -- /bin/bash`
6. `apt-get update`
7. Download perfcollect
    1. `curl -OL http://aka.ms/perfcollect`
8. Make the script executable
    1. `chmod +x perfcollect`
9. Install tracing libraries
    1. `./perfcollect install`
10. Collect
    1. `./perfcollect collect sampleTrace`
11. Exit container and copy file locally
    1. `kubectl cp testapp:/tools/sampleTrace6.trace.zip ./Traces/sampleTrace.trace.zip -c testapp`
12. Open with PerfView

## Troubleshooting

- If perfcollect exits immediately, run perf directly to see the error. A common cause is not running privileged. The perf command that PerfCollect typically runs under the covers is: `perf record -g -p`
- In some cases (especially in AKS), the Perf version is wrong in `/usr/bin/perf`. This can be patched up manually (set `version="4.19"`) by editing `/usr/bin/perf` or, if perf did not install at all, [build it locally](https://askubuntu.com/questions/50145/how-to-install-perf-monitoring-tool/753796#753796).
