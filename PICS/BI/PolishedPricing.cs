using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS.BI
{
    public class PolishedPricing
    {
        public PolishedPricing(string parcel, string pricing, decimal value, decimal share, out decimal adjustedValue)
        {
            adjustedValue = 0;
            switch (pricing)
            {
                case "Special":
                    switch (parcel)
                    {
                        case "1076293":
                            adjustedValue = 40000000;
                            break;
                        case "1387067":
                            adjustedValue = 25000000;
                            break;
                        case "1379883":
                            adjustedValue = 25000000;
                            break;
                    }
                    break;
                case "WEB-25":
                    adjustedValue = value / share / (decimal)0.75;
                    break;
                case "RAP-50":
                    adjustedValue = value / share / (decimal)(0.5 / 0.7);
                    break;
                case "KUP-20":
                    adjustedValue = value / share / (decimal)0.8 ;
                    break;
                default:
                    break;
            }
        }
    }
}
