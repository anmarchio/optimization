using System.Configuration;
using System.IO;

namespace Optimization
{
    /// <summary>
    /// Set the appropriate values for all variables read by ConfigurationManager in the PRIME.Optimization.StartUp.exe.config located in the
    /// PRIME.Optimization.StartUp folder: e.g. PRIME.Optimization.StartUp/bin/Debug
    /// 
    /// DO NOT EDIT THE VALUES IN App.config
    /// 
    /// this will cause problems if used from within a different program (I suppose) move to StartUp or make robust to missing Config entries
    /// </summary>
    public class Configuration
    {
        public static string PRIMEWebAppPath = ConfigurationManager.AppSettings.Get("PRIMEWebAppPath");
        public static bool UseSQLite = bool.Parse(ConfigurationManager.AppSettings.Get("UseSqlite"));
        public static string DjangoDataPath = ConfigurationManager.AppSettings.Get("DjangoDataPath"); //.Combine(PRIMEWebAppPath, "media", "batch_data"));
        public static string DjangoSavePath = ConfigurationManager.AppSettings.Get("DjangoSavePath"); //Path.Combine(PRIMEWebAppPath, "media", "batch_results"));
        public static string DjangoMediaPath = ConfigurationManager.AppSettings.Get("DjangoMediaPath"); //Path.Combine(PRIMEWebAppPath, "media"));

        public static string SQLiteDBPath = Path.Combine(PRIMEWebAppPath, "db.sqlite3");

        public static string UserID = ConfigurationManager.AppSettings.Get("PostgresUserID");
        public static string Port = ConfigurationManager.AppSettings.Get("PostgresPort");
        public static string Server = ConfigurationManager.AppSettings.Get("PostgresServer");
        public static string Password = ConfigurationManager.AppSettings.Get("PostgresPassword");
        public static string DataBaseName = ConfigurationManager.AppSettings.Get("PostgresDBName");

        public static string HalconFastOperatorsPath = ConfigurationManager.AppSettings.Get("HalconFastOperatorsPath");
    }
}
