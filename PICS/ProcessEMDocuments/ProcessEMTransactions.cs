using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using PICS.EF;
using PICS.BI;
namespace PICS
{
    public class ProcessEMTransactions
    {
        public RSLink RS;
        public ProcessEMTransactions(ProcessResults resultsP)
        {
            results = resultsP;
            RS = new RSLink();
            EMSourceTransactions.GetTransactions();
            //int transactionsCount = EMSourceTransactions.Transactions.Count;
            //using (var context = new PICSEntities(Config.model.SqlServer))
            //{
            //    // Helpers.InitializePicsDatabase();
            //    foreach (var t in EMSourceTransactions.Transactions)
            //    {
            //        var cursorLeft = Console.CursorLeft;
            //        var cursorTop = Console.CursorTop;
            //        Console.SetCursorPosition(15, 0);
            //        Console.Write($"{t.Document} {t.DocumentDate.ToString("dd/MMM/yy")}  \t{results.ResultDocuments.Count}/{transactionsCount}");
            //        Console.SetCursorPosition(cursorLeft, cursorTop);
            //        process(t, RS,context, ref results.Parcels);
            //    }
            //    Console.WriteLine();
            //}
        }
        // Todo: SMELL - this constructor for debugging purposes only
        internal ProcessEMTransactions(string document,ProcessResults resultsP)
        {
            results = resultsP;
            var rs = new RSLink();
            EMSourceTransactions.GetTransactions();
            var t = EMSourceTransactions.Transactions.FirstOrDefault(r => (r.Document == document));
            using (var context = new PICSEntities(Config.model.SqlServer))
            {
                // Helpers.InitializePicsDatabase();
                var cursorLeft = Console.CursorLeft;
                var cursorTop = Console.CursorTop;
                Console.SetCursorPosition(15, 0);
                Console.Write($"{t.Document} \t{t.DocumentDate.ToString("dd/MMM/yy")}");
                Console.SetCursorPosition(cursorLeft, cursorTop);
                process(t, rs, context, ref results.Parcels);
                Console.WriteLine();
                Monitor.write(t.Description());
            }
        }
        internal void process(EMSourceModel t,RSLink rs,PICSEntities context, ref List<ResultsParcelsModel> rps)
        {
            var resultDocument = new ResultsDocumentModel();
            resultDocument.Document = t.Document;
            resultDocument.DocumentDate = t.DocumentDate;
            resultDocument.Weight = t.Weight;
            resultDocument.GoodsString = t.GoodsString();
            resultDocument.AccountName = t.AccountName;
            resultDocument.Amount =t.Amount;
            if (Helpers.ExistsEMTransaction(t.Document,context) == true)
            {
                resultDocument.Success = true;
                resultDocument.Message = "Previously Processed.";
                resultDocument.Severity = "Warning";
                resultDocument.Observation = Helpers.GetRSDocuments(t.Document, context);
                resultDocument.WeightDifference = Helpers.ComputeWeightDifference(t.Document, context);
                resultDocument.ModulationRatio = Helpers.ComputeModulationRatio(t.Document, context);
                results.ResultDocuments.Add(resultDocument);
                return;
            }
            EMCombines.GetCombined(t.Document);
            if ((EMCombines.InList==true) && (EMCombines.IsFirst == false))
            {
                resultDocument.Success = true;
                resultDocument.Message = "Not first Document in Combined";
                resultDocument.Severity = "Warning";
                resultDocument.Observation = EMCombines.Description();
                results.ResultDocuments.Add(resultDocument);
                return;
            }
            rs.LinkDocument(t);
            if (rs.OpResult.Success == false)
            {
                resultDocument.Success = false;
                resultDocument.Message = "Linking Error";
                resultDocument.Severity = "Error";
                resultDocument.Observation = "No Link Found";
                results.ResultDocuments.Add(resultDocument);
                Monitor.write(t.Description());
                Monitor.write(rs.OpResult.Dump());
                return;
            }
            var check = new CheckWeights(t, rs);
            resultDocument.WeightDifference = check.WeightDiff;
            if (check.or.Success == false)
            { 
                resultDocument.Success = false;
                resultDocument.Message = "Weight Error";
                resultDocument.Severity = "Error";
                resultDocument.Observation = check.or.MessageList[0];
                results.ResultDocuments.Add(resultDocument);
                return;
            }
            var bi = new GetBeginningInventory(context, t.Goods, rs, t.Inventory, t.sign(),ref results.Parcels);
            if (bi.or.Success == false)
            {
                resultDocument.Success = false;
                resultDocument.Message = "Beginning Inventory";
                resultDocument.Severity = "Error";
                resultDocument.Observation = bi.Observation;
                results.ResultDocuments.Add(resultDocument);
                Monitor.write(rs.Headers[0].Description());
                Monitor.write(bi.or.Dump());
                return;
            }

            var st = new SaveEMTransaction(t, rs, context,ref results.Parcels,check);
            if (st.or.Success == false)
            {
                resultDocument.Success = false;
                resultDocument.Message = "Save Transaction";
                resultDocument.Severity = "Error";
                resultDocument.Observation = rs.OpResult.Dump();
                results.ResultDocuments.Add(resultDocument);
                return;
            }
            resultDocument.Success = true;
            resultDocument.Observation = $"Linked to {rs.Headers[0].Document}, ({rs.Headers.Count} links) ";
            resultDocument.Severity = "OK";
            resultDocument.Observation = Helpers.GetRSDocuments(t.Document, context);
            resultDocument.ModulationRatio = st.ModulationRatio;
            results.ResultDocuments.Add(resultDocument);
        }
        ProcessResults results = new ProcessResults();
    }
}
