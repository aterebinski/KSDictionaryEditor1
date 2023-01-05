using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            string providerParameter = "";
            string serverParameter = "";
            string databaseParameter = "";
            string userParameter = "";
            string passwordParameter = "";
            path = path + "\\kspl.ini";
            string connectionString = "";

            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                Trace.WriteLine(line);
                Trace.WriteLine(line.IndexOf("="));

                int index = line.IndexOf("=");

                if (index > -1)
                {
                    string parameter = line.Substring(0, index);

                    switch (parameter)
                    {
                        case "PROVIDER":
                            providerParameter = line.Substring(index+1);
                            //MessageBox.Show("aaa "+providerParameter);
                            break;
                        case "DATABASE":
                            databaseParameter = line.Substring(index + 1);
                            break;
                        case "SERVER":
                            serverParameter = line.Substring(index + 1);
                            break;
                        case "XUSER":
                            userParameter = line.Substring(index + 1);
                            break;
                        default:
                            break;
                    }
                }
                
            }

            PasswordWindow passwordWindow = new PasswordWindow();
            passwordWindow.ShowDialog();
            passwordParameter = passwordWindow.Password.Text;

            //MessageBox.Show("ccc " + providerParameter);

            switch (providerParameter)
            {
                case "INTERBASE":
                    //MessageBox.Show("bbb " + providerParameter);
                    connectionString = "Server=localhost;User="+userParameter+";Password="+passwordParameter+";Database="+databaseParameter;
                    break;
                case "ORACLE":
                    connectionString = "Server=localhost;User=gabinet;Password=flavamed;Database=C:\\KS\\KS-PLW\\BAZY\\med1250.KSB";
                    break;
                default:
                    break;
            }
            //MessageBox.Show(connectionString);

            return connectionString;
        }
    }
}
