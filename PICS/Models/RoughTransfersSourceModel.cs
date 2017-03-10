using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class RoughTransfersSourceModel
    {
        public string Document { get; set; }
        public string DocumentTypeString { get; set; }
        public string LogDate { get; set; }
        public DateTime DocumentDate { get; set; }
        public override bool Equals(object obj)
        {
            var p = obj as RoughTransfersSourceModel;
            return ((Document == p.Document) && (DocumentTypeString == p.DocumentTypeString));
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public EnumRSRoughtransferTypes DocumentType()
        {
            if (DocumentTypeString == "MIX ") return EnumRSRoughtransferTypes.Mix;
            return EnumRSRoughtransferTypes.Sort;
        }
        static public string GetDocumentTypeString(EnumRSRoughtransferTypes t)
        {
            if (t == EnumRSRoughtransferTypes.Mix) { return "MIX "; }
            return "SORT";
        }
        public string Description()
        {
            return DocumentTypeString + " " + Document;
        }
    }
}
