using System.Data;

namespace Optimization.Serialization.Interfaces
{
    public interface IConnector
    {
        void updateQuery(string query);
        DataTable selectQuery(string query);
        void Close();
    }
}
