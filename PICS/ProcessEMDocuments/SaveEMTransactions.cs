using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using PICS.EF;

namespace PICS
{
    public class SaveEMTransaction
    {
        //Todo : Handling of cost price.   At this stage the Parcel.CostPrice is computed when the Parcel is created only, irrespective if InventoryType and TransactionType
        //       This will not work quantity based sales, but at this stage there has not been an instance of those.
        public OperationResult or { get; set; } = new OperationResult();
        public decimal ModulationRatio { get; set; }
        public SaveEMTransaction(EMSourceModel tParameter, RSLink rsParameter, PICSEntities contextParameter, ref List<ResultsParcelsModel> rpsP,CheckWeights checkP)
        {
            t = tParameter;
            rs = rsParameter;
            rps = rpsP;
            check = checkP;
            context = contextParameter;
            or.Success = true;
            modulate();
            using (var t = context.Database.BeginTransaction())
            {
                processTransactionHeader();
                processTransactionDetails();
                if (or.Success == false)
                {
                    t.Rollback();
                    return;
                }
                processRsHeaders();
                processEmHeaders();
                t.Commit();
            }

        }
        void processTransactionHeader()
        {
            th.TransactionTypes_Id = Helpers.GetTransactionTypes_Id(t.Document, context);
            th.TransactionDate = t.DocumentDate;
            th.DateCreation = DateTime.Now;
            th.DateLastUpdate = DateTime.Now;
            sign = (t.Amount > 0 ? 1 : -1);
            context.TransactionHeaders.Add(th);
            context.SaveChanges();
        }
        void processTransactionDetails()
        {
            string firstParcel = "";
            int errorCount = 0;
            foreach (var detail in rs.Details)
            {
                decimal costPrice = 0;
                bool success = true;
                processParcel(detail, out costPrice, out success);
                if (success == true)
                {
                    var td = new TransactionDetail();
                    td.Parcels_Id = (int)parcels_Id;
                    td.TransactionHeaders_Id = th.TransactionHeaders_Id;
                    td.Weight = detail.Weight * sign;
                    td.Amount = detail.Amount * sign;
                    td.Cogs = detail.Weight * costPrice * sign;
                    td.DateCreation = DateTime.Now;
                    td.DateLastUpdate = DateTime.Now;
                    context.TransactionDetails.Add(td);
                    context.SaveChanges();
                }
                else
                {
                    if (or.Success == true)
                    {
                        firstParcel = detail.Parcel;
                    }
                    or.Success = false;
                    var rp = new ResultsParcelsModel();
                    rp.Parcel = detail.Parcel;
                    rp.DocumentType = $"SaveTransaction {rs.Headers[0].RSTransactionType} {rs.Headers[0].Document}";
                    rp.Document = detail.RSDocument;
                    rp.DocumentDate = rs.Headers[0].DocumentDate;
                    rp.Message = $"Parcel not found {rs.Headers[0].Document}";
                    errorCount++;
                }
            }
            if (or.Success == false)
            {
                or.AddMessage($"{errorCount} Parcels not found.  First : {firstParcel} ");
            }
        }
        void processParcel(RSDetail detail, out decimal costPrice, out bool success)
        {
            success = true;
            costPrice = 0;
            parcels_Id = null;
            int pi = Helpers.GetParcelInventoryTypes_Id(t.Inventory, context);
            // TODO: Smell - See ParcelExists in Helpers
            var p = context.Parcels.FirstOrDefault(r => ((r.RSReference == detail.Parcel)));
            if (sign == -1)
            {
                if (p == null)
                {
                    if ((pi == Helpers.GetParcelInventoryTypes_Id(EnumEMInventories.OutboundShipments, context)) == false)
                    {
                        success = false;
                        return;
                    }
                    else if ((pi == Helpers.GetParcelInventoryTypes_Id(EnumEMInventories.OutboundShipments, context)) == false)
                    {
                        p = context.Parcels.FirstOrDefault(r => ((r.RSReference == detail.Parcel) && (r.ParcelInventoryTypes_Id == Helpers.GetParcelInventoryTypes_Id(EnumEMInventories.Inventory, context))));
                        if (p == null)
                        {
                            success = false;
                            return;
                        }
                    }
                }
            }
            if (p == null)
            {
                p = new Parcel();
                p.GoodsTypes_Id = Helpers.GetGoodsTypes_Id(t.Goods, context);
                p.ParcelInventoryTypes_Id = Helpers.GetParcelInventoryTypes_Id(t.Inventory, context);
                p.RSReference = detail.Parcel;
                p.WeightOnhand = detail.Weight * sign;
                // TODO: 1 Domain Alert - CostOnhand depends on the transaction
                // post only if positive, otherwise trigger begin inventory or error.
                // parcel helper : get price - maybe add Sales Price.
                p.CostOnhand = detail.Amount * sign;
                p.CostPrice = detail.Amount / detail.Weight;
                p.DateCreation = DateTime.Now;
                p.DateLastUpdate = DateTime.Now;
                context.Parcels.Add(p);
            }
            else
            {
                // TODO: 1. if negative, compute price and subtract price.
                p.WeightOnhand += detail.Weight * sign;
                p.CostOnhand += detail.Weight * p.CostPrice * sign;
                p.DateLastUpdate = DateTime.Now;
            }
            parcels_Id = p.Parcels_Id;
            costPrice = p.CostPrice;
        }
        void processRsHeaders()
        {
            foreach (var rsHeader in rs.Headers)
            {
                var rsTransaction = new RSTransaction();
                rsTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                rsTransaction.RSTransactionTypes_Id = Helpers.GetRSTransactionTypes_id(rsHeader.RSTransactionType, context);
                rsTransaction.Document = rsHeader.Document;
                // either get RS Date into rsHeader or get rid of the field altogether
                rsTransaction.RSTransactionDate = t.DocumentDate;
                rsTransaction.DateCreation = DateTime.Now;
                rsTransaction.DateLastUpdate = DateTime.Now;
                context.RSTransactions.Add(rsTransaction);
                context.SaveChanges();
            }
        }
        void processEmHeaders()
        {
            foreach (var emDocument in emDocuments)
            {
                var emTransaction = new EMTransaction();
                emTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                emTransaction.Document = emDocument.Document;
                emTransaction.GlAccount = emDocument.Account;
                // not sure what this is
                emTransaction.EMStatus = "Status";
                emTransaction.StatusMessage = "Status Message";
                //
                emTransaction.DateCreation = DateTime.Now;
                emTransaction.DateLastUpdate = DateTime.Now;
                emTransaction.Weight = emDocument.Weight;
                emTransaction.Amount = emDocument.Amount;

                context.EMTransactions.Add(emTransaction);
                context.SaveChanges();
            }
            if (check.WeightDiff!=0)
            {
                var emTransaction = new EMTransaction();
                emTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                emTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                emTransaction.Document = "";
                emTransaction.GlAccount = Helpers.GetWeightdiffenceEMAccount(t.Goods,t.Inventory,context);;
                // not sure what this is
                emTransaction.EMStatus = "Status";
                emTransaction.StatusMessage = "Status Message";
                //
                emTransaction.DateCreation = DateTime.Now;
                emTransaction.DateLastUpdate = DateTime.Now;
                emTransaction.Weight = check.WeightDiff;
                emTransaction.Amount = 0;
                context.EMTransactions.Add(emTransaction);
                context.SaveChanges();
                Monitor.write($"SaveEMTransaction added WeightDifference for {emDocuments[0].Document} {check.WeightDiff:n2} crts");
            }
        }
        internal void modulate()
        {
            EMCombines.GetCombined(t.Document);
            emDocuments = new List<EMSourceModel>();
            if ((EMCombines.InList == true) && (EMCombines.IsFirst == true))
            {
                t.UnSwap();
                foreach (var ct in EMCombines.Combined)
                {
                    emDocuments.Add(ct.EMDocument);
                }
            }
            else emDocuments.Add(t);
            decimal em = Math.Abs(emDocuments.Sum(r => r.Amount));
            decimal details = rs.Details.Sum(r => r.Amount);
            ModulationRatio = (em == 0) ? 1 : em / details;
            foreach (var d in rs.Details)
            {
                d.Amount *= ModulationRatio;
            }
        }
        EMSourceModel t;
        RSLink rs;
        PICSEntities context;
        TransactionHeader th = new TransactionHeader();
        int sign;
        int? parcels_Id;
        List<EMSourceModel> emDocuments;
        List<ResultsParcelsModel> rps;
        CheckWeights check;
    }
}
