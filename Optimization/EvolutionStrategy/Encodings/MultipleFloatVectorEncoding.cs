using System.Collections.Generic;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Encodings
{

    /// <summary>
    /// Float encoding of each column of a genotype. One genotype consists of a list of float vectors for each column (-> List of float vector lists). Each suitable operator in present in every column. 
    /// Advantage: different sized columns
    /// </summary>
    public class MultipleFloatVectorEncoding : IIndividual
    {
        private float[][] vector;

        public MultipleFloatVectorEncoding(int columncount)
        {
            vector = new float[columncount][];
        }

        public MultipleFloatVectorEncoding(MultipleFloatVectorEncoding vector)
        {
            this.vector = new float[vector.vector.Length][];
            for (int i = 0; i < vector.vector.Length; i++)
            {
                this.vector[i] = new float[vector.vector[i].Length];

                for (int j = 0; j < vector.vector[i].Length; j++)
                {
                    this.vector[i][j] = vector.vector[i][j];
                }
            }
            this.Fitness = vector.Fitness;
            MultipleFitnessValues = vector.MultipleFitnessValues;
        }

        public MultipleFloatVectorEncoding(float[][] vector)
        {
            this.vector = new float[vector.Length][];
            for (int i = 0; i < vector.Length; i++)
            {
                this.vector[i] = new float[vector[i].Length];

                for (int j = 0; j < vector[i].Length; j++)
                {
                    this.vector[i][j] = vector[i][j];
                }
            }
        }

        public float this[int i, int j]
        {
            get { return vector[i][j]; }
            set { vector[i][j] = value; }
        }

        public float this[uint i, uint j]
        {
            get { return vector[i][j]; }
            set { vector[i][j] = value; }
        }

        /// <summary>
        /// Return the length of the whole vector (including all columns).
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int Length()
        {

            int length = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                length += vector[i].Length;
            }
            return length;
        }

        /// <summary>
        /// Return the length of the specified column within the vector.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int Length(int i)
        {
            return vector[i].Length;
        }


        public float[][] ToArray()
        {
            return (float[][])vector.Clone();
        }

        MultipleFloatVectorEncoding IIndividual.MultipleFloatVectorEncoding
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
                throw new EncodingException("This Individual uses MultipleFloatVectorEncoding Encoding, not BooleanVector Encoding.");
            }
        }

        public Dictionary<FitnessFunction, double?> Fitness { get; set; }
        public long GetId()
        {
            throw new System.NotImplementedException();
        }

        public FloatVector FloatVector
        {
            get
            {
                throw new EncodingException("Apparently this Individual was considered a FloatVector, yet is actually of type MultipleFloatVectorEncoding");
            }
        }

        public double[] MultipleFitnessValues
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Fitness.ToString() + "\n"
                 + "Length: " + Length() + "\n";
        }

        public bool isEqual(MultipleFloatVectorEncoding comparisonVector)
        {
            bool isEqual = true;

            for (int column = 0; column < vector.Length; column++)
            {
                for (int rows = 0; rows < vector[column].Length; rows++)
                {
                    if (vector[column][rows] != comparisonVector.vector[column][rows])
                    {
                        isEqual = false;
                        goto END;
                    }
                }
            }
        END:
            return isEqual;
        }

        ICopyable ICopyable.Copy()
        {
            return new MultipleFloatVectorEncoding(this);
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new System.NotImplementedException();
        }
    }
}
