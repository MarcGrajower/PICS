using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class ResultsDocumentModel : IComparable<ResultsDocumentModel>
    {
        public string Document { get; set; }
        public DateTime DocumentDate { get; set; }
        public bool Success { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public string GoodsString { get; set; }
        public string AccountName { get; set; }
        public string Observation { get; set; }
        public decimal WeightDifference { get; set; }
        public decimal ModulationRatio { get; set; }
        internal string description()
        {
            return $"{Document} \t {Weight:N2} \t {GoodsString} \t {AccountName} \t {Severity} \t {Message} ";
        }
        public int CompareTo(ResultsDocumentModel other)
        {
            return DocumentDate.CompareTo(other.DocumentDate);
        }
    }
}
