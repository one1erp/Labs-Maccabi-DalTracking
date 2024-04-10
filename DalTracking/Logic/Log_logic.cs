using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;

namespace DalTracking.Logic
{
    internal class Log_logic
    {
        private OracleConnection _connection;
        private double _sessionId;
        private Dictionary<string, string> _diclabNames;
        private OracleCommand cmd;

        public Log_logic(OracleConnection connection, double sessionId,
            Dictionary<string, string> diclabNames)
        {
            this._connection = connection;
            this._sessionId = sessionId;
            this._diclabNames = diclabNames;
        }

        internal void Add2Log(TrackItem titem, string desc, string appCode)
        {
            var sdgId4Log = GetSdgId(titem.U_TRACK_ITEM_ID, titem.U_TRACK_TABLE_NAME);




            int temp;

            if (sdgId4Log != null && int.TryParse(sdgId4Log, out temp))
            {

                try
                {
                    string msg = titem.U_TRACK_TABLE_NAME + " " + titem.U_TRACK_ITEM_NAME + desc;// + "from or To" + GetLabLocation(toLab);
                    string sql = string.Format("Insert Into lims_sys.Sdg_Log (sdg_id, time, application_code, session_id, description) values ('{0}',sysdate,'{1}','{2}','{3}')"
                        , sdgId4Log, appCode, _sessionId, msg);
                    cmd = new OracleCommand(sql, _connection);

                    var res =
                        cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }
                catch (Exception e)
                {
                    //Logger.WriteLogFile(e);


                }

            }
        }

        internal void Add2Log(EntityDetails ed)
        {
            string sdgId4Log = "";

            sdgId4Log = GetSdgId(ed.Id, ed.Table_Name);

            int temp;

            if (sdgId4Log != null && int.TryParse(sdgId4Log, out temp))
            {

                try
                {
                    string sql = string.Format("Insert Into lims_sys.Sdg_Log (sdg_id, time, application_code, session_id, description) values ('{0}',sysdate,'{1}','{2}','{3}')"
                        , sdgId4Log, Constants._appLogParam_req.Trim(), _sessionId, ed.EntityType + " " + ed.Name + " From " + GetLabLocation(ed.LabLocation));
                    cmd = new OracleCommand(sql, _connection);

                    var res =
                        cmd.ExecuteNonQuery();
                    cmd.Dispose();

                }
                catch (Exception e)
                {
                    //Logger.WriteLogFile(e);


                }
            }
        }

        private string GetLabLocation(string labName)
        {
            if (_diclabNames.ContainsKey(labName))
                return _diclabNames[labName];

            return "";
        }

        private string GetSdgId(string entityId, string _tableName)
        {
            string sql;
            try
            {

                sql = string.Empty;
                switch (_tableName)
                {
                    case "SDG":
                        sql = "SELECT SDG_ID FROM lims_sys.SDG where SDG_ID='" + entityId + "'";
                        break;
                    case "SAMPLE":
                        sql = "SELECT SDG_ID FROM  lims_sys.Sample where sample_id='" + entityId + "'";
                        break;
                    case "ALIQUOT":
                        sql = " SELECT Sample.SDG_ID FROM lims_sys.Sample where lims_sys.sample.sample_id in(SELECT  lims_sys.aliquot.sample_id FROM  lims_sys.aliquot where  lims_sys.aliquot.aliquot_id='" + entityId + "')";
                        break;
                    default:
                        sql = string.Empty; ;
                        break;
                }

                if (!string.IsNullOrEmpty(sql))
                {
                    cmd = new OracleCommand(sql, _connection);

                    var res = cmd.ExecuteScalar();

                    if (res != null)
                    {
                        var id = res.ToString();
                        return id;
                    }
                    cmd.Dispose();

                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return null;
        }


        string GetText(string fromLab, string toLab = null)
        {
            if (fromLab == "98" || fromLab == "דרום")
            {

            }
            return null;
        }
    }
}
