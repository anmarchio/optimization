# Wiki

* [Commandline](commandline.md)
* [Programming-Guidelines](programming-guidelines.md)
* [REST communication between Optimization and Webapp](rest.md)
* [Testing-Guidelines](testing.md)
* [Troubleshooting](troubleshooting.md)
* [gitlab runner](gitlab-runner.md)
* [Home](home.md)

## Commandline

The commandline tool is currently the most conventient and easy to use frontend for the CGP optimization library. The tool should navigate you through its usage. This entry only contains a few pointers for best practices.

If any option or parameter is not clear, please open an issue describing which one you would like to be explained more plainly.

## Selecting a Fitness Function

If you want to use a fitness function other than MCC, you'll have to specify it explicitly by using the --fit-func argument. For example ```--fit-func=IntersectionOverUnion``` will use IoU or Jaccard Distance as fitness function. For a list of fitness functions, use 

```
Optimization.Commandline.Exe fit-func
```

The names of fitness functions are case sensitive. This is not to annoy you with having to write the names, but rather due to the fact that the names are generated automatically from the corresponding FitnessFunction enum.

## Specifying Operators

Both currently supported backends offer a large range of image processing operators (~50 each). Not all are suited for all tasks. For example, anisotropic diffusion can consume a lot of compute time, which exceeds the execution time threshold and is subsequently awarded 0 fitness. It is good practice to exclude such operators from evolution by specifying a list of allowed operators. This can be done in the following way:

```
Optimization.Commandline.exe operators --backend=halcon --filename=default.xml
```

You can then open the .xml file and manually remove all operators you do not want to use. This file can then be used for subsequent evolutions via:

```
Optimization.Commandline.exe batch --operators=default.xml <other parameters>
```

## CLI on Deeplearning2

You can also use a docker image on deeplearning2 to run the commandline interface.

``` Console
docker run --network=host -dt --rm -v evias:/evias localhost:5000/cgp batch \
--backend=halcon \ 
--runs=5 \
--train-data-dir=/evias/<train_data> \
--val-data-dir=/evias/<val_data> \
--results-dir=/evias/<your_results> \
--generations=200
```
You must specify the results directory, else the results will not be accessible by you after the container has finished its work. the ```-rm``` flag removes the container after it has finished, ```-d``` detaches it, so it does not block your shell. However, if you want to be updated with the console output, you might wan to use -i instead of -d. ```--network=host``` is required for halcon licensing reasons.

If your data resides in the docker volumes evias: /mnt/sdc/docker/volumes/evias/_data/<your_data>. Else you'll have to mount your data directory by replacing above -v flag with ```<your_data_dir>:<mount point in docker container, must start with / >```. Basically analogously to above example.

### Get the latest version of CLI for Deeplearning2

Execute the CI-Pipeline: ```build:cli-docker-image```. Then log on to deeplearning2 and ```docker pull localhost:5000/cgp``` and you are good to go.