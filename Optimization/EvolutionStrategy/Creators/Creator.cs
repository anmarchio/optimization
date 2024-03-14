using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Creators
{
    [Serializable]

    public abstract class Creator : ICreator, ICopyable
    {
        public Creator(IRandom random, int length)
        {
            Random = random;
            Length = length;
        }
        protected IRandom Random { get; set; }

        public int Length { get; protected set; }

        public abstract IIndividual Create();

        public abstract ICopyable Copy();

        public abstract ICopyable Copy(IRandom rand);
    }
}
