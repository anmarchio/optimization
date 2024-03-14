using System;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Uses threshold maxAttempts (default 500) to try and produce valid (non-threshold-breaching) individuals for CGP.
    /// </summary>
    [Serializable]
    public class CGPValidFloatVectorCreator : ICreator
    {

        private int maxAttempts;
        private CGPConfiguration configuration;
        private IValidityTester evaluator;
        private IRandom random;
        public CGPValidFloatVectorCreator(IRandom random, CGPConfiguration configuration, IValidityTester evaluator, int maxAttempts = 500) 
        {
            this.configuration = configuration;
            this.evaluator = evaluator;
            this.random = random;
            this.maxAttempts = maxAttempts;
        }

        public IIndividual Create()
        {
            int attempts = 0;
            var valid = false;
            FloatVector vector = null;
            while (!valid && attempts < maxAttempts)
            {
                vector = CGPFloatVectorCreator.Apply(random, configuration);
                valid = evaluator.IsValid(vector);
                attempts++;
            }
            return vector;
        }

        public ICopyable Copy()
        {
            return new CGPValidFloatVectorCreator(random, configuration, evaluator, maxAttempts);
        }

        public ICopyable Copy(IRandom rand)
        {
            return new CGPValidFloatVectorCreator(rand, configuration, evaluator, maxAttempts);
        }
    }
}
