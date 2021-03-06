FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
RUN apt-get update && apt-get install unzip
# download extra dependencies if they are not already in cache
RUN --mount=type=cache,id=download,target=/download if [ ! -f /download/watchexec.deb ]; then wget -O /download/watchexec.deb https://github.com/watchexec/watchexec/releases/download/cli-v1.17.1/watchexec-1.17.1-x86_64-unknown-linux-musl.deb; fi
RUN --mount=type=cache,id=download,target=/download dpkg -i /download/watchexec.deb
RUN --mount=type=cache,id=download,target=/download if [ ! -f /download/remotedebug.zip ]; then wget -O /download/remotedebug.zip https://github.com/fjb4/dotnet-accelerator/releases/download/tools/JetBrains.Rider.RemoteDebuggerUploads.linux.2021.3EAP7.zip; fi
RUN --mount=type=cache,id=download,target=/download if [ ! -d /download/remotedebug ]; then unzip -o /download/remotedebug.zip -d /download/remotedebug/; fi 
RUN --mount=type=cache,id=download,target=/download if [ ! -d /download/vsdbg ]; then curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -r linux-arm -v latest -l /download/vsdbg; fi 


WORKDIR /
COPY ["src/Tanzu.Common/Tanzu.Common.csproj", "/src/Tanzu.Common/Tanzu.Common.csproj"]
COPY ["src/Tanzu.WebDemo/Tanzu.WebDemo.csproj", "/src/Tanzu.WebDemo/Tanzu.WebDemo.csproj"]
ENV NUGET_PACKAGES=/nuget
RUN --mount=type=cache,id=nuget,target=/nuget dotnet restore -r ubuntu-x64  "/src/Tanzu.WebDemo/Tanzu.WebDemo.csproj"
COPY . .
RUN --mount=type=cache,id=nuget,target=/nuget dotnet msbuild /p:RuntimeIdentifier=ubuntu-x64 /p:SelfContained=false /p:_CommandLineDefinedRuntimeIdentifier=true /p:RestorePackages=false /t:PublishLayer /p:PublishDir=/layer/ /p:DockerLayer=All "/src/Tanzu.WebDemo/Tanzu.WebDemo.csproj"

 
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS run
# install watchexec which will restart the process when any of the files are updated (to support hotpatching container)
COPY --from=build /usr/bin/watchexec /usr/bin/watchexec
# install JetBrains Rider debugging agent
RUN --mount=type=cache,id=download,target=/download mkdir -p /root/.local/share/JetBrains/RiderRemoteDebugger/ && cp -R /download/remotedebug /root/.local/share/JetBrains/RiderRemoteDebugger/2021.3EAP7
# install Visual Studio debugging agent
RUN --mount=type=cache,id=download,target=/download cp -R /download/vsdbg /root/vsdbg
# setup open ssh server to allow Rider to establish debugging session
RUN apt-get update && apt-get install -y openssh-server
RUN echo 'root:admin' | chpasswd
RUN echo "PasswordAuthentication yes" >> /etc/ssh/sshd_config
RUN echo "PermitRootLogin yes" >> /etc/ssh/sshd_config
RUN update-rc.d ssh enable

WORKDIR /app
# apply inteligent layering to maximize cache hits and reduce image size between builds
COPY --from=build /layer/package ./
COPY --from=build /layer/earlypackage ./
COPY --from=build /layer/project ./
COPY --from=build /layer/app ./
ENTRYPOINT ["bash", "-c", "/etc/init.d/ssh start && watchexec --ignore *.yaml --restart --watch . 'dotnet Tanzu.WebDemo.dll --urls http://0.0.0.0:${PORT}'"]
ENV PORT=8080
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
ENV LOGGING__CONSOLE__DISABLECOLORS=true
EXPOSE 8080
