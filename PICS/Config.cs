using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;


namespace PICS
{
    public class Config
    {
        static string fullpath = "c:\\_config\\PICS.ini";
        public static ConfigModel model = new ConfigModel();
        static Config()
        {
            Boolean isOk = true;
            string errorMessage = "";
            Json.deserialize<ConfigModel>(Config.fullpath, out model, out isOk, out errorMessage);
            if (isOk == false)
            {
                model = new ConfigModel();
                model.SqlServer = "PLUCZ";
                Monitor.write("Loading Default Configuration");
                Monitor.write($"Error Message : {errorMessage} ");
                Config.saveConfiguration();
            }
            dumpMonitor();
        }
        public static void dumpMonitor()
        {
            Dictionary<String, String> d;
            Json.deserialize<Dictionary<String, String>>(fullpath, out d);
            foreach (var item in d) { Monitor.Console($"[{item.Key}] [{item.Value}]"); }
        }
        internal static void saveConfiguration()
        {
            Json.serialize(model, @"c:\_config\PICS.ini");
        }
    }
}
