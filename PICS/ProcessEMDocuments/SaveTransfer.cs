using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using PICS.EF;
namespace PICS
{
    public class SaveTransfers
    // Todo : breaks if total details is 0
    //RS PolMix Cheat Sheet
    //------------------
    //PolMix consists of 3 ttables:
    //POLMIX gets copied in Headers[0]
    //POLMIXi gets copied in Details - these are the parcels that get destroyed by the POLMIX.  They are checked for existence.
    //POLMIXo gets copied in DetailsTo.  These parcesl are created as consequence of the the POLMIX.
    {
        public decimal ModulationRatio { get; set; }
        public OperationResult or { get; set; } = new OperationResult();
        public CheckWeights check;
        public SaveTransfers(RSConnection rsParameter, PICSEntities contextParameter, EnumEMGoods goodsP,ref List<ResultsParcelsModel> rpsP,CheckWeights checkP )
        {
            rs = rsParameter;
            context = contextParameter;
            or.Success = true;
            modulate();
            processTransactionHeader();
            processTransactionDetails();
            goods = goodsP;
            check = checkP;
            if (or.Success == false)
            {
                return;
            }
            processTransactionDetailsTo();
            processRsHeaders();
            processEmHeaders();
        }
        void modulate()
        {
            foreach(var d in rs.Details)
            {
                // TODO : Breaks if parcel does not exists.
                d.Amount =  d.Weight * (decimal)Helpers.GetCostprice(d.Parcel, context);
            }
            decimal inTotal = rs.Details.Sum(r => r.Amount);
            decimal outTotal = rs.DetailsTo.Sum(r => r.Amount);
            ModulationRatio = inTotal / outTotal;
            foreach (var d in rs.DetailsTo)
            {
                d.Amount *= ModulationRatio;
            }
        }
        void processTransactionHeader()
        {
            th.TransactionTypes_Id = context.TransactionTypes.First<TransactionType>(r => (r.Description == "Internal Transfer")).TransactionTypes_Id;
            th.TransactionDate = rs.Headers[0].DocumentDate;
            th.DateCreation = DateTime.Now;
            th.DateLastUpdate = DateTime.Now;
            context.TransactionHeaders.Add(th);
            context.SaveChanges();
        }
        void processTransactionDetails()
        {
            foreach (var detail in rs.Details)
            {
                decimal price = 0;
                processParcelIn(detail, out price);
                if (or.Success == true)
                {
                    var td = new TransactionDetail();
                    td.Parcels_Id = (int)parcels_Id;
                    td.TransactionHeaders_Id = th.TransactionHeaders_Id;
                    td.Weight = -detail.Weight;
                    td.Amount = -detail.Weight * price;
                    td.DateCreation = DateTime.Now;
                    td.DateLastUpdate = DateTime.Now;
                    context.TransactionDetails.Add(td);
                    context.SaveChanges();
                }
            }
        }
        void processParcelIn(RSDetail detail,out decimal price)
        {
            price = 0;
            parcels_Id = null;
            int pi = Helpers.GetParcelInventoryTypes_Id("Inventory", context);
            var p = context.Parcels.FirstOrDefault(r => ((r.RSReference == detail.Parcel) && (r.ParcelInventoryTypes_Id == pi)));
            if (p == null)
            {
                or.Success = false;
                or.AddMessage($"{detail.Parcel} is not available.");
                return;
            }
            p.GoodsTypes_Id = Helpers.GetGoodsTypes_Id(goods, context);
            p.WeightOnhand -= detail.Weight;
            p.CostOnhand -= detail.Weight * p.CostPrice;
            p.DateLastUpdate = DateTime.Now;
            parcels_Id = p.Parcels_Id;
            price = p.CostPrice;
        }
        void processTransactionDetailsTo()
        {
            foreach (var detailTo in rs.DetailsTo)
            {
                processParcelTo(detailTo);
                if (or.Success == true)
                {
                    var td = new TransactionDetail();
                    td.Parcels_Id = (int)parcels_Id;
                    td.TransactionHeaders_Id = th.TransactionHeaders_Id;
                    td.Weight = detailTo.Weight;
                    td.Amount = detailTo.Amount;
                    td.DateCreation = DateTime.Now;
                    td.DateLastUpdate = DateTime.Now;
                    context.TransactionDetails.Add(td);
                    context.SaveChanges();
                }
            }
        }
        void processParcelTo(RSDetail detailTo)
        {
            var p = new Parcel();
            p.GoodsTypes_Id = Helpers.GetGoodsTypes_Id(goods,context);
            p.ParcelInventoryTypes_Id = Helpers.GetParcelInventoryTypes_Id("Inventory",context);
            p.WeightOnhand += detailTo.Weight;
            p.RSReference = detailTo.Parcel;
            p.CostOnhand += detailTo.Amount;
            p.CostPrice = p.CostOnhand / p.WeightOnhand;
            p.DateLastUpdate = DateTime.Now;
            context.Parcels.Add(p);
            context.SaveChanges();
            parcels_Id = p.Parcels_Id;
        }
        void processRsHeaders()
        {
            foreach (var rsHeader in rs.Headers)
            {
                var rsTransaction = new RSTransaction();
                rsTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                // TODO : This should also be applicable to Rough Transfers
                rsTransaction.RSTransactionTypes_Id = Helpers.GetRSTransactionTypes_id("POLMIX", context);
                rsTransaction.Document = rsHeader.Document;
                // either get RS Date into rsHeader or get rid of the field altogether
                rsTransaction.RSTransactionDate = rs.Headers[0].DocumentDate;
                rsTransaction.DateCreation = DateTime.Now;
                rsTransaction.DateLastUpdate = DateTime.Now;
                context.RSTransactions.Add(rsTransaction);
                context.SaveChanges();
            }
        }
        void processEmHeaders()
        {
            if (check.WeightDiff != 0)
            {
                var emTransaction = new EMTransaction();
                emTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                emTransaction.TransactionHeaders_Id = th.TransactionHeaders_Id;
                emTransaction.Document = "";
                // TODO : assumes transfers only for [enumEMInventory.Inventory] goods
                emTransaction.GlAccount = Helpers.GetWeightdiffenceEMAccount(goods, EnumEMInventories.Inventory, context); ;
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
                Monitor.write($"SaveTransfers added WeightDifference for Transfer {rs.Headers[0].Document} {check.WeightDiff:n2} crts");
            }
        }
        int? parcels_Id;
        TransactionHeader th = new TransactionHeader();
        RSConnection rs;
        PICSEntities context;
        // TODO : 2 Hmm... goods not assigned in SaveTransfers?
        EnumEMGoods goods;
        List<ResultsParcelsModel> rsp;
    }
}
