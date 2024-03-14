import os
import sys
from pathlib import Path
from contextlib import suppress
from shutil import copyfile

"""
This script splits datasets in regionmarker-format into different datasets in regionmarger-format based on the classes.
Each class gets its own dataset. This way we can easily build individual pipelines for each class
"""

input_directory = Path(sys.argv[1])

if len(sys.argv) > 2:
    output_directory = sys.argv[2]
else:
    output_directory = os.path.join(input_directory.parent, input_directory.name + "_split_and_sorted")
    
def create_dir(d):
    with suppress(FileExistsError):
        os.makedirs(d)
        
        
create_dir(output_directory)
    
if not all([x in os.listdir(input_directory) for x in ["images", "regions"]]):
    raise FileNotFoundError("Can only handle region marker data (Halcon-based)")

regions_path = os.path.join(input_directory, "regions")
images_path = os.path.join(input_directory, "images")

def get_image_path(region_dir_name):
    return [Path(os.path.join(images_path, x)) for x in os.listdir(images_path) if region_dir_name == Path(x).stem][0]

for region_dir_name in os.listdir(regions_path):

#try:
    region_dir = os.path.join(regions_path, region_dir_name)
    classes = set([z.split('_')[0] for z in os.listdir(region_dir)])
    
    for c in classes:
        class_regions_path = os.path.join(output_directory, c, "regions", region_dir_name)
        class_images_path = os.path.join(output_directory, c, "images")
        create_dir(class_regions_path)
        create_dir(class_images_path)
        image_path = get_image_path(region_dir_name)
        # image only gets copied if it contains one of the classes
        copyfile(image_path, os.path.join(class_images_path, image_path.name))
        
        for z in [x for x in os.listdir(region_dir) if c in x]:
            z_path = Path(os.path.join(region_dir, z))
            copyfile(z_path, os.path.join(class_regions_path, z_path.name))

        assert len(os.listdir(os.path.join(output_directory, c, "regions"))) == len(os.listdir(class_images_path))
#except Exception as e:
#    print(e)
        

        
        
        
        




