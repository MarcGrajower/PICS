using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICS.EF;
using PICS.BI;
using Core.Common;
namespace PICS
{
    public class ProcessPolmixes
    {
        public RSPolmixes PM { get; set; }
        public ProcessPolmixes(ProcessResults resultsP)
        {
            PM = new RSPolmixes();
            results = resultsP;
            //using (var context = new PICSEntities(Config.model.SqlServer))
            //{
            //    foreach(var mixDocument in RM._polMixes)
            //    {
            //        var cursorLeft = Console.CursorLeft;
            //        var cursorTop = Console.CursorTop;
            //        Console.SetCursorPosition(15, 0);
            //        Console.Write($"Mix {mixDocument}                  ");
            //        Console.SetCursorPosition(cursorLeft, cursorTop);
            //        processMix(mixDocument.Document,rm,context);
            //    }
            //}
            //Results.SaveResults();
            //Console.WriteLine();
        }
        public void ProcessPolmix(string mixDocument,PICSEntities context)
        {
            PM.Load(mixDocument);
            var resultsDocument = new ResultsDocumentModel();
            resultsDocument.Document = $"PolMix {mixDocument}";
            resultsDocument.DocumentDate = PM.Headers[0].DocumentDate;
            resultsDocument.GoodsString = Helpers.GetGoodsTypesDescription(EnumEMGoods.Polished);
            decimal weightDifference = 0;
            decimal modulationRatio = 1;
            if (Helpers.ExistsRSPolmix(
                mixDocument,
                context,
                out weightDifference,
                out modulationRatio) == true)
            {
                resultsDocument.Success = true;
                resultsDocument.Message = "Previously Processed.";
                resultsDocument.Severity = "Warning";
                resultsDocument.WeightDifference = weightDifference;
                resultsDocument.ModulationRatio = modulationRatio;
                results.ResultDocuments.Add(resultsDocument);
                return;
            }
            var check = new CheckWeights(PM);
            resultsDocument.WeightDifference = check.WeightDiff;
            {
                if (check.or.Success == false)
                {
                    resultsDocument.Success = false;
                    resultsDocument.Message = "Weight Error";
                    resultsDocument.Severity = "Warning";
                    resultsDocument.Observation = check.or.MessageList[0];
                    results.ResultDocuments.Add(resultsDocument);
                    return;
                }
            }

            var bi = new GetBeginningInventory(context, EnumEMGoods.Polished,(RSConnection) PM,EnumEMInventories.Inventory,ref results.Parcels);
            if (bi.or.Success == false)
            {
                resultsDocument.Success = false;
                resultsDocument.Message = "Begginning Inventory";
                resultsDocument.Severity = "Warning";
                resultsDocument.Observation = bi.Observation;
                results.ResultDocuments.Add(resultsDocument);
                Monitor.write(PM.Headers[0].Description());
                Monitor.write(bi.or.Dump());
                return;
            }
            // TODO : assume [enumEMInventories.Inventory] - not enforced.
            var sm = new SaveTransfers(PM, context,EnumEMGoods.Polished,ref results.Parcels,check);
            resultsDocument.ModulationRatio = sm.ModulationRatio;
            if (sm.or.Success == false)
            {
                resultsDocument.Success = false;
                resultsDocument.Message = "Save Transfers";
                resultsDocument.Severity = "Warning";
                results.ResultDocuments.Add(resultsDocument);
                return;
            }
            resultsDocument.Success = true;
            resultsDocument.Message = "OK";
            resultsDocument.Severity = "OK";
            results.ResultDocuments.Add(resultsDocument);
        }
        ProcessResults results;
    }
}
