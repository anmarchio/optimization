'''
In this example, we'll create a Status Quo pipeline object in python, load an image 
and execute the pipeline on it.
'''
#from pythonnet, this is used to import the PRIME dlls
import clr
clr.AddReference("PRIME.Optimization")
clr.AddReference("PRIME.Optimization.Tests")

from os import path

import PRIME.Optimization as opt
from PRIME.Optimization import Program
from PRIME.Optimization.EvolutionStrategy import EvolutionStrategy
from PRIME.Optimization.Pipeline import CommonPipelines
from PRIME.Optimization.Tests.TestImages import CommonImages

'''
IMPORTANT: halcon seems to cause BadImageFormatException (i.e. licence not recognised when called from python)
if python architecture is 32 bit or ANYCPU (at least RegionMarker causes this exception if compiled for ANYCPU)
However, Monodevelop won't let me target 64 bit exclusively. hopyfully, calling the library from 64 bit python will solve this
'''
p = Program()
img = p.LoadImage(path.join(path.curdir, "..", "..", "TestImages", "1.bmp"))
sq = CommonPipelines.StatusQuo

result = sq.ExecuteSingle(img)

# ESTests is a namespace/folder in Optimization.Tests, BatchRunTests is a class
# first we need to create an object of that class






