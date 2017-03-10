using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using Core.Common;

namespace PICS
{
    public class RSConnection
    {
        public List<RSHeader> Headers = new List<RSHeader>();
        public List<RSDetail> Details = new List<RSDetail>();
        public List<RSDetail> DetailsTo = new List<RSDetail>();
        public OperationResult OpResult;
        public RSConnection()
        { 
            OpResult = new OperationResult();
            cmd = new OleDbCommand();
            connect();
        }
        //TODO: Move to config Rough configuation string
        string connectionstring = @"Provider=VFPOLEDB; Data Source= v:\data\plucz\plucz.dbc;Password='';Collating Sequence=MACHINE";
        public OleDbCommand cmd { get; set; }
        void connect()
        {
            var conn = new OleDbConnection(connectionstring);
            conn.Open();
            cmd.Connection = conn;
        }
    }
}
