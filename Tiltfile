os.putenv ('DOCKER_BUILDKIT' , '1' )
isWindows = True if os.name == "nt" else False

name = 'fjb4/dotnet-accelerator'
expected_ref = "%EXPECTED_REF%" if isWindows else "$EXPECTED_REF"
rid = "ubuntu.18.04-x64"
configuration = "Debug"
isWindows = True if os.name == "nt" else False

local_resource(
  'live-update-build',
  cmd= 'dotnet publish src/Tanzu.WebDemo --configuration ' + configuration + ' --runtime ' + rid + ' --self-contained false --output ./src/Tanzu.WebDemo/bin/.buildsync',
  deps=['./src/Tanzu.WebDemo/bin/' + configuration],
  ignore=['./src/Tanzu.WebDemo/bin/**/' + rid]
)

custom_build(
        name,
        'docker build . -f ./src/Tanzu.WebDemo/Dockerfile -t ' + expected_ref,
        deps=["./src/Tanzu.WebDemo/bin/.buildsync", "./src/Tanzu.WebDemo/Dockerfile", "./config"],
        live_update=[
            sync('./src/Tanzu.WebDemo/bin/.buildsync', '/app'),
            sync('./config', '/app/config'),
        ]
    )





k8s_yaml(['kubernetes/deployment.yaml'])
k8s_resource('dotnet-accelerator', port_forwards=[8080,22])