using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Encodings
{
    [Serializable]

    public class BooleanVector : IIndividual
    {
        private bool[] vector;

        public BooleanVector(int length)
        {
            vector = new bool[length];
        }

        public BooleanVector(BooleanVector vector)
        {
            this.vector = new bool[vector.Length];
            for (int i = 0; i < vector.Length; i++) this.vector[i] = vector[i];
            Fitness = vector.Fitness;
        }

        public BooleanVector(bool[] vector)
        {
            this.vector = new bool[vector.Length];
            for (int i = 0; i < vector.Length; i++) this.vector[i] = vector[i];
        }

        public bool this[int i]
        {
            get { return vector[i]; }
            set { vector[i] = value; }
        }
        public bool this[uint i]
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


        public bool[] ToArray()
        {
            return (bool[])vector.Clone();
        }

        BooleanVector IIndividual.BooleanVector
        {
            get
            {
                return this;
            }
        }

        public Dictionary<FitnessFunction, double?> Fitness { get; set; }
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

        public FloatVector FloatVector
        {
            get
            {
                throw new EncodingException("Apparently this Individual was considered a FloatVector, yet is actually of type BooleanVector");
            }
        }
        
        public MultipleFloatVectorEncoding MultipleFloatVectorEncoding
        {
            get
            {
                throw new EncodingException("Apparently this Individual was considered a MultipleFloatVectorEncoding, yet is actually of type BooleanVector");
            }
        }

        public ICopyable Copy()
        {
            return new BooleanVector(this);
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }

        public int TrueCount
        {
            get
            {
                return vector.Where(x => x == true).Count();
            }
        }

        public bool[] Vector
        {
            get
            {
                return vector;
            }          
        }

    
    }
}
