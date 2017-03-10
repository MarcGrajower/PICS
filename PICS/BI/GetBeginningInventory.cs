using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using PICS.EF;

namespace PICS.BI
{
    public class GetBeginningInventory
    {
        public OperationResult or = new OperationResult();
        public string Observation;
        internal bool find(string parcel, out decimal weight, out decimal amount)
        {
            //Todo: Hardcoded Alert
            weight = 0;
            amount = 0;
            var row = xl.FindRow(parcel, "A");
            if (row == 0) { return false; };
            if (goods == EnumEMGoods.Rough)
            {
                weight = xl.getDecimal(row, "D");
                amount = xl.getDecimal(row, "E");
                return true;
            }
            decimal adjustedValue = 0;
            weight = xl.getDecimal(row, "C");
            decimal share = xl.getDecimal(row, "N");
            amount = xl.getDecimal(row, "Q");
            string pricing = xl.getString(row, "P");
            var pp = new PolishedPricing(parcel, pricing, amount, share, out adjustedValue);
            amount = adjustedValue;
            return true;
        }
        public void open()
        {
            if (goods == EnumEMGoods.Rough)
            {
                xl.ws = xl.wbs.Open(fullpathRough).ActiveSheet;
                return;
            };
            xl.ws = xl.wbs.Open(fullpathPolished).ActiveSheet;
        }
        public GetBeginningInventory(
            PICSEntities contextP,
            EnumEMGoods goodsP,
            RSConnection rsP,
            EnumEMInventories inventoryP,
            int signP,
            ref List<ResultsParcelsModel> rps)
        {
            context = contextP;
            goods = goodsP;
            details = rsP.Details;
            inventory = inventoryP;
            sign = signP;
            rs = rsP;
            or.Clear();
            or.Success = true;
            CheckEarlySales();
            if (or.Success == true) { return; }
            or.Success = true;
            if (sign == +1) { if (inventory != EnumEMInventories.OutboundShipments) { return; } };
            int count = details.Count();
            int inError = 0;
            string firstError = "";
            using (xl = new ExcelWrapper())
            {
                open();
                foreach (var detail in details)
                {
                    decimal weight;
                    decimal amount;
                    if (inventory != EnumEMInventories.OutboundShipments)
                    {
                        if (Helpers.ExistsParcel(detail.Parcel, context) == true) { continue; }
                        if (find(detail.Parcel, out weight, out amount))
                        {
                            BISaveTransaction.SaveTransaction(detail.Parcel, weight, amount, inventory, goods, context);
                        }
                        else
                        {
                            inError++;
                            if (firstError == "") { firstError = detail.Parcel; };
                            or.Success = false;
                            or.AddMessage($"\t{detail.Parcel} in {Helpers.GetParcelInventoryDescription(inventory, context)} - attempt to coopt failed.");
                            var rp = new ResultsParcelsModel();
                            rp.Parcel = detail.Parcel;
                            rp.DocumentType = $"Begin Inventory - ({rs.Headers[0].RSTransactionType})";
                            rp.Document = rs.Headers[0].Document;
                            rp.DocumentDate = rs.Headers[0].DocumentDate;
                            rp.Message = "Attempt to coopt failed.";
                            rps.Add(rp);
                        }
                        continue;
                    }
                    if (sign == -1)
                    {
                        if (Helpers.ExistsParcel(detail.Parcel, EnumEMInventories.InboundShipments, context) == true) {continue; }
                        if (find(detail.Parcel, out weight, out amount))
                        {
                            BISaveTransaction.SaveTransaction(detail.Parcel, weight, amount, inventory, goods, context);
                        }
                        else
                        {
                            inError++;
                            if (firstError == "") { firstError = detail.Parcel; };
                            or.Success = false;
                            or.AddMessage($"\t{detail.Parcel} in {Helpers.GetParcelInventoryDescription(inventory, context)} - attempt to coopt failed.");
                            var rp = new ResultsParcelsModel();
                            rp.Parcel = detail.Parcel;
                            rp.DocumentType = $"Begin Inventory - ({rs.Headers[0].RSTransactionType})";
                            rp.Document = rs.Headers[0].Document;
                            rp.DocumentDate = rs.Headers[0].DocumentDate;
                            rp.Message = "Attempt to coopt failed.";
                            rps.Add(rp);
                        }
                        continue;
                    }
                    //Todo: 1. Outbound Positive (return of consigned goods - See decision table
                }
                if (or.Success == false) { Observation = $"Could not coopt : {inError} out of {count} First : {firstError}"; }
            }
        }
        // Todo: smelly - this constructor just for POlMixes and RoughTransfers
        public GetBeginningInventory(
            PICSEntities contextP,
            EnumEMGoods goodsP,
            RSConnection rsP,
            EnumEMInventories inventoryP,
            ref List<ResultsParcelsModel> rps)
        {
            context = contextP;
            goods = goodsP;
            rs = rsP;
            details = rs.Details;
            inventory = inventoryP;
            or.Clear();
            or.Success = true;
            int count = details.Count();
            int inError = 0;
            string firstError = "";
            using (xl = new ExcelWrapper())
            {
                open();
  
                foreach (var detail in details)
                {
                    decimal weight;
                    decimal amount;
                    if (Helpers.ExistsParcel(detail.Parcel, inventory, context) == true) { continue; }
                    if (find(detail.Parcel, out weight, out amount))
                    {
                        BISaveTransaction.SaveTransaction(detail.Parcel, weight, amount, inventory, goods, context);
                    }
                    else
                    {
                        inError++;
                        if (firstError == "") { firstError = detail.Parcel; };
                        or.Success = false;
                        or.AddMessage($"\t{detail.Parcel} in {Helpers.GetParcelInventoryDescription(inventory, context)} - attempt to coopt failed.");
                        var rp = new ResultsParcelsModel();
                        rp.Parcel = detail.Parcel;
                        rp.DocumentType = $"Begin Inventory - ({rs.Headers[0].RSTransactionType})";
                        rp.Document = rs.Headers[0].Document;
                        rp.DocumentDate = rs.Headers[0].DocumentDate;
                        rp.Message = "Attempt to coopt failed.";
                        rps.Add(rp);
                    }
                }
                if (or.Success == false) { Observation = $"Could not coopt : {inError} out of {count} First : {firstError}"; }
            }
        }
        internal void CheckEarlySales()
        {
            var es = new BIEarlySales(rs, context);
            bool s = true;
            foreach (var detail in rs.Details)
            {
                es.Process(detail.Parcel, goods, inventory);
                if (s == true) { s = es.or.Success; };
            }
            or.Success = s;
        }
        //TODO: Config - Location of beginning inventoris
        string fullpathRough { get; set; } = @"c:\_Projects in process\170127 PICS - Project Management\inventories 16 jan 17\RoughAdjust_170116_001.xlsx";
        string fullpathPolished { get; set; } = @"c:\_Projects in process\170127 PICS - Project Management\inventories 16 jan 17\Polished Inventory_170116_001.xlsx";
        PICSEntities context;
        ExcelWrapper xl;
        EnumEMGoods goods;
        List<RSDetail> details;
        RSConnection rs;
        EnumEMInventories inventory;
        int sign;
    }
}
