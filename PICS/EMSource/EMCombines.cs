using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using Microsoft.Office.Interop.Excel;


namespace PICS
{
    public static class EMCombines
    {
        public static bool IsFirst { get; set; }
        public static bool InList { get; set; }
        public static List<EMCombineModel> Combined = new List<EMCombineModel>();
        public static void GetCombined(string EMDocument)
        {
            string d;
            if (list.Exists(m => (m.EMCombineDocument == EMDocument)) == true)
            {
                InList = true;
                IsFirst = true;
                d = EMDocument;
            }
            else
            {
                InList = list.Exists(m => (m.EMDocument.Document == EMDocument));
                if (InList == false) { return; }
                d = list.Find(m => m.EMDocument.Document == EMDocument).EMCombineDocument;
                IsFirst = (list.First(m => m.EMCombineDocument == d).EMDocument.Document == EMDocument);
                if (IsFirst == false) { return; }
            }
            Combined.Clear();
            Combined = list.FindAll(m => (m.EMCombineDocument == d));
        }
        public static string Description()
        {
            if (Combined.Count<1) { return ""; };
            return $"{Combined[0].EMCombineDocument} {Combined[0].EMDocument} ({Combined.Count - 1} documents"; 
        }
        static EMCombines()
        {
            using (var xl = new ExcelWrapper())
            {
                list.Clear();
                xl.wbs.Open(fullpath);
                xl.ws = xl.app.ActiveSheet;
                long lastRow = xl.getUsedRows();
                for (long i = 2; i <= lastRow; i++)
                {
                    var m = new EMCombineModel();
                    m.EMCombineDocument = xl.getString(i, 1);
                    m.EMDocument = new EMSourceModel();
                    m.EMDocument.Document = xl.getString(i, 2);
                    // TODO: Robustness Alert - This would not detect if documents are not valid in the EM Spreadsheet
                    EMSourceModel t = EMSourceTransactions.Transactions.Find(r => (r.Document == m.EMDocument.Document));
                    m.EMDocument = t;
                    list.Add(m);
                }
            }
            Monitor.Console($"EMCombines has {list.Count()} entries");
        }
        //TODO: Config - EMCombines Workbook FullPath
        static string fullpath = @"v:\data\pics\combine 2017.xlsx";
        static List<EMCombineModel> list = new List<EMCombineModel>();
        public static string ObservationNotFirst(string document)
        {
            return "";
        }
    }
}
