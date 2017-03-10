using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICS.EF;

namespace PICS
{
    public class BISaveTransaction
    {
        public static int GetBITransactionHeaders_Id(PICSEntities context)
        {
            int biTransactionType = Helpers.GetTransactionTypes_IdBeginningInventory(context);
            var th = new TransactionHeader();
            th = (context.TransactionHeaders.FirstOrDefault(r => (r.TransactionTypes_Id == biTransactionType)));
            if ((th == null) == false)
            {
                return th.TransactionHeaders_Id;
            }
            th = new TransactionHeader();
            th.TransactionTypes_Id = biTransactionType;
            th.TransactionDate = new DateTime(2017, 1, 15);
            th.DateCreation = DateTime.Now;
            th.DateLastUpdate = DateTime.Now;
            context.TransactionHeaders.Add(th);
            context.SaveChanges();
            return th.TransactionHeaders_Id;
        }
        public static void SaveTransaction(string parcel, decimal weight, decimal amount, EnumEMInventories inventoryType, EnumEMGoods goods,PICSEntities context)
        {
            var p = new Parcel();
            p.GoodsTypes_Id = Helpers.GetGoodsTypes_Id(goods, context);
            p.ParcelInventoryTypes_Id = Helpers.GetParcelInventoryTypes_Id(inventoryType, context);
            p.RSReference = parcel;
            p.WeightOnhand = weight;
            p.CostOnhand = amount;
            p.CostPrice = (weight == 0) ? 0 : amount / weight;
            p.DateCreation = DateTime.Now;
            p.DateLastUpdate = DateTime.Now;
            context.Parcels.Add(p);
            context.SaveChanges();
            var td = new TransactionDetail();
            td.TransactionHeaders_Id = GetBITransactionHeaders_Id(context);
            td.Parcels_Id = p.Parcels_Id;
            td.Weight = weight;
            td.Amount = amount;
            td.DateCreation = DateTime.Now;
            td.DateLastUpdate = DateTime.Now;
            context.TransactionDetails.Add(td);
            context.SaveChanges();
        }
    }
}
