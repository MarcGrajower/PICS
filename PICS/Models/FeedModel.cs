using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class FeedModel : IComparable<FeedModel>
    {
        public DateTime DocumentDate { get; set; }
        public EnumFeedorder FeedOrder { get; set; }
        public int Sequence { get; set; } // you need this for rough transfers
        public string Document { get; set; }
        public string Source { get; set; }
        public object Record { get; set; }

        int IComparable<FeedModel>.CompareTo(FeedModel other)
        {
            if (DocumentDate !=other.DocumentDate) { return DocumentDate.CompareTo(other.DocumentDate); }
            if (FeedOrder != other.FeedOrder) { return FeedOrder.CompareTo(other.FeedOrder); }
            if (Sequence != other.Sequence) { return Sequence.CompareTo(other.Sequence); }
            return (Document.CompareTo(other.Document));
        }
    }
}
