using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Creators
{
    public class BooleanVectorCreator : Creator
    {
        public BooleanVectorCreator(IRandom random, int length) : base(random, length)
        {

        }

        public BooleanVectorCreator(IRandom random, int length, int maxTrueCount) : base(random, length)
        {
            MaxTrueCount = maxTrueCount;
        }

        public int? MaxTrueCount { get; private set; }

       
        public override ICopyable Copy()
        {
            if (MaxTrueCount == null)
                return new BooleanVectorCreator(Random, Length);
            else
                return new BooleanVectorCreator(Random, Length, (int) MaxTrueCount);
        }

        public override ICopyable Copy(IRandom rand)
        {
            if (MaxTrueCount == null)
                return new BooleanVectorCreator(rand, Length);
            else
                return new BooleanVectorCreator(rand, Length, (int)MaxTrueCount);
        }

        public override IIndividual Create()
        {
            var vector = new BooleanVector(Length);

            if (MaxTrueCount != null)
            {
                for (int i = 0; i < MaxTrueCount; i++)
                {
                    var idx = Random.Next(Length);
                    vector[idx] = !vector[idx];
                }
            }
            else
            {
                for(int i = 0; i < Length; i++)
                {
                    vector[i] = Random.Next(1) > 0 ? true : false;
                }
            }
            return vector;
        }
    }
}
