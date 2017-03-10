using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class EMSenecaModel
    {
        public string EMDocument { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Parcel { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string Description()
        {
            return $"{EMDocument} \t {DocumentDate.ToString("dd-MMM-yy")} \t {Parcel} \t {Weight:N2} \t {Amount:N2} ";
        }
    }
}
