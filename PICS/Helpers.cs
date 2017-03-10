using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICS.EF;
using Core.Common;
namespace PICS
{
    public static class Helpers
    {
        public static decimal ComputeWeightDifference(int transactionHeaders_Id,PICSEntities context)
        {
            decimal? emWeight = 0;
            decimal? detailsWeight = 0;
            emWeight = context.EMTransactions.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id)).Sum(r => r.Weight);
            detailsWeight = context.TransactionDetails.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id)).Sum(r => r.Weight);
            return (decimal)((emWeight == null) ? 0 : emWeight)- (decimal) ((detailsWeight == null) ? 0 : detailsWeight);
        }
        public static decimal ComputeWeightDifference(string EMDocument, PICSEntities context)
        {
            return ComputeWeightDifference(Helpers.GetTransactionHeaders_IdFromEMDocument(EMDocument, context), context);
        }
        public static decimal ComputeModulationRatio(int transactionHeaders_Id, PICSEntities context)
        {
            decimal? emAmount = 0;
            decimal? detailsAmount = 0;
            emAmount = context.EMTransactions.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id)).Sum(r => r.Weight);
            detailsAmount = context.TransactionDetails.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id)).Sum(r => r.Weight);
            return (decimal)((emAmount == null) ? 1 :  ((decimal)((detailsAmount == null) ? 1 : detailsAmount / emAmount)));
        }
        public static decimal ComputeModulationRatio(string EMDocument,PICSEntities context)
        {
            return ComputeModulationRatio(Helpers.GetTransactionHeaders_IdFromEMDocument(EMDocument, context), context);
        }
        internal static decimal? GetCostprice(string parcel, PICSEntities context)
        {
            return context.Parcels.FirstOrDefault(r => r.RSReference == parcel).CostPrice;
        }
        public static int GetTransactionHeaders_IdFromEMDocument(string document, PICSEntities context)
        {
            return (context.EMTransactions.First(r => (r.Document == document)).TransactionHeaders_Id);
        }
        public static int? GetTransactionHeaders_IdFromPolmix(string document, PICSEntities context)
        {
            int pmT = GetRSTransactionTypes_id("POLMIX", context);
            var q = context.RSTransactions.FirstOrDefault(
                        r =>
                            ((r.Document == document)
                            && (r.RSTransactionTypes_Id == pmT))
                    );
            if (q==null) { return null; }
            return q.TransactionHeaders_Id;
        }
        public static int? GetTransactionHeaders_IdFromRoughTransfer(string documentType, string document, PICSEntities context)
        {
            int dT = GetRSTransactionTypes_id(documentType, context);
            RSTransaction q = context.RSTransactions.FirstOrDefault(r => ((r.Document == document) && (r.RSTransactionTypes_Id == dT)));
            if (q == null) { return null; }
            return q.TransactionHeaders_Id;
        }
        public static int GetTransactionTypes_Id(string document, PICSEntities context)
        {
            string description = "";
            if ((new string[] { "CX", "RX" }).Contains(document.Substring(1, 2)) == true) { description = "Inbound Consignment"; }
            else if ((new string[] { "QX" }).Contains(document.Substring(1, 2)) == true) { description = "Purchase"; }
            else if ((new string[] { "S", "R" }).Contains(document.Substring(1, 1)) == true) { description = "Sale"; }
            else if ((new string[] { "X" }).Contains(document.Substring(1, 1)) == true) { description = "Purchase"; }
            else if ((new string[] { "F" }).Contains(document.Substring(1, 1)) == true) { description = "Outbound Consignment"; }
            return context.TransactionTypes.First<TransactionType>(r => (r.Description == description)).TransactionTypes_Id;
        }
        public static int GetTransactionTypes_IdBeginningInventory(PICSEntities context)
        {
            // TODO: smell - consistency - Begin Inventory is the same as Beginning Inventory DB issue/
            return context.TransactionTypes.First<TransactionType>(r => (r.Description == "Begin Inventory")).TransactionTypes_Id;
        }
        public static bool ExistsParcel(string parcel, EnumEMInventories inventory,PICSEntities context)
        {
            int i = Helpers.GetParcelInventoryTypes_Id(inventory, context);
            var p = context.Parcels.FirstOrDefault(r => ((r.RSReference == parcel.Trim()) && (r.ParcelInventoryTypes_Id == i)));
            if ((p==null) == false) { return true; };

            return !(p == null);
        }
        public static bool ExistsParcel(string parcel,  PICSEntities context)
        {
            var p = context.Parcels.FirstOrDefault(r => ((r.RSReference == parcel.Trim()))) ;
            return !(p == null);
        }
        public static bool ExistsEMTransaction(string document,PICSEntities context)
        {
            return (context.EMTransactions.FirstOrDefault(r => (r.Document == document)) != null);
        }
        public static bool ExistsRSPolmix(string document, PICSEntities context)
        {
            return (GetTransactionHeaders_IdFromPolmix(document, context) == null);
        }
        public static bool ExistsRSPolmix(
            string document, 
            PICSEntities context,
            out decimal weightDifference ,
            out decimal modulationRatio)
        {
            weightDifference = 0;
            modulationRatio = 1;
            int? transactionHeaders_Id = GetTransactionHeaders_IdFromPolmix(document, context);
            if (transactionHeaders_Id == null) { return false; }
            List<TransactionDetail> list = context.TransactionDetails.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id)).ToList();
            weightDifference =list.Sum(r => r.Weight);
            decimal inTotal = -list.Where(r => (r.Amount < 0)).Sum(r => r.Amount);
            decimal outTotal = list.Where(r => (r.Amount > 0)).Sum(r => r.Amount);
            modulationRatio = (inTotal == 0) ? 1 : outTotal / inTotal;
            return true;
        }
        public static bool ExistsRSTransfer(RoughTransfersSourceModel transfer,PICSEntities context)
        {
            return (GetTransactionHeaders_IdFromRoughTransfer(transfer.DocumentTypeString,transfer.Document, context) == null);
        }
        public static bool ExistsRSTransfer(RoughTransfersSourceModel transfer, PICSEntities context,out decimal weightDifference,out decimal modulationRatio)
        {
            weightDifference = 0;
            modulationRatio = 1;
            int? transactionHeaders_Id = GetTransactionHeaders_IdFromRoughTransfer(transfer.DocumentTypeString,transfer.Document, context);
            if (transactionHeaders_Id == null) { return false; }
            List<TransactionDetail> list = context.TransactionDetails.Where(r => (r.TransactionDetails_Id == transactionHeaders_Id)).ToList();
            weightDifference = list.Sum(r => r.Weight);
            decimal inTotal = -list.Where(r => (r.Amount < 0)).Sum(r => r.Amount);
            decimal outTotal = list.Where(r => (r.Amount > 0)).Sum(r => r.Amount);
            modulationRatio = (inTotal == 0) ? 1 : outTotal / inTotal;
            return true;
        }
        public static string GetRSDocuments(string emDocument, PICSEntities context)
        {
            int? transactionHeaders_Id = context.EMTransactions.FirstOrDefault(r => r.Document == emDocument).TransactionHeaders_Id;
            if (transactionHeaders_Id == null) { return ""; }
            var rsDocuments = context.RSTransactions.Where(r => (r.TransactionHeaders_Id == transactionHeaders_Id));
            int count = rsDocuments.Count();
            if (count == 0) { return ""; }
            var document = rsDocuments.First();
            string documentType = Helpers.GetRSTransactionTypeDescription(document.RSTransactionTypes_Id,context);
            return $"Linked to  {documentType} {document.Document} [+{count - 1} document(s)]";
        }
        private static string GetRSTransactionTypeDescription(int rsTransactionTypes_Id,PICSEntities context)
        {
            return context.RSTransactionTypes.FirstOrDefault(r => r.RSTransactionTypes_Id == rsTransactionTypes_Id).Description;
        }

        internal static string GetWeightdiffenceEMAccount(EnumEMGoods goods,EnumEMInventories inventory, PICSEntities context)
        {
            string extension = "";
            string root = "";
            string rValue = "";
            // TODO: Hardcoded alert
            switch (goods)
            {
                case EnumEMGoods.Rough:
                    extension = "116";
                    break;
                case EnumEMGoods.Polished:
                    extension = "126";
                    break;
                default:
                    break;
            }
            switch (inventory)
            {
                case EnumEMInventories.Inventory:
                    root = "600";
                    break;
                case EnumEMInventories.InboundShipments:
                    root = "960";
                    break;
                case EnumEMInventories.OutboundShipments:
                    root = "970";
                    break;
                default:
                    break;
            }
            rValue = root + extension;
            return rValue;
        }

        public static string GetGoodsTypesDescription(EnumEMGoods g)
        {
            return (g == EnumEMGoods.Polished) ? "Polished" : "Rough";
        }
        public static int GetGoodsTypes_Id(EnumEMGoods g, PICSEntities context)
        {
            string description = GetGoodsTypesDescription(g); ;
            return context.GoodsTypes.First(r => (r.Description == description)).GoodsTypes_Id;
        }
        public static int GetParcelInventoryTypes_Id(string description, PICSEntities context)
        {
            return context.ParcelInventoryTypes.First(r => (r.Description == description)).ParcelInventoryTypes_Id;
        }
        public static int GetParcelInventoryTypes_Id(EnumEMInventories i, PICSEntities context)
        {
            string description = "";
            if (i == EnumEMInventories.Inventory) { description = "Inventory"; }
            else if (i == EnumEMInventories.InboundShipments) { description = "Inbound Shipments"; }
            else if (i == EnumEMInventories.OutboundShipments) { description = "Outbound Shipments"; }
            return GetParcelInventoryTypes_Id(description, context);    
        }
        public static string GetParcelInventoryDescription(int ParcelInventoryType_Id, PICSEntities context)
        {
            return context.ParcelInventoryTypes.First(m => (m.ParcelInventoryTypes_Id == ParcelInventoryType_Id)).Description;
        }
        public static string GetParcelInventoryDescription(EnumEMInventories i, PICSEntities context)
        {
            return GetParcelInventoryDescription(GetParcelInventoryTypes_Id(i, context), context);
        }
        public static int GetRSTransactionTypes_id(string rsCode, PICSEntities context)
        {
            string description = "";
            if (rsCode == "BOX") { description = "Box"; }
            // if (rsCode == "") { description = "External"; }
            // if (rsCode == "") { description = "External Consignment"; }
            if (rsCode == "LOCATION") { description = "Location"; }
            if (rsCode == "SALE") { description = "Sale"; }
            if (rsCode == "RETOUR") { description = "Retour"; }
            // if (rsCode == "") { description = "Labour"; }
            if (rsCode == "MIX ") { description = "Mix"; }
            if (rsCode == "SORT") { description = "Sort"; }
            // if (rsCode == "") { description = "Sort L"; }
            // if (rsCode == "") { description = "Rough Update"; }
            // if (rsCode == "") { description = "Estimation"; }
            if (rsCode == "PURCHASE") { description = "Purchase"; }
            // if (rsCode == "") { description = "Purchase Report"; }
            if (rsCode == "POLMIX") { description = "Mix"; }
            // if (rsCode == "") { description = "Polished Update"; }
            if (rsCode == "POLSAL") { description = "Sale (Pol)"; }
            if (rsCode == "SALREP") { description = "Sales Report"; }
            if (rsCode == "OURGOODS") { description = "Consignment"; }
            if (rsCode == "OGRETURN") { description = "Return"; }
            if (rsCode == "THEIRGOODS") { description = "Theirgoods"; }
            // if (rsCode == "") { description = "Credit on Sale"; }
            // if (rsCode == "") { description = "Credit on Sales Report"; }
            // if (rsCode == "PURCHASE") { description = "Credit on Purchase"; }
            // if (rsCode == "") { description = "Credit on Purchase Report"; }
            // if (rsCode == "") { description = "Credit on Theirgoods"; }
            if (rsCode == "SENECA") { description = "Seneca"; }
            return context.RSTransactionTypes.First(r => (r.Description == description)).RSTransactionTypes_Id;
        }
        public static void InitializePicsDatabase()
        {
            using (var context = new PICSEntities(Config.model.SqlServer))
            {
                InitializePicsDatabase(context);
            }
        }
        public static void InitializePicsDatabase(PICSEntities context)
        {
            if (Config.model.AllowPicsDatabaseInitialize == false)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;
                Monitor.Console("Request to Initialize PICS database denied by config.");
                Console.ResetColor();
                return;
            }
            context.Database.ExecuteSqlCommand("dbo.sp_initialize");
            Monitor.Console("Pics Database Initialized");
        } 
    }
}
