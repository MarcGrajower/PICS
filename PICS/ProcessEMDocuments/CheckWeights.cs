using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;

namespace PICS
{
    public class CheckWeights
    {
        public OperationResult or { get; set; }
        public decimal WeightDiff { get; set; }
        public CheckWeights(EMSourceModel t,RSLink rs)
        {
            or = new OperationResult();
            decimal emTotalWeight = t.Weight;
            decimal rsTotalWeight = rs.Details.Sum(m => m.Weight);
            EMCombines.GetCombined(t.Document);
            if ((EMCombines.InList == true) && (EMCombines.IsFirst == true))
            {
                emTotalWeight = EMCombines.Combined.Sum(m => m.EMDocument.Weight);
            }
            if (exceededTolerance(rsTotalWeight,emTotalWeight))
            {
                or.Success = false;
                Monitor.write(tolerance);
                Monitor.write($"{ t.Document}\t{ rs.Headers[0].Document}\t{ rs.Headers[0].RSTransactionType}\tEM[{ emTotalWeight: N2}\t]\tRS[{ rsTotalWeight: N2}]");
                or.AddMessage($"EM[{emTotalWeight:N2}]\tRS[{rsTotalWeight:N2}] {tolerance}");
            }
        }
        public CheckWeights(RSLink rm)
        {
            or = new OperationResult();
            decimal inWeight = rm.Details.Sum(r => r.Weight);
            decimal outWeight = rm.DetailsTo.Sum(r => r.Weight);
            if (exceededTolerance(outWeight, inWeight))
            {
                or.Success = false;
                Monitor.write(tolerance);
                string m = $"{ rm.Headers[0].Document} Transfer - Weight In { inWeight:n2}\tWeight Out {outWeight:n2}";
                Monitor.write(m);
                or.AddMessage($"[{m}] {tolerance}");
            }
        }
        bool exceededTolerance(decimal outWeight,decimal inWeight)
        {
            WeightDiff = Math.Abs(Math.Abs(outWeight) - Math.Abs(inWeight));
            if ((decimal.Compare(Math.Abs(Math.Abs(outWeight) / Math.Abs(inWeight) - 1), weightToleranceRate) == 1)) { return true; };
            if ((decimal.Compare(Math.Abs(Math.Abs(outWeight) - Math.Abs(inWeight) - 1), weightTolerance) == 1)) { return true; };
            return false;
        }
        // TODO: 1. Weight Tolerance.  2% or 5 crts. 
        static Decimal weightToleranceRate = 0.02m;
        static Decimal weightTolerance = 5;
        static string tolerance = $"Tolerance set at {weightToleranceRate * 100:N1} % or {weightTolerance:N1} crts";
    }
}
