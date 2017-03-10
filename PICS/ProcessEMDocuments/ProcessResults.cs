using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;

namespace PICS
{
    public class ProcessResults
    {
        public List<ResultsDocumentModel> ResultDocuments = new List<ResultsDocumentModel>();
        public List<ResultsParcelsModel> Parcels = new List<ResultsParcelsModel>();
        public ProcessResults()
        {
            ResultDocuments.Clear();
            Parcels.Clear();
        }
        public void SaveResults()
        {
            using (var xl = new ExcelWrapper())
            {
                // TODO: config - filenames hardcoded
                xl.wb = xl.wbs.Add();
                string filename = SequencedFilename.get(@"c:\docs\PicsRejectedParcels", $".xlsx");
                var wb = xl.wb;
                var wss = wb.Worksheets;
                if (wss.Count < 2) { wss.Add(); }
                wss[1].Name = "Documents";
                xl.ws = wss[1];
                saveResults(xl);
                wss[2].Name = "Parcels";
                xl.ws = wss[2];
                saveParcels(xl);
                xl.wb.SaveAs(filename);
                xl.wb.Close();
            }
        }
        internal void saveResults(ExcelWrapper xl)
        {
            xl.ws.Activate();
            xl.SetValue("Document", 1, "A");
            xl.SetValue("Date", 1, "B");
            xl.SetValue("Success", 1, "C");
            xl.SetValue("Severity", 1, "D");
            xl.SetValue("Message", 1, "E");
            xl.SetValue("Weight", 1, "F");
            xl.SetValue("Amount", 1, "G");
            xl.SetValue("GoodsString", 1, "H");
            xl.SetValue("AccountName", 1, "I");
            xl.SetValue("Obervation", 1, "J");
            xl.SetValue("Weigt Diff", 1, "K");
            xl.SetValue("Modulation",1,"L");
            int r = 2;
            foreach (var rp in ResultDocuments)
            {
                xl.SetValue(rp.Document, r, "A");
                xl.SetValue(rp.DocumentDate, r, "B");
                xl.SetValue(rp.Success, r, "C");
                xl.SetValue(rp.Severity, r, "D");
                xl.SetValue(rp.Message, r, "E");
                xl.SetValue(rp.Weight, r, "F");
                xl.SetValue(rp.Amount, r, "G");
                xl.SetValue(rp.GoodsString, r, "H");
                xl.SetValue(rp.AccountName, r, "I");
                xl.SetValue(rp.Observation, r, "J");
                xl.SetValue(rp.WeightDifference, r, "K");
                xl.SetValue(rp.ModulationRatio, r, "L");
                r++;
            }
            xl.formatColumn("F", ExcelWrapper.Formats.decimal2);
            xl.formatColumn("G", ExcelWrapper.Formats.decimal2);
            xl.formatColumn("K", ExcelWrapper.Formats.decimal2);
            xl.formatColumn("L", ExcelWrapper.Formats.percent2);
            xl.formatTitle();
            xl.setColumnWidth();
        }
        internal void saveParcels(ExcelWrapper xl)
        {
            xl.ws.Activate();
            xl.SetValue("Parcel", 1, "A");
            xl.SetValue("DocumentType", 1, "B");
            xl.SetValue("Document", 1, "C");
            xl.SetValue("Document Date", 1, "D");
            xl.SetValue("Message", 1, "E");
            int r = 2;
            foreach (var rp in Parcels)
            {
                xl.SetValue(rp.Parcel, r, "A");
                xl.SetValue(rp.DocumentType, r, "B");
                xl.SetValue(rp.Document, r, "C");
                xl.SetValue(rp.DocumentDate.ToString("dd/MMM/yy"), r, "D");
                xl.SetValue(rp.Message, r, "E");
                r++;
            }
            xl.formatTitle();
            xl.setColumnWidth();
        }
    }
}
