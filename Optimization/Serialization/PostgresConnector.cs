using System;
using System.Data;
using Npgsql;

namespace Optimization.Serialization
{
    public class PostgresConnector : Interfaces.IConnector, IDisposable
    {
        private NpgsqlConnection connection;
        private object mutex = new object();

        public PostgresConnector(string server, string userId, string port, string password, string database)
        {
            string connstring = String.Format("Server={0};Port={1};" +
                    "User Id={2};Password={3};Database={4};",
                    server, port, userId,
                    password, database);
            connection = new NpgsqlConnection(connstring);
            Open();
        }

        public void Open()
        {
            connection.Open();  //Initiate connection to the db
        }

        public void Close()
        {
            connection.Close();
        }

        public DataTable selectQuery(string query)
        {
            lock (mutex)
            {
                NpgsqlDataAdapter ad;
                DataTable dt = new DataTable();

                try
                {
                    ad = new NpgsqlDataAdapter(query, connection);
                    ad.Fill(dt); //fill the datasource
                }
                catch (NpgsqlException)
                {
                    throw;
                }
                return dt;
            }
        }

        public void updateQuery(string query)
        {
            lock (mutex)
            {
                try
                {
                    var cmd = connection.CreateCommand();
                    var statet = connection.FullState;
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                }
                catch (NpgsqlException)
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            if(connection != null)
                connection.Dispose();
        }
    }
}
