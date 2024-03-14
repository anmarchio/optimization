using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Mutators
{
    [Serializable]
    public abstract class Mutator : IMutator, ICopyableRandom
    {
        public Mutator(IRandom random)
        {
            Random = random;
        }
        protected IRandom Random { get; set; }

        public abstract ICopyable Copy();

        public abstract ICopyable Copy(IRandom rand);
       
        public abstract IIndividual Mutate(IIndividual individual);
        
        
    }
}
