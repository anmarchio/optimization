using System;
using System.IO;
using System.Xml.Serialization;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy
{
    [Serializable]
    public class ESConfiguration : IConfiguration
    {
        public ESConfiguration()
        {
            Mu = 1;
            Lambda = 4;
            PlusSelection = true;
        }

        public ESConfiguration(int mu, int lambda, int rho, bool plus)
        {
            if (mu > lambda) throw new Exception("Population size(mu) must not be larger than offspring (lambda)");
            if (mu == 0 || lambda == 0) throw new Exception(string.Format("Lambda {0} or Mu {1} are zero, i.e. there will be no population or no offspring.", mu, lambda));
            Rho = rho;
            Lambda = lambda;
            PlusSelection = plus;
            Mu = mu;
        }

        public ESConfiguration(int mu, int lambda, bool plus)
        {
            if (mu > lambda) throw new Exception("Population size(mu) must not be larger than offspring (lambda)");
            if (mu == 0 || lambda == 0) throw new Exception(string.Format("Lambda {0} or Mu {1} are zero, i.e. there will be no population or no offspring.", mu, lambda));
            Lambda = lambda;
            PlusSelection = plus;
            Mu = mu;
        }

  
        public int Rho
        {
            get; set;
        }

        public int Lambda
        {
            get; set;
        }

        public bool PlusSelection
        {
            get; set;
        }
        
        public int Mu { get; set; }

      
        public ConfigurationType ConfigurationType
        {
            get
            {
                return ConfigurationType.EvolutionStrategy;
            }
        }

        public bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }

        public bool SerializeXmlSupported
        {
            get
            {
                return true;
            }
        }

        public override string ToString()
        {
            return "Mu: " + Mu + " Lambda: " + Lambda + " Plus: " + PlusSelection;
        }

        internal void Print(string configDirectory)
        {
            using (var writer = new StreamWriter(configDirectory + "\\" + "ESConfiguration.txt"))
            {
                writer.WriteLine("Mu:" + Mu);
                writer.WriteLine("Lambda: " + Lambda);
                writer.WriteLine("PlusSelection: " + PlusSelection);
                writer.WriteLine("Rho: " + Rho);
            }
        }

        public void SerializeXml(string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                var xml = new XmlSerializer(typeof(ESConfiguration));
                xml.Serialize(writer, this);
            }
        }

        public void SerializeBinary(string filename)
        {
            throw new NotSupportedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var tmp = obj as ESConfiguration;
            if (tmp == null) return false;
            if (Rho != tmp.Rho) return false;
            if (Mu != tmp.Mu) return false;
            if (Lambda != tmp.Lambda) return false;
            if (PlusSelection != tmp.PlusSelection) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return Rho * Mu * Lambda;
        }
    }
}
