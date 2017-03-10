using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;

namespace PICS
{
    public class RSPolmixes :RSLink
    {
        public List<PolmixSourceModel> PM { get; set; }
        public RSPolmixes()
        {
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            cmd.CommandText = "select PM_DocNr,PM_Date from PolMix where PM_Date between ? and ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", FiscalYear.RSBeginningDate());
            cmd.Parameters.AddWithValue("?", FiscalYear.RSEndingDate());
            dt.Clear();
            da.Fill(dt);
            PM = new List<PolmixSourceModel>();
            foreach(DataRow r in dt.Rows)
            {
                var p = new PolmixSourceModel();
                p.Document = (string)r["PM_DocNr"];
                p.DocumentDate = (DateTime)r["PM_Date"];
                PM.Add(p);
            }
        }
        public void Load(string document)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            var dtDatailTo = new DataTable();
            cmd.CommandText = "select * from POLMIX where PM_Docnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", document);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{document} not found in PolMix.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            DetailsTo.Clear();
            foreach (DataRow r in dt.Rows)
            {
                    var h = new RSHeader();
                    h.RSTransactionType = "MIX";
                    h.Document = (string)r["PM_DOCNR"];
                    h.DocumentDate = (DateTime)r["PM_DATE"];
                    Headers.Add(h);
                    cmd.CommandText = "select * from POLMIXi where PMi_DOCNR = ?";
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("?", h.Document);
                    dtDetail.Clear();
                    da.Fill(dtDetail);
                    foreach (DataRow rd in dtDetail.Rows)
                    {
                        var d = new RSDetail();
                        d.RSDocument = h.Document;
                        d.Parcel = (string)rd["PMi_PNR"];
                        d.Weight = (decimal)rd["PMi_Weight"];
                        d.Amount = (decimal)rd["PMi_Weight"] * (decimal)rd["PMi_PriceCost"];
                        Details.Add(d);
                    }
                cmd.CommandText = "select * from POLMIXo where PMo_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["PMo_PNR"];
                    d.Weight = (decimal)rd["PMo_Weight"];
                    d.Amount = (decimal)rd["PMo_Weight"] * (decimal)rd["PMo_PriceCost"];
                    DetailsTo.Add(d);
                }
            }
        }
    }
}
