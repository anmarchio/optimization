"""
This script takes an input directory in typical image/label format, i.e.

dir
    labels
            image_a
            ...
    images
            image_a
            ...

and splits it into train, test and validation sets.

It also splits the test set further into very small sets of 1, 5, 10 for a total of 5 runs each and copies the
appropriate files into their own directories.
"""
from sklearn.model_selection import train_test_split
import os
import sys
from pathlib import Path
import shutil
from contextlib import suppress
import numpy as np

input_folder = sys.argv[1]
output_folder = sys.argv[2]

label_dir_name = "labels"
image_dir_name = "images"

label_dir = os.path.join(input_folder, label_dir_name)
image_dir = os.path.join(input_folder, image_dir_name)

label_paths = [Path(os.path.join(label_dir, x)) for x in os.listdir(label_dir) if not x.endswith('.db')]
image_paths = [Path(os.path.join(image_dir, x)) for x in os.listdir(image_dir) if not x.endswith('.db')]
image_paths.sort()
label_paths.sort()
pairs = zip(image_paths,label_paths)

train, rest = train_test_split(list(pairs), test_size=0.4, shuffle=True)
val, test = train_test_split(rest, test_size=0.5, shuffle=True)

with suppress(FileExistsError):
    for x in ["train", "val", "test"]:
        for y in ["images", "labels"]:
            os.makedirs(os.path.join(output_folder, x, y))


for d, x in zip(["train", "val", "test"], [train, val, test]):
    for image, label in x:
        assert image.stem == label.stem, f"{image.stem} != {label.stem}"
        shutil.copyfile(image,
                        os.path.join(output_folder, d, "images", image.name))
        shutil.copyfile(label,
                        os.path.join(output_folder, d, "labels", label.name))


train_images_new = [Path(os.path.join(output_folder, "train", "images", x))
                  for x in os.listdir(os.path.join(output_folder, "train", "images")) if not x.endswith('.db')]
train_images_new.sort()
train_labels_new = [Path(os.path.join(output_folder, "train", "labels", x))
                  for x in os.listdir(os.path.join(output_folder, "train", "labels")) if not x.endswith('.db')]
train_labels_new.sort()

train_paths = np.array(list(zip(train_images_new, train_labels_new)))

assert all([x.stem == y.stem for x, y in train_paths])

num_runs = 5
for size in [1, 3, 5, 10, 25, 50]:
    indices = np.random.choice(range(0, len(train_paths)), size=size*num_runs, replace=False)
    for run, chunk in zip(range(0, num_runs), np.array_split(indices, num_runs)):
        out_dir = os.path.join(output_folder, "train_experiment", str(size), str(run), "train")
        with suppress(FileExistsError):
            for x in ["images", "labels"]:
                os.makedirs(os.path.join(out_dir, x))
        for image, label in [train_paths[i] for i in chunk]:
            assert image.stem == label.stem, f"{image.stem} != {label.stem}"
            shutil.copyfile(image, os.path.join(out_dir, "images", image.name))
            shutil.copyfile(label, os.path.join(out_dir, "labels", label.name))

        assert len(os.listdir(os.path.join(out_dir, "labels"))) == len(os.listdir(os.path.join(out_dir, "images")))

