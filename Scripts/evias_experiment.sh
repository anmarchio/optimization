#! /bin/bash
# This script uses docker and the optimization CLI to perform a few minor experiments.
#update docker image
docker pull localhost:5000/cgp

for fitness in IntersectionOverUnion MCC
do
	for operators in batteriebleche laplace fast
	do
	    for data in FiberCrack Fuzzball LooseFilament Contaminant Loop
	    do
		  docker run --network=host -it --rm -v evias:/evias localhost:5000/cgp batch \
		             --backend=halcon \
		             --runs=30 \
		             --train-data-dir=/evias/eval_data/Batteriebleche/Dunkelfeld_split_and_sorted/${data}_transformed/train \
		             --results-dir=/evias/script_results/${data}_${operators}_${fitness} \
		             --generations=200 \
		             --operators=/evias/eval_data/Halcon/${operators}.xml \
		             --fit-func=$fitness \
                     --fits-mem=true
	    done
	done
done
