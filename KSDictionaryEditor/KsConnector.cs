using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDictionaryEditor
{
    class KsConnector
    {
        public static string getKsPlIniFilePath()
        {
            List<string> registryPaths = new List<string>() {
                "HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\KAMSOFT",
                "HKEY_LOCAL_MACHINE\\SOFTWARE\\KAMSOFT",
                "HKEY_CURRENT_USER\\SOFTWARE\\WOW6432Node\\KAMSOFT",
                "HKEY_CURRENT_USER\\SOFTWARE\\KAMSOFT", };
            const string registryName = "KSPL_SCIEZKA";
            const string nullValue = "NoPath";

            string registryValue = "";

            foreach (var path in registryPaths)
            {
                registryValue = (string)Registry.GetValue(path,
                registryName,
                nullValue);
                if (registryValue != null)
                {
                    return registryValue;
                }
            }

            return nullValue;

        }

        public static string getConnectionString(string path)
        {
            path = path + "\\kspl.ini";

            string connectionString = "";

            FileStream file = File.Open(path, FileMode.Open);

            
            return connectionString;
        }
    }
}
