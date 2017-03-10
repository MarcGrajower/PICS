using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class EMSourceModel : IComparable<EMSourceModel>
    {
        public string Document { get; set; }
        string swappedDocument { get; set; }  // for EMCombine
        public EnumEMGoods Goods { get; set; }
        public EnumEMInventories Inventory { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string Account { get; set; }
        public DateTime DocumentDate { get; set; }
        public string AccountName { get; set; }
        public int CompareTo(EMSourceModel t1)
        {
            if (this.DocumentDate.CompareTo(t1.DocumentDate)==0)
            {
                return -(this.Amount.CompareTo(t1.Amount));
            }
            return this.DocumentDate.CompareTo(t1.DocumentDate);
        }
        public string GoodsString()
        {
            return (this.Goods == EnumEMGoods.Polished ? "Polished" : "Rough");
        }
        //TODO: smell - needs to be implemented
        public int sign()
        {
            return Weight < 0 ? -1 : 1;
        }
        public void Swap(string document)
        {
            swappedDocument = Document;
            Document = document;
        }
        public void UnSwap()
        {
            Document = swappedDocument;
        }
        public string Description()
        {
            return $"{Document} {DocumentDate.ToString("dd/MMM/yy")} {AccountName} {Weight:N2} {Amount:N2}";
        }

    }
}
