using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.CartesianGeneticProgramming;
using MathNet.Numerics.LinearAlgebra.Solvers;
using System.IO;

namespace Optimization.EvolutionStrategy.Encodings
{
    /// <summary>
    /// Recommended encoding type for CGP (really, is there any actual downside from not using int?)
    /// </summary>
    [Serializable]

    public class FloatVector : IIndividual
    {
        public float[] vector;

        public FloatVector(int length)
        {
            vector = new float[length];
        }

        public FloatVector(FloatVector vector)
        {
            this.vector = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++) this.vector[i] = vector[i];
            this.Fitness = vector.Fitness;
        }

        public FloatVector(float[] vector)
        {
            this.vector = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++) this.vector[i] = vector[i];
        }

        public float this[int i]
        {
            get { return vector[i]; }
            set { vector[i] = value; }
        }
        public float this[uint i]
        {
            get { return vector[i]; }
            set { vector[i] = value; }
        }

        public int Length
        {
            get
            {
                return vector.Length;
            }
        }
        

        public float[] ToArray()
        {
            return (float[])vector.Clone();
        }

        FloatVector IIndividual.FloatVector
        {
            get
            {
                return this;
            }
        }

        public BooleanVector BooleanVector
        {
            get
            {
                throw new EncodingException("This Individual uses FloatVector Encoding, not BooleanVector Encoding.");
            }
        }

        public Dictionary<FitnessFunction, double?> Fitness { get; set; }

        public MultipleFloatVectorEncoding MultipleFloatVectorEncoding
        {
            get
            {
                throw new EncodingException("Apparently this Individual was considered a MultipleFloatVectorEncoding, yet is actually of type FloatVector");
            }
        }

        public override string ToString()
        {
            return Fitness.ToString() + "\n"
                 + "Length: " + Length + "\n";
                

        }

        public ICopyable Copy()	//	Leo's variant
        {
            return new FloatVector(this);        
        }
        
        private static ObjectIDGenerator objectIdGenerator = null;

        private static ObjectIDGenerator ObjectIDGenerator
        {
            get
            {
                if (objectIdGenerator == null)
                    objectIdGenerator = new ObjectIDGenerator();
                return objectIdGenerator;
            }
        }

        public long GetId()
        {
            bool firstTime;
            return ObjectIDGenerator.GetId(this, out firstTime);
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }

        public class FloatVectorEqualityComparer : IEqualityComparer<FloatVector>
        {
            public bool Equals(FloatVector x, FloatVector y)
            {
                if (x.Length != y.Length) return false;
                return Enumerable.Range(0, y.Length).All(idx => x[idx] == y[idx]);
            }

            public int GetHashCode(FloatVector obj)
            {
                return (int)Math.Log10(obj.ToArray().Sum());
            }
        }
    }
}
