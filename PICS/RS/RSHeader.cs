using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class RSHeader
    {
        //TODO: Smell - RSTRansactionType should be an Enum of PICS
        public string RSTransactionType;
        public string Document;
        public DateTime DocumentDate;
        public string Description() { return $"[{RSTransactionType}] \t {Document} \t {DocumentDate.ToString("dd/MMM/yy")}"; }
    }
}
