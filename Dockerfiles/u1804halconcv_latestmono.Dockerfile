# This dockerfile is used to create ubuntu 18.04 base halcon images
# It can be used by gitrunner
# with halcon xx.xx installed.
# Use this to build, test, deploy the optimization application
# Unfortunately, COPY only works with files from within the build context.
# Therefore, we need to specify the location of halcon on the host system as build context.
# For example, from within this git-repo:
# docker build /opt/halcon -f u1804halconcv_latestmono.Dockerfile -t local/ubuntu_latestmono:halconcv

FROM ubuntu:18.04

ENV HALCONROOT "/opt/halcon"
ENV HALCONARCH "x64-linux"
# note that this is not recommended, but since this file is not supposed to be public, we'll do it anyway:
ENV HTTP_PROXY http://10.41.64.158:3128
ENV LD_LIBRARY_PATH $HALCONROOT/lib/$HALCONARCH:emgucv/libs
ENV EMGUCV_VERSION 4.1.0


COPY /bin $HALCONROOT/bin
COPY /lib/$HALCONARCH $HALCONROOT/lib/$HALCONARCH
COPY /license $HALCONROOT/license

# install necessary packages
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y apt-transport-https dirmngr git ca-certificates curl python cmake libtiff5-dev libgeotiff-dev libgtk-3-dev libgstreamer1.0-dev libavcodec-dev libswscale-dev libavformat-dev libopenexr-dev libdc1394-22-dev libv4l-dev libeigen3-dev libtbb-dev libtesseract-dev cmake-curses-gui ocl-icd-dev freeglut3-dev && apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && echo "deb https://download.mono-project.com/repo/ubuntu vs-bionic main" | tee /etc/apt/sources.list.d/mono-official-vs.list && apt-get update && DEBIAN_FRONTEND=noninteractive apt-get -y install monodevelop

RUN curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

# now install emgucv
RUN git clone https://github.com/emgucv/emgucv emgucv
WORKDIR emgucv
# it seems that newer emgucv versions might also still support ubuntu 16.04
#RUN git checkout EMGUCV_3_4_3 && git submodule update --init --recursive
RUN git checkout tags/$EMGUCV_VERSION && git submodule update --init --recursive

WORKDIR /emgucv/platforms/ubuntu/16.04
RUN ./cmake_configure.sh
WORKDIR /
