namespace Optimization.Serialization.Interfaces
{
    public interface ISupportsSerialization
    {
        bool SerializeBinarySupported { get;}
        bool SerializeXmlSupported { get; }

        void SerializeXml(string filename);
        void SerializeBinary(string filename);

    }
}
