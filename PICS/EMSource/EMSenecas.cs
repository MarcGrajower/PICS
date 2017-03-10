using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;

namespace PICS
{
    public static class EMSenecas
    {
        public static List<EMSenecaModel> GetLines(string Document)
        {
            var lines = list.FindAll(m => (m.EMDocument == Document));
            return lines;
        }
        static EMSenecas()
        {
            using (var xl = new ExcelWrapper())
            {
                list.Clear();
                xl.wbs.Open(fullpath);
                xl.ws = xl.app.ActiveSheet;
                long lastRow = xl.getUsedRows();
                for (long i = 2; i <= lastRow; i++)
                {
                    // TODO: Robustness alert - Excel columns hard coded
                    var m = new EMSenecaModel();
                    m.EMDocument = xl.getString(i, "F");
                    m.DocumentDate = xl.getDate(i, "B");
                    m.Parcel = xl.getString(i, "C");
                    m.Weight = xl.getDecimal(i, "D");
                    m.Amount = xl.getDecimal(i, "E");
                    list.Add(m);
                    var m2 = new EMSenecaModel();
                    m2.EMDocument = xl.getString(i, "I");
                    m2.DocumentDate = xl.getDate(i, "B");
                    m2.Parcel = xl.getString(i, "C");
                    m2.Weight = xl.getDecimal(i, "G");
                    m2.Amount = xl.getDecimal(i, "H");
                    list.Add(m2);
                }
                Monitor.Console($"EMSenecas has {list.Count()} entries");
            }
        }
        //TODO: Config - EMSenecas Workbook FullPath
        static string fullpath = @"v:\data\pics\PICS SENECA PURCHASES 2017.xlsx";
        static List<EMSenecaModel> list = new List<EMSenecaModel>();
    }
}
