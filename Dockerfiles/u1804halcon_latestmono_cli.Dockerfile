# This dockerfile is used to create a docker image for usage of the CLI
# It used the latestmono:halcon docker image from the local registry
# so you need a local registry running is well
# with halcon xx.xx installed.
# Use this to build, test, deploy the optimization application
# Unfortunately, COPY only works with files from within the build context.

FROM localhost:5000/ubuntu_latestmono:halcon

COPY . /optimization/

RUN mono /usr/local/bin/nuget.exe restore /optimization/Optimization.Commandline.sln \
    && msbuild /optimization/Optimization.Commandline.sln

ENTRYPOINT ["mono", "/optimization/Optimization.Commandline/bin/Debug/Optimization.Commandline.exe"]
#CMD ["mono", "/optimization/Optimization.Commandline/bin/Debug/Optimization.Commandline.exe"]