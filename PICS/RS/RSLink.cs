using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Common;
using System.Data.OleDb;
using System.Data;
namespace PICS
{
    public class RSLink : RSConnection
    {
        public void LinkDocument(EMSourceModel t)
        {
            if ((EMCombines.InList == true) && (EMCombines.IsFirst == true))
            {
                t.Swap(EMCombines.Combined[0].EMCombineDocument);
            }
            OpResult.MessageList.Clear();
            if (t.Goods == EnumEMGoods.Rough)
            {
                if (t.Document.Substring(1,1) == "F")
                {
                    getLocation(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 2) == "RX")
                {
                    // getRoughBoxes(t); if (OpResult.Success == true) { return; }
                    // getLocation(t); if (OpResult.Success == true) { return; }
                    getSeneca(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "R")
                {
                    getLocation(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 2) == "QX")
                {
                    // getRoughBoxes(t); if (OpResult.Success == true) { return; }
                    // getLocation(t); if (OpResult.Success == true) { return; }
                    getSeneca(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "S")
                {
                    getRoughSale(t.Document); if (OpResult.Success == true) { return; }
                    getLocation(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "X")
                {
                    getRoughBoxes(t);if (OpResult.Success == true) { return; }
                    getLocation(t); if (OpResult.Success == true) { return; }
                    getSeneca(t); if (OpResult.Success == true) { return; }
                    getRetour(t); if (OpResult.Success == true) { return; }
                    return;
                }
            }
            if (t.Goods == EnumEMGoods.Polished)
            {
                if (t.Document.Substring(1, 2) == "CX")
                {
                    getOGReturns(t); if (OpResult.Success == true) { return; }
                    getTheirgood(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 2) == "FR")
                {
                    getOGReturns(t); if (OpResult.Success == true) { return; }
                    return;
                }

                if (t.Document.Substring(1, 1) == "F")
                {
                    getOurgoods(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "R")
                {
                    getOGReturns(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "S")
                {
                    getOurgoods(t); if (OpResult.Success == true) { return; }
                    getSalesReport(t); if (OpResult.Success == true) { return; }
                    getPolSal(t); if (OpResult.Success == true) { return; }
                    return;
                }
                if (t.Document.Substring(1, 1) == "X")
                {
                    getOGReturns(t); if (OpResult.Success == true) { return; }
                    getPurchases(t); if (OpResult.Success == true) { return; }
                    return;
                }
            }
            OpResult.Success = false;
            OpResult.AddMessage($"Transaction type not yet implemented");
        }
        private void getPurchases(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from Purchase where PUR_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.Purchase.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "PURCHASE";
                h.Document = (string)r["PUR_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from PURCHASEd where PURd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["PURd_PNR"];
                    d.Weight = (decimal)rd["PURd_Weight"];
                    d.Amount = (decimal)rd["PURd_Weight"] * (decimal)rd["PURd_Price"];
                    Details.Add(d);
                }
            }
        }
        private void getRetour(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from Retour where ret_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.Retour.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "RETOUR";
                h.Document = (string)r["RET_DOCNR"];
                h.DocumentDate = (DateTime)dt.Rows[0]["RET_DATE"];
                Headers.Add(h);
                cmd.CommandText = "select * from retourd where retd_Docnr = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["retd_Parcel"];
                    d.Weight = (decimal)rd["retd_Weight"];
                    d.Amount = (decimal)rd["retd_Weight"] * (decimal)rd["retd_Price"];
                    Details.Add(d);
                }
            }
        }
        internal void getTheirgood(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from TheirGoods where TG_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.TheirGoods.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "THEIRGOODS";
                h.Document = (string)r["TG_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from THEIRGOODSd where TGd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["TGd_PNR"];
                    d.Weight = (decimal)rd["TGd_Weight"];
                    d.Amount = (decimal)rd["TGd_Weight"] * (decimal)rd["TGd_PriceMemo"];
                    Details.Add(d);
                }
            }
        }
        internal void getSeneca(EMSourceModel t)
        {
            OpResult.Success = false;
            var lines = EMSenecas.GetLines(t.Document);
            if (lines.Count == 0)
            {
                OpResult.MessageList.Add($"{t.Document} not found in Seneca");
                return;
            }
            Headers.Clear();
            Details.Clear();
            var h = new RSHeader();
            h.RSTransactionType = "SENECA";
            h.Document = t.Document;
            h.DocumentDate = t.DocumentDate;
            Headers.Add(h);
            foreach(var l in lines)
            {
                var d = new RSDetail();
                d.RSDocument = h.Document;
                d.Parcel = l.Parcel;
                d.Weight = l.Weight;
                d.Amount = l.Amount;
                Details.Add(d);
            }
            OpResult.Success = true;
        }
        internal void getPolSal(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from Polsal where PS_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.PolSal.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "POLSAL";
                h.Document = (string)r["PS_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from POLSALd where PSd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["PSd_PNR"];
                    d.Weight = (decimal)rd["PSd_Weight"];
                    d.Amount = (decimal)rd["PSd_Weight"] * (decimal)rd["PSd_PriceSal"];
                    Details.Add(d);
                }
            }
        }
        internal void getSalesReport(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from SalRep where SR_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.SalRep.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "SALREP";
                h.Document = (string)r["SR_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from SALREPD where SRd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document.Trim());
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["SRd_PNR"];
                    d.Weight = (decimal)rd["SRd_Weight"];
                    d.Amount = (decimal)rd["SRd_Weight"] * (decimal)rd["SRd_Price"];
                    Details.Add(d);
                }
            }
        }
        internal void getOGReturns(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from OGReturn where OGR_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.OGReturn.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "OGRETURN";
                h.Document = (string)r["OGR_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from OGRETURND where OGRd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document.Trim());
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["OGRd_PNR"];
                    d.Weight = (decimal)rd["OGRd_Weight"];
                    d.Amount = (decimal)rd["OGRd_Weight"] * (decimal)rd["OGRd_PriceMemo"];
                    Details.Add(d);
                }
            }
        }
        internal void getLocation(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from Loc where LOC_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.LOC.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "LOCATION";
                h.Document = (string)r["LOC_DOCNR"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from LOCD where LOCd_DOCNR = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["LOCd_Parcel"];
                    d.Weight = (decimal)rd["LOCd_Weight"];
                    d.Amount = (decimal)rd["LOCd_Weight"] * (decimal)rd["LOCd_Price"];
                    Details.Add(d);
                }
            }
        }
        internal void getRoughBoxes(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from box where box_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.Box.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "BOX";
                h.Document = (string)r["BOX_PARCEL"];
                h.DocumentDate = t.DocumentDate;
                Headers.Add(h);
                cmd.CommandText = "select * from BOXASS where BA_BOXPARCEL = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document);
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["BA_Parcel"];
                    d.Weight = (decimal)rd["ba_Weight"];
                    d.Amount = (decimal)rd["ba_Weight"] * (decimal)rd["ba_Price"];
                    Details.Add(d);
                }
            }
            da.Fill(dt);
        }
        internal void getRoughSale(string document)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from sale where sal_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{document} not found in RS.Sale.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "SALE";
                h.Document = (string)r["SAL_DOCNR"];
                h.DocumentDate = (DateTime)dt.Rows[0]["SAL_DATE"];
                Headers.Add(h);
                cmd.CommandText = "select * from saled where sald_Docnr = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document.Trim());
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["sald_Parcel"];
                    d.Weight = (decimal)rd["sald_Weight"];
                    d.Amount = (decimal)rd["sald_Weight"] * (decimal)rd["sald_Price"];
                    Details.Add(d);
                }
            }
        }
        internal void getOurgoods(EMSourceModel t)
        {
            OpResult.Clear();
            var da = new OleDbDataAdapter(cmd);
            var dt = new DataTable();
            var dtDetail = new DataTable();
            cmd.CommandText = "select * from ourgoods where og_Invnr = ?";
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("?", t.Document.Trim());
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                OpResult.Success = false;
                OpResult.MessageList.Add($"{t.Document} not found in RS.Ourgoods.");
                return;
            }
            Headers.Clear();
            Details.Clear();
            foreach (DataRow r in dt.Rows)
            {
                var h = new RSHeader();
                h.RSTransactionType = "OURGOODS";
                h.Document = (string)r["OG_DOCNR"];
                h.DocumentDate = (DateTime)dt.Rows[0]["OG_DATE"];
                Headers.Add(h);
                cmd.CommandText = "select * from ourgoodsd where ogd_Docnr = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("?", h.Document.Trim());
                dtDetail.Clear();
                da.Fill(dtDetail);
                foreach (DataRow rd in dtDetail.Rows)
                {
                    var d = new RSDetail();
                    d.RSDocument = h.Document;
                    d.Parcel = (string)rd["ogd_Pnr"];
                    d.Weight = (decimal)rd["ogd_Weight"];
                    d.Amount = (decimal)rd["ogd_Weight"] * (decimal)rd["ogd_PriceMemo"];
                    Details.Add(d);
                }
            }
        }
    }
}
