using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using EM.EF;

namespace PICS
{

    public static class EMSourceTransactions
    {
        //TODO: Move to Config the definition of the current EM fiscal year
        static string yearcode = "17";
        static int fromJournal = 125;
        static int toJournal = 137;

        public static List<EMSourceModel> Transactions = new List<EMSourceModel>();
        public static void GetTransactions()
        {
            using (var context = new EMEntities(Config.model.SqlServer))
            {
                context.AttachExp_0071T();
                var aankopen = context.VW_Aankopen_ByJournal(fromJournal, toJournal);
                foreach (var i in aankopen)
                {
                    bool skip = false;
                    var t = GetAankopenTransaction(i, out skip);
                    if (skip == false) { Transactions.Add(t); }
                }
                var verkopen = context.VW_Verkopen_ByJournal(125, 137);
                foreach (var i in verkopen)
                {
                    bool skip = false;
                    var t = GetVerkopenTransaction(i, out skip);
                    if (skip == false) { Transactions.Add(t); }
                }
                context.DetachExp_0071T();
            }
            Transactions.Sort();
        }
        static EMSourceModel GetAankopenTransaction(VW_Aankopen_ByJournal_Result i,out bool skip)
        {
            var t = new EMSourceModel();
            t.Account = i.Account.ToString();
            bool isRetour = (i.Amount < 0);
            t.Weight = (decimal) i.Weight;         
            t.Amount = (decimal) i.Amount;
            t.DocumentDate = i.DocumentDate;
            t.AccountName = i.ClientName;
            skip = false;
            switch (t.Account.left(5))
            {
                case "60011":
                    t.Goods = EnumEMGoods.Rough;
                    t.Inventory = EnumEMInventories.Inventory;
                    t.Document = FormatDocument((isRetour ? "PQX" : "PX"), i.DocumentNumber);
                    break;
                case "60012":
                    t.Goods = EnumEMGoods.Polished;
                    t.Inventory = EnumEMInventories.Inventory;
                    t.Document = FormatDocument((isRetour ? "PQX" : "PX"), i.DocumentNumber);
                    break;
                case "96011":
                    t.Goods = EnumEMGoods.Rough;
                    t.Inventory = EnumEMInventories.InboundShipments;
                    t.Document = FormatDocument((isRetour ? "PRX" : "PCX"), i.DocumentNumber);
                    break;
                case "96012":
                    t.Goods = EnumEMGoods.Polished;
                    t.Inventory = EnumEMInventories.Inventory;
                    t.Document = FormatDocument((isRetour ? "PRX" : "PCX"), i.DocumentNumber);
                    break;
                default:
                    skip = true;
                    break;
            }
            return t;
        }
        static EMSourceModel GetVerkopenTransaction(VW_Verkopen_ByJournal_Result i, out bool skip)
        {
            var t = new EMSourceModel();
            t.Account = i.ACCOUNT.ToString();
            bool isRetour = (i.Amount < 0);
            t.Weight = -(decimal)i.Weight;
            t.Amount = -(decimal)i.Amount;
            t.DocumentDate = i.DocumentDate;
            t.AccountName = i.ClientName;
            skip = false;
            switch (t.Account.left(5))
            {
                case "70011":
                    t.Goods = EnumEMGoods.Rough;
                    t.Inventory = EnumEMInventories.Inventory;
                    t.Document = FormatDocument((isRetour ? "PR" : "PS") , i.DocumentNumber);
                    break;
                case "70012":
                    t.Goods = EnumEMGoods.Polished;
                    t.Inventory = EnumEMInventories.Inventory;
                    t.Document = FormatDocument((isRetour ? "PR" : "PS"), i.DocumentNumber);
                    break;
                case "97011":
                    t.Goods = EnumEMGoods.Rough;
                    t.Inventory = EnumEMInventories.OutboundShipments;
                    t.Document = FormatDocument((isRetour ? "PFR" : "PF"), i.DocumentNumber);
                    break;
                case "97012":
                    t.Goods = EnumEMGoods.Polished;
                    t.Inventory = EnumEMInventories.OutboundShipments;
                    t.Document = FormatDocument((isRetour ? "PFR" : "PF"), i.DocumentNumber);
                    break;
                default:
                    skip = true;
                    break;
            }
            return t;
        }
        static string FormatDocument(string root,int document )
        {
            return root + yearcode + document.ToString().right(4);
        }
    }
}
