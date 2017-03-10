using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;

namespace PICS
{
    public class RSRoughTransfers :RSLink
    {
        public RSRoughTransfers()
        {
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            cmd.CommandText = @"SELECT CAST(""MIX "" as c(4)) as DocumentType, mix_docnr as Document, mix_date as documentDate,log.*";
            cmd.CommandText += " FROM mix, log WHERE log_docnr = mix_docnr  AND log_filename = ? ";
            cmd.CommandText += " and mix_date between ? AND ? ";
            cmd.CommandText += @" UNION SELECT CAST(""SORT"" as c(4)) as DocumentType, srt_docnr as document,srt_date as documentDate,log.*";
            cmd.CommandText += $" FROM sort, log WHERE log_docnr = srt_docnr  AND log_filename = ? ";
            cmd.CommandText += $" AND srt_date between ? AND ? ORDER BY log_date,log_time,Document";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", $"{"MIX"}");
            cmd.Parameters.AddWithValue("?", FiscalYear.RSBeginningDate());
            cmd.Parameters.AddWithValue("?", FiscalYear.RSEndingDate());
            cmd.Parameters.AddWithValue("?", $"{"SORT"}");
            cmd.Parameters.AddWithValue("?", FiscalYear.RSBeginningDate());
            cmd.Parameters.AddWithValue("?", FiscalYear.RSEndingDate());
            dt.Clear();
            da.Fill(dt);
            var work = new List<RoughTransfersSourceModel>();
            foreach (DataRow r in dt.Rows)
            {
                var rsrt = new RoughTransfersSourceModel();
                rsrt.Document = (string)r["Document"];
                rsrt.DocumentDate = (DateTime)r["documentDate"];
                rsrt.DocumentTypeString = (string)r["DocumentType"];
                rsrt.LogDate = ((DateTime)r["Log_Date"]).ToString("yyyyMMMdd") + (string)r["Log_Time"];
                work.Add(rsrt);
            }
            RSRT = work.GroupBy(p => p.Document + p.DocumentTypeString).Select(g => g.First()).ToList();
        }
        public void Load(RoughTransfersSourceModel transfer)
        {
            OpResult.Clear();
            if (transfer.DocumentTypeString == RoughTransfersSourceModel.GetDocumentTypeString(EnumRSRoughtransferTypes.Mix))
            {
                loadMix(transfer);
                return;
            }
            loadSort(transfer);
        }
        void loadMix(RoughTransfersSourceModel transfer)
        {
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            var dtDatailTo = new DataTable();
            cmd.CommandText = "select * from MIX where MIX_Docnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", transfer.Document);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{transfer.Document} not found in MIX.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            DetailsTo.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "MIX";
                h.Document = (string)r["MIX_DOCNR"];
                h.DocumentDate = (DateTime)r["MIX_DATE"];
                Headers.Add(h);
                var detail = new RSDetail();
                detail.RSDocument = h.Document;
                detail.Parcel = (string)r["MIX_PARCEL"];
                detail.Weight = (decimal)r["MIX_Weight"];
                detail.Amount = (decimal)r["MIX_Weight"] * (decimal)r["MIX_Price"];
                DetailsTo.Add(detail);
                cmd.CommandText = "select * from MIXd where MIXd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["MIXd_Parcel"];
                    d.Weight = (decimal)rd["Mixd_Weight"];
                    d.Amount = (decimal)rd["Mixd_Weight"] * (decimal)rd["Mixd_Price"];
                    Details.Add(d);
                }
            }
        }
        void loadSort(RoughTransfersSourceModel transfer)
        {
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            var dtDatailTo = new DataTable();
            cmd.CommandText = "select * from SORT where SRT_Docnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", transfer.Document);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{transfer.Document} not found in MIX.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            DetailsTo.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "SORT";
                h.Document = (string)r["SRT_DOCNR"];
                h.DocumentDate = (DateTime)r["SRT_DATE"];
                Headers.Add(h);
                dtDetail.Clear();
                da.Fill(dtDetail);
                var detail = new RSDetail();
                detail.RSDocument = h.Document;
                detail.Parcel = (string)r["SRT_PARCEL"];
                detail.Weight = (decimal)r["SRT_Weight"];
                detail.Amount = (decimal)r["SRT_Weight"] * (decimal)r["SRT_Price"];
                Details.Add(detail);
                cmd.CommandText = "select * from SoRTd where SRTd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["SRTd_Parcel"];
                    d.Weight = (decimal)rd["SRTd_Weight"];
                    d.Amount = (decimal)rd["SRTd_Weight"] * (decimal)rd["SRTd_Price"];
                    DetailsTo.Add(d);
                }
            }
        }
        public List<RoughTransfersSourceModel> RSRT;
    }
}
