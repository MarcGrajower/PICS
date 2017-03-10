using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using PICS.EF;
using Core.Common;

namespace PICS.BI
{
    public class BIEarlySales
    {

        public void Process(string parcel, EnumEMGoods goods, EnumEMInventories inventory)
        {
            decimal weight = 0;
            decimal value = 0;
            var da = new OleDbDataAdapter(rs.cmd);
            var dt = new DataTable();
            if (Helpers.ExistsParcel(parcel, inventory, context)) { return; };
            or.Clear();
            if (goods == EnumEMGoods.Polished)
            {
                if (searchSalesReports(rs, parcel, out weight, out value) == false)
                {
                    if (searchPolSales(rs, parcel, out weight, out value) == false)
                    {
                        or.Success = false;
                        or.MessageList.Add($"[EarlySales] {parcel} could not be coopted. ");
                        return;
                    }
                }
            }
            else
            {
                if (searchSales(rs, parcel, out weight, out value) == false)
                {
                    or.Success = false;
                    or.MessageList.Add($"[EarlySales] {parcel} could not be coopted. ");
                    return;
                }
            }
            BISaveTransaction.SaveTransaction(parcel, weight, value, EnumEMInventories.Inventory, goods, context);
            or.Success = true;
        }
        bool searchSalesReports(RSConnection rs, String parcel, out decimal weight, out decimal value)
        {
            weight = 0;
            value = 0;
            var da = new OleDbDataAdapter(rs.cmd);
            var dt = new DataTable();
            rs.cmd.CommandText = "SELECT srd_weight,srd_weight * srd_price as value FROM SalRep, SalRepd ";
            rs.cmd.CommandText += "WHERE srd_docnr = sr_docnr ";
            rs.cmd.CommandText += "AND sr_date < DATE(2017, 1, 16) ";
            rs.cmd.CommandText += "and srd_PNR = ?";
            rs.cmd.Parameters.Clear();
            rs.cmd.Parameters.AddWithValue("?", parcel);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                return false;
            }
            weight = (decimal)dt.Rows[0]["srd_weight"];
            value = (decimal)dt.Rows[0]["value"] * PolishedCostOfGoods;
            return true;
        }
        bool searchPolSales(RSConnection rs, String parcel, out decimal weight, out decimal value)
        {
            var da = new OleDbDataAdapter(rs.cmd);
            var dt = new DataTable();
            weight = 0;
            value = 0;
            rs.cmd.CommandText = "SELECT psd_weight,psd_weight * psd_pricesal as value FROM PolSal, Polsald ";
            rs.cmd.CommandText += "WHERE psd_docnr = ps_docnr ";
            rs.cmd.CommandText += "AND ps_date < DATE(2017, 1, 16) ";
            rs.cmd.CommandText += "and psd_PNR = ?";
            rs.cmd.Parameters.Clear();
            rs.cmd.Parameters.AddWithValue("?", parcel);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                return false;
            }
            weight = (decimal)dt.Rows[0]["psd_weight"];
            value = (decimal)dt.Rows[0]["value"] * PolishedCostOfGoods;
            return true;
        }
        bool searchSales(RSConnection rs, String parcel, out decimal weight, out decimal value)
        {

            weight = 0;
            value = 0;
            var da = new OleDbDataAdapter(rs.cmd);
            var dt = new DataTable();
            rs.cmd.CommandText = "SELECT ba_weight,ba_weight * ba_price as value FROM saled, sale, boxass ";
            rs.cmd.CommandText += "WHERE sald_docnr = sal_docnr ";
            rs.cmd.CommandText += "and ba_parcel = sald_parcel ";
            rs.cmd.CommandText += "AND sal_date < DATE(2017, 1, 16) ";
            rs.cmd.CommandText += "and BA_Parcel = ?";
            rs.cmd.Parameters.Clear();
            rs.cmd.Parameters.AddWithValue("?", parcel);
            dt.Clear();
            da.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                return false;
            }
            weight = (decimal)dt.Rows[0]["ba_weight"];
            value = (decimal)dt.Rows[0]["value"] * PolishedCostOfGoods;
            return true;
        }
        public BIEarlySales(RSConnection rsP,PICSEntities contextP)
        {
            rs = rsP;
            context = contextP;
            or = new OperationResult();
        }
        public OperationResult or;
        internal RSConnection rs;
        internal PICSEntities context;
        //TODO: Config - Polished Cost of Goods rate hardcoded in [BiEarlySales]
        internal decimal PolishedCostOfGoods = 0.97m;
    }
}
