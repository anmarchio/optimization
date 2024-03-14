'''
This is a minimal working example for calling a method from
PRIME.Optimization via python using pythonnet

this has been tested on windows 7
'''
#from pythonnet, this is used to import the PRIME dlls
import clr 

'''
IMPORTANT: the cryptic message "could not find assembly" is usually caused 
by missing dependencies. these may even be "hidden" dependencies, in which
case more elaborate dependency analysis tools are required to figure out
which ones are missing exactly.

IMPORTANT: the cryptic message "no module found" is usually caused by all libraries
not targeting the same cpu type. below code worked for build target ANY CPU
'''

clr.AddReference("PRIME.Optimization.Tests")
import PRIME.Optimization.Tests as tsts

# ESTests is a namespace/folder in Optimization.Tests, BatchRunTests is a class
# first we need to create an object of that class
batch = tsts.ESTests.BatchRunTests()

# call BeamDeflectionPiBatchRun(), which wraps data loading and all the nasty
# Evolution Strategy stuff
batch.BeamDeflectionPiBatchRun()
# you should see the usual batch run output on your terminal
# above code executes a test from the test project
# the output files should be in the corresponding directory



