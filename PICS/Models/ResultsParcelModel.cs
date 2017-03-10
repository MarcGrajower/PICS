using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class ResultsParcelsModel
    {
        public string Parcel { get; set; }
        public string DocumentType { get; set; }
        public string Document { get; set; }
        public DateTime DocumentDate { get; set; }
        public string Message { get; set; }
        public ResultsParcelsModel()
        {
            this.Parcel = "n.a.";
            this.DocumentType = "n.a.";
            this.Document = "n.a.";
            this.DocumentDate = DateTime.Now;
            this.Message = "n.a.";
        }
    }
}
