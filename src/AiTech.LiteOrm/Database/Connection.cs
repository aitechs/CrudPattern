using AiTech.LiteOrm.Encryption;
using System;
using System.Data.SqlClient;
using System.IO;

namespace AiTech.LiteOrm.Database
{
    public sealed class Connection
    {
        public static DatabaseCredential MyDbCredential;
        private static bool CredentialIsLoaded;

        public static void LoadCredential()
        {
            var settings = new System.Xml.XmlDocument();

            if (!File.Exists("credentials.xml")) throw new FileNotFoundException("Credential File NOT Found!");

            settings.Load("credentials.xml");

            var rootNode = settings.SelectSingleNode("Settings");

            var node = rootNode.SelectSingleNode("Connection");
            if (node == null) throw new Exception("Can not Read Connection Settings");

            MyDbCredential.ServerName = node.Attributes["ServerIp"].Value;
            MyDbCredential.DatabaseName = node.Attributes["Database"].Value;
            MyDbCredential.Username = Password.Decrypt(node.Attributes["Username"].Value);
            MyDbCredential.Password = Password.Decrypt(node.Attributes["Password"].Value);

            //var isec = node.Attributes["IntegratedSecurity"];

            MyDbCredential.IntegratedSecurity = node.Attributes["IntegratedSecurity"]?.Value == "true" ? true : false;

            CredentialIsLoaded = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection CreateConnection()
        {
            //System.Threading.Thread.Sleep(2000);

            if (!CredentialIsLoaded) LoadCredential();

            if (string.IsNullOrEmpty(MyDbCredential.DatabaseName)) throw new Exception("Database Credential NOT set!");

            var credential = MyDbCredential;

            var builder = new SqlConnectionStringBuilder()
            {
                DataSource = credential.ServerName,
                InitialCatalog = credential.DatabaseName,
                UserID = credential.Username,
                Password = credential.Password,
                IntegratedSecurity = credential.IntegratedSecurity,
                MultipleActiveResultSets = true
            };

            return new SqlConnection(builder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static SqlConnection CreateAndOpenConnection()
        {
            var db = CreateConnection();
            try
            {
                db.Open();

            } catch(Exception ex)
            {
                throw new IOException("Can NOT connect to the server", ex);
            }
            return db;
        }

    }
}
