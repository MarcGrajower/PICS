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
    public class ProcessTransfers
    {
        public RSRoughTransfers RT;
        public ProcessTransfers(ProcessResults resultsP)
        {
            RT = new RSRoughTransfers();
            results = resultsP;
        }
        public void ProcessTransfer(RoughTransfersSourceModel transfer,  PICSEntities context)
        {
            RT.Load(transfer);
            var resultsDocument = new ResultsDocumentModel();
            resultsDocument.Document = $"RoughTransfer {transfer.Description()}";
            resultsDocument.DocumentDate = RT.Headers[0].DocumentDate;
            resultsDocument.GoodsString = Helpers.GetGoodsTypesDescription(EnumEMGoods.Rough);
            decimal weightDifference = 0;
            decimal modulationRatio = 1;
            if (Helpers.ExistsRSTransfer(transfer, context,out weightDifference,out modulationRatio) == true)
            {
                resultsDocument.Success = true;
                resultsDocument.Message = "Previously Processed.";
                resultsDocument.Severity = "Warning";
                resultsDocument.WeightDifference = weightDifference;
                resultsDocument.ModulationRatio = modulationRatio;
                results.ResultDocuments.Add(resultsDocument);
                return;
            }
            var check = new CheckWeights(RT);
            resultsDocument.WeightDifference = check.WeightDiff;
            {
                if (check.or.Success == false)
                {
                    resultsDocument.Success = false;
                    resultsDocument.Message = "Weight Error";
                    resultsDocument.Severity = "Error";
                    resultsDocument.Observation = check.or.MessageList[0];
                    results.ResultDocuments.Add(resultsDocument);
                    return;
                }
            }
            var bi = new GetBeginningInventory(context, EnumEMGoods.Rough, (RSConnection)RT, EnumEMInventories.Inventory, ref results.Parcels);
            if (bi.or.Success == false)
            {
                resultsDocument.Success = false;
                resultsDocument.Message = "Begginning Inventory";
                resultsDocument.Severity = "Warning";
                resultsDocument.Observation = bi.Observation;
                results.ResultDocuments.Add(resultsDocument);
                Monitor.write(RT.Headers[0].Description());
                Monitor.write(bi.or.Dump());
                return;
            }
            var sm = new SaveTransfers(RT, context, EnumEMGoods.Rough,ref results.Parcels,check);
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
        ProcessResults results = new ProcessResults();
    }
}
