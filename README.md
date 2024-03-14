# Image Processing Pipeline Optimization

This set of libraries uses evolutionary computing to automatically generate image processing pipelines.

Currently supported frameworks:
* OpenCV
* Halcon

## First Steps

Clone the repository:

```git clone https://gitlab.cc-asp.fraunhofer.de/evias/optimization.git```

I'm assuming you have a working installation on VisualStudio (2017+). Open the **Optimization.sln** to see all project files involved. If you want to work with only one backend, feel free to open the backend-specific solutions.

If you want to use the Halcon backend, see the section on [Halcon](##halcon).

Use NuGet to restore the missing packages. Either in VisualStudio or via nuget restore. Check [here](##Ubuntu), if working with Ubuntu.

### Halcon

You'll need both an installation of some Halcon framework as well as a license. Contact mara or leen if you need help.
The following steps may depend on properly configured Halcon environment variables. For details look at the Halcon installation manual. In particular, the `HALCONROOT`, `HALCONARCH` and `LD_LIBRARY_PATH` variables must be set:

```export LD_LIBRARY_PATH=$HALCONROOT/lib/$HALCONARCH```

If you are using the latest development version, then you should not have to adjust the halcon references, as all project files point to the HALCONROOT environment variable.
If you are using some older version of this project, you might want to run the ```python Scripts/fix_halcon_reference.py``` script in order to adjust the .csproj xml accordingly. The old version of this script wrote the value of HALCONROOT in the appropriate sections of the .csproj files, the newer version of this script just writes $HALCONROOT.

#### Troubleshooting

BadImageFormatExceptions may be caused by missing licenses. Also, when running tests, make sure to compile against the x64 architecture, else you will also get a BadImageFormatException.

### OpenCV

At the moment we depend on EmguCV as a C# wrapper for OpenCV. There are a number of issues with that, so we are considering moving to another wrapper instead. However, for the time being, you will have to build EmguCV if you want to run this project on Linux. Windows should work fine with NuGet.

## Commandline Interface

The application exposes rudimentary functionality via the commandline interface. After building the CLI, run for example:

``` Optimization.Commandline.exe batch --backend=halcon --runs=5 --train-data-dir=<abs-path-train-data-parent-dir> --val-data-dir=<abs-path-val-data-parent-dir> --generations=200 ```

Or using mono:

``` mono Optimization.Commandline.exe batch --backend=halcon --runs=5 --train-data-dir=<abs-path-train-data-parent-dir> --val-data-dir=<abs-path-val-data-parent-dir> --generations=200 ```

Both data directories must be of the format parent\images, parent\labels with both names in images and labels matching. labels only accepts .pngs, images should accept .jpg, .bmp, .png.

```
dataset
|__images
|  |__image1.bmp
|  |__image2.bmp
|  |  ...
|
|__labels
|  |__image1.png
|  |__image2.png
|  |  ...
```

If you used RegionMarker to label the images, you might want to use ```Optimization.Commandline.exe convert``` to convert it to above format, though the application should be able to handle data from RegionMarker directly.

If you want to run the CLI on deeplearning2, consult the [Wiki](https://gitlab.cc-asp.fraunhofer.de/evias/optimization/-/wikis/Commandline#cli-on-deeplearning2).

The location of the resulting images, pipeline.xmls and logging data can be configured using ```--results-dir```. The location defaults to the directory of ```Optimization.Commandline.exe```.
If you are using the Debug configuration in VisualStudio, this will be ```Optimization.Commandline/bin/Debug/```. The (default) result directory is structured as follows:

```
<date-time of start>
|__Analyzer // logging data for each batch run
|  |__0
|  |__1
|  ...
|__Config // cgp config, evolution strategy config, etc.
|  |__0
|  ...
|__Grid // pipelines as grid.txt, pipeline.xml, pipeline.dot, etc.
|  |__0
|  ...
|__Images
|  |__0 // result images of batch run 0
|  |__1 // etc.
|  |__2
|  ...
|__date.txt // the start time of each evolution strategy
|__overview.txt // top individual's fitness values
|__seed.txt // the seed used for the batch run
|__validation.txt // same as overview, but with validation data
```

## Further Reference

* [Wiki](https://gitlab.cc-asp.fraunhofer.de/evias/optimization/-/wikis/home)
* [API] ...

## Ubuntu

On Ubuntu 18.04, the nuget installed via official repository is outdated and incapable of solving all references required by this project.
Updating nuget to anything beyond 4.6.4 may break nuget, as it depends on .dlls not available using the mono version supplied by the official ubuntu repos. Using the most recent mono version from the developer's repository causes incompatibility issues with Ubuntu 18.04.
Thus, use nuget as follows (on Linux; on Windows everything should work just fine from within VisualStudio):

```curl -o /usr/local/bin/nuget.exe https://dist.nuget.org/win-x86-commandline/v4.6.4/nuget.exe ```

```alias nuget="mono /usr/local/bin/nuget.exe"```

afterwards you should be able to use ```nuget restore <.sln>``` as usual.

