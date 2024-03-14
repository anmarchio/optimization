using System;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Mutators
{
    [Serializable]
    public class BooleanVectorMutator : Mutator
    {
        public BooleanVectorMutator(IRandom random) : base(random)
        {

        }

        public BooleanVectorMutator(IRandom random, int? maxTrueCount) : base(random)
        {
            MaxTrueCount = maxTrueCount;
        }

     
        private int? MaxTrueCount { get; set; }

        public override ICopyable Copy()
        {
            return new BooleanVectorMutator(Random);
        }

        public override ICopyable Copy(IRandom rand)
        {
            return new BooleanVectorMutator(rand, MaxTrueCount);
        }

        public override IIndividual Mutate(IIndividual individual)
        {
            var boolVector = new BooleanVector(individual.BooleanVector);

            var idx = Random.Next(boolVector.Length);
            boolVector[idx] = !boolVector[idx];

            if(MaxTrueCount != null)
            {
                var trueCount = boolVector.TrueCount;
                if(trueCount > MaxTrueCount)
                {
                    var falseIdx = Random.Next(trueCount);
                    var searchIdx = 0;
                    for(int i = 0; i < boolVector.Length; i++)
                    {
                        if (!boolVector[i] && searchIdx < falseIdx) falseIdx++;
                        else if (boolVector[i]) boolVector[i] = !boolVector[i];
                    }                    
                }
            }

            return boolVector;
        }
    }
}
