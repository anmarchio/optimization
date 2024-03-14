# Data Handling

The `IO` directory is our go-to place for reading data and writing results according to the following convention:

## General Information

### Train and Validation sets
* (The data has to be divided into a `train` and `validation` set.)
* The standard path for input data is `./IO/train` and `./IO/val`
* The application will check `train` folder first for training data files. So far, it allows to process image (png,bmp,jpg) and time series (CSV) data.
* The `val` folder expects the same format as `train`. Validation data is used to evaluate the evolved solutions.
* The input paths can be changed via the flags `--train-data-dir=<your-training-data> --val-data-dir=<your-validation-data>` when executing the `main.py`,  (i.e. `--train-data-dir=.\Test\Paderborn --val-data-dir=.\Test\Paderborn`). The provided path has to point to the directory containing the `train`/`val` folder.
* **Currently, only the train set is used!**

### Input Structure: Dataset and Labels
* The data has to be placed in a sub-folder of the `train`/`val` folder. The name depends on the datatype (i.e. `series`, `images`).
* The data has to be labeled. Labels are placed in the `labels` sub-folder.
* Each data file needs a corresponding label file. The names **without** the filetype need to be __identical__!
* The `images`folder accepts `.png|.jpg|.bmp`
* Make sure all `labels` are provided  as `.png`!
```
|__train / val
|  |__images
|  |  |__image1.jpg|.png|.bmp
|  |  |__image2.jpg|.png|.bmp
|  |  |__image3.jpg|.png|.bmp
|  |__labels
|  |  |__image1.png
|  |  |__image2.png
|  |  |__image3.png
```

### Result Output
The results folder contains the following subfolders:
```
IO
|__results
|  |__YYYYMMDD-HHMMSS
|  |  |__exceptions
|  |  |__Analyzer
|  |  |__Config
|  |  |__Grid
|  |  |__Images
|  |  ...
```