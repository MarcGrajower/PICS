using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class RSDetail
    {
        public string RSDocument;
        public string Parcel;
        public decimal Weight;
        public decimal Amount;
        public string Description() { return $"[{Parcel}] \t {Weight:N2} \t {Amount:N2}"; }
    }

}
