# Wiki

* [Commandline](commandline.md)
* [Programming-Guidelines](programming-guidelines.md)
* [REST communication between Optimization and Webapp](rest.md)
* [Testing-Guidelines](testing.md)
* [Troubleshooting](troubleshooting.md)
* [gitlab runner](gitlab-runner.md)
* [Home](home.md)

## Gitlab-Runner

Here we describe how to set up a gitlab runner to serve build/test pipelines with a dedicated halcon license.

Prerequisites:
* a working gitlab-runner installation: https://docs.gitlab.com/runner/install/
* a working Docker installation: https://docs.docker.com/install/
* a working Halcon installation: see Halcon install manual; in particular the section on installing on linux

### Building the image

This repository contains a zoo of Dockerfiles that can be used to build a docker image. The one the main ci-pipeline requires is
Dockerfiles/u1804halcon_latestmono.Dockerfile

The filename indicates that the base image is Ubuntu 18.04 with halcon and the latest version of mono installed.

In the future, when we get EmguCV running properly, we'll need the u1804halconcv_latestmono.Dockerfile, but currently EmguCV is not building properly on ubuntu 18.04

Each file contains an example as to how to build the image and how to properly tag it for it to work with the ci-pipeline.

### Configuring gitlab-runner.service

First, follow the gitlab instructions to set this runner as the default runner for a repository. This is done via adding a token to the config file in 

/etc/gitlab-runner/config.toml

which can be found in the repository settings.

Halcon server licenses are bound to a specific mac-address. For this to work with docker containers, we need to:
* pass `--network host` as parameter to docker run
* set network_mode = "host" in the gitlab-runner config, under the [runners.docker] section

Some pipelines also require access to a shared volume.

The "token" below does *not* refer to the project registration token, which you will need during registering your runner with the project.

```toml

concurrent = 1
check_interval = 0

[session_server]
  session_timeout = 1800

[[runners]]
  name = "deeplearning2"
  url = "https://gitlab.cc-asp.fraunhofer.de/"
  token = "Xs9NszbiGmKwZCgxSs_o"
  executor = "docker"
  [runners.custom_build_dir]
  [runners.docker]
    tls_verify = false
    image = "local/ubuntu:halcon"
    privileged = false
    disable_entrypoint_overwrite = false
    oom_kill_disable = false
    disable_cache = false
    volumes = ["/cache",
                "evias:/evias"]
    shm_size = 0
    pull_policy = "if-not-present"
    network_mode = "host"
    memory = "20GB"
  [runners.cache]
    [runners.cache.s3]
    [runners.cache.gcs]

```
### Debugging new images

All images can also be started interactively e.g. with `docker run -it --network host -v=evias:/evias/ <image_name>`, which will make the container use the host network (thus allowing is to use Halcon within the container) and mount the folder evias from within the docker/volumes/ directory.

Containers cane be started and entered with `docker start <container_name>` and `docker exec <container_name> /bin/bash`

### Running Gitlab-Runner as Docker Container

If you want to use a gitlab runner inside a docker container, simply pass above config file when starting the dontainer.

``` docker run -d --name optimization-gitlab-runner --restart always -v /etc/gitlab-runner/optimization:/etc/gitlab-runner -v /var/run/docker.sock:/var/run/docker.sock gitlab/gitlab-runner:latest ```