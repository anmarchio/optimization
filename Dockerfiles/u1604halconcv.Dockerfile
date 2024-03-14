# This dockerfile is used to create ubuntu 18.04 base halcon images
# It can be used by gitrunner
# with halcon xx.xx installed (in the default location recommendd by the halcon
# installation guide).
# Use this to build, test, deploy the optimization application
# Unfortunately, COPY only works with files from within the build context.
# Therefore, we need to specify the location of halcon on the host system as build context.
# For example, from within this git-repo:
# docker build /opt/halcon -f u1604halconcv.Dockerfile -t local/ubuntu:u1604halconcv

FROM ubuntu:16.04

ENV HALCONROOT "/opt/halcon"
ENV HALCONARCH "x64-linux"
# note that this is not recommended, but since this file is not supposed to be public, we'll do it anyway:
# we could also move the environment initialization to an .env file and not commit
# it, but then this file would not be self-contained any longer...
# if this ever were to become public, the HTTP_PROXY line should be removed
ENV HTTP_PROXY http://10.41.64.158:3128
ENV LD_LIBRARY_PATH $HALCONROOT/lib/$HALCONARCH:/emgucv/libs
ENV EMGUCV_VERSION 4.1.0

COPY /bin $HALCONROOT/bin
COPY /lib/$HALCONARCH $HALCONROOT/lib/$HALCONARCH
COPY /license $HALCONROOT/license

# install necessary packages (partially copied from apt_install_dependencies.sh
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y git ca-certificates cmake curl python build-essential monodevelop mono-mcs python3 libtiff5-dev libgeotiff-dev libgtk-3-dev libgstreamer1.0-dev libavcodec-dev libswscale-dev libavformat-dev libopenexr-dev libjasper-dev libdc1394-22-dev libv4l-dev libeigen3-dev libtbb-dev libtesseract-dev cmake-curses-gui ocl-icd-dev freeglut3-dev 

# only this older nuget version is compatible with the ubuntu repository version of mono
RUN curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/v4.6.4/nuget.exe

# now install emgucv
RUN git clone https://github.com/emgucv/emgucv emgucv
WORKDIR emgucv
# it seems that newer emgucv versions might also still support ubuntu 16.04
#RUN git checkout EMGUCV_3_4_3 && git submodule update --init --recursive
RUN git checkout tags/$EMGUCV_VERSION && git submodule update --init --recursive

WORKDIR /emgucv/platforms/ubuntu/16.04
RUN ./cmake_configure.sh
WORKDIR /
