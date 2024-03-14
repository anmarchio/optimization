This directory contains a zoo of Dockerfiles that each serve some specific purpose. The canonical build command for each Dockerfile is contained in the file itself.

Generally speaking, the name of the Dockerfile indicates the following:

* u<1804 | 1604> refers to Ubuntu 18.04 or 16.04 (the latter is useful for testing older, buildable EmguCV versions)
* <halcon | cv> refers wo whether Halcon and/or EmguCV have been installed
* [_dotnetcore] refers to whether or not the Image is using the dotnet core instead of the mono framework.


