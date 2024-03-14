using System.Data;
using System.Data.SQLite;

namespace Optimization.Serialization
{
    public class SqliteConnector : Interfaces.IConnector
    {
        private SQLiteConnection sqlite;

        public SqliteConnector(string path_to_db)
        {
            sqlite = new SQLiteConnection(@"Data Source=" + path_to_db + ";New=False");
            Open();
        }

        public void Open()
        {
            sqlite.Open();  //Initiate connection to the db
        }

        public void Close()
        {
            sqlite.Close();
        }

        public DataTable selectQuery(string query)
        {
            SQLiteDataAdapter ad;
            DataTable dt = new DataTable();

            try
            {
                SQLiteCommand cmd;        
                cmd = sqlite.CreateCommand();
                cmd.CommandText = query;  //set the passed query
                ad = new SQLiteDataAdapter(cmd);
                ad.Fill(dt); //fill the datasource
            }
            catch (SQLiteException)
            {
                throw;
            }
            return dt;
        }

        public void updateQuery(string query)
        {
            try
            {
                SQLiteCommand cmd;
                cmd = sqlite.CreateCommand();
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException)
            {
                throw;
            }
        }

    }
}
