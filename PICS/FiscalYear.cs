using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PICS.EF;

namespace PICS
{
    static public class FiscalYear
    {
        static PICS.EF.Config _config;
        static FiscalYear()
        {
            using (var context = new PICSEntities(Config.model.SqlServer))
            {
                _config = context.Configs.FirstOrDefault();
            }
        }
        public static DateTime RSBeginningDate()
        {
            return _config.RSBeginningDate;
        }
        public static DateTime RSEndingDate()
        {
            return _config.RSEndingDate;
        }

    }
}
