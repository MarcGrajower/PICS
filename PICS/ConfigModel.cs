using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PICS
{
    public class ConfigModel
    {
        public string SqlServer { get; set; }
        // TODO: Config - move to db
        public bool AllowPicsDatabaseInitialize { get; set; }
    }
}
