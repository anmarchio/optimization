# This dockerfile is used to create ubuntu 18.04 base halcon images
# It can be used by gitrunner
# with halcon xx.xx installed.
# Use this to build, test, deploy the optimization application
# Unfortunately, COPY only works with files from within the build context.
# Therefore, we need to specify the location of halcon on the host system as build context.
# For example, from within this git-repo:
# docker build /opt/halcon -f u1804halcon.Dockerfile -t local/ubuntu:halcon

FROM ubuntu:18.04

ENV HALCONROOT "/opt/halcon"
ENV HALCONARCH "x64-linux"
# note that this is not recommended, but since this file is not supposed to be public, we'll do it anyway:
ENV HTTP_PROXY http://10.41.64.158:3128
ENV LD_LIBRARY_PATH $HALCONROOT/lib/$HALCONARCH

COPY /bin $HALCONROOT/bin
COPY /lib/$HALCONARCH $HALCONROOT/lib/$HALCONARCH
COPY /license $HALCONROOT/license

# install necessary packages
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y git ca-certificates mono-devel curl python

# only this older nuget version is compatible with the repository version of mono
RUN curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/v4.6.4/nuget.exe

