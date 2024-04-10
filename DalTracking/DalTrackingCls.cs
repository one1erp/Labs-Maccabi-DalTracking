using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
//using Oracle.ManagedDataAccess.Client;
using LSSERVICEPROVIDERLib;
using LSExtensionWindowLib;
using System.Windows.Forms;



namespace DalTracking
{
    public class DalTrackingCls
    {

        public OracleConnection _connection;

        public OracleCommand cmd;
        List<SdgObj> _listSdgObj;

        private string sql
            ;
        Logic.VirtualLogic virtualLogic;
        public DalTrackingCls()
        {


            _diclabNames = new Dictionary<string, string>();
            _listSdgObj = new List<SdgObj>();
        }
        void clear()
        {
            _listSdgObj.Clear();
        }


        double SessionId = 1;
        public OracleConnection GetConnection(INautilusDBConnection ntlsCon, INautilusUser _ntlsUser, IExtensionWindowSite2 _ntlsSite, string _connectionString, double sessionId, OracleCommand cmd, OracleConnection _connection)
        {

            if (ntlsCon != null)
            {
                this.SessionId = ntlsCon.GetSessionId();
                // Initialize variables
                String roleCommand;
                // Try/Catch block
                try
                {


                    _connectionString = ntlsCon.GetADOConnectionString();

                    var splited = _connectionString.Split(';');

                    var cs = "";

                    for (int i = 1; i < splited.Count(); i++)
                    {
                        cs += splited[i] + ';';
                    }

                    var username = ntlsCon.GetUsername();
                    if (string.IsNullOrEmpty(username))
                    {
                        var serverDetails = ntlsCon.GetServerDetails();
                        cs = "User Id=/;Data Source=" + serverDetails + ";";
                    }

                    //Create the connection
                    _connection = new OracleConnection(cs);

                    // Open the connection
                    _connection.Open();

                    // Get lims user password
                    string limsUserPassword = ntlsCon.GetLimsUserPwd();

                    // Set role lims user
                    if (limsUserPassword == "")
                    {
                        // LIMS_USER is not password protected
                        roleCommand = "set role lims_user";
                    }
                    else
                    {
                        // LIMS_USER is password protected.
                        roleCommand = "set role lims_user identified by " + limsUserPassword;
                    }

                    // set the Oracle user for this connecition
                    cmd = new OracleCommand(roleCommand, _connection);

                    // Try/Catch block
                    try
                    {
                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception f)
                    {
                        // Throw the exception
                        throw new Exception("Inconsistent role Security : " + f.Message);
                    }

                    // Get the session id
                    sessionId = ntlsCon.GetSessionId();

                    // Connect to the same session
                    string sSql = string.Format("call lims.lims_env.connect_same_session({0})", sessionId);

                    // Build the command
                    cmd = new OracleCommand(sSql, _connection);

                    // Execute the command
                    cmd.ExecuteNonQuery();

                }
                catch (Exception e)
                {
                    // Throw the exception
                    throw e;
                }
                // Return the connection
            }

            this.cmd = cmd;
            this._connection = _connection;
            return _connection;
        }

        Dictionary<string, string> _diclabNames;
        public string GetLabLocation(string labName)
        {
            if (_diclabNames.ContainsKey(labName)) return _diclabNames[labName];

            sql = string.Format("select lims.phrase_lookup (  'Lab Location' , '{0}' ) lab_name from dual", labName);
            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                MessageBox.Show("The Lab " +
                     labName +
                    "  does not exist!", "Nautilus",
                    MessageBoxButtons.OK, MessageBoxIcon.Hand);

                return null;
            }
            else
            {
                while (reader.Read())
                {

                    var l = reader["LAB_NAME"].ToString();
                    _diclabNames.Add(labName, l);
                    return l;
                }
                return null;
            }
        }

        #region LoadEntity
        private string GetSdgId(string barcode)
        {

            //Search wich
            cmd = new OracleCommand(sql, _connection);
            sql = string.Format("select sdg_id from lims_sys.sdg where name='{0}'", barcode);
            cmd.CommandText = sql;
            var res = cmd.ExecuteScalar();

            if (res == null)
            {
                sql = string.Format("select sdg_id from lims_sys.sample where name='{0}'", barcode);
                cmd.CommandText = sql;
                res = cmd.ExecuteScalar();


                if (res == null)
                {
                    sql = string.Format("select s.sdg_id from lims_sys.aliquot a,lims_sys.sample s where a.sample_id=s.sample_id and  a.name='{0}'", barcode);
                    cmd.CommandText = sql;
                    res = cmd.ExecuteScalar();


                    if (res == null)
                    {


                        return null;
                    }
                }
            }
            return res.ToString();
        }

        public SdgObj GetSDG(string barcode)
        {
            SdgObj newSdg = null;

            string sdgId = GetSdgId(barcode);

            if (string.IsNullOrEmpty(sdgId)) return null;


            sql = string.Format("SELECT d.sdg_id, d.name sdg_name, d.status sdg_status, du.u_lab_location sdg_location,d.External_Reference " +
                "FROM lims_sys.sdg d INNER JOIN lims_sys.sdg_user du ON d.sdg_id =du.sdg_id " +
                " WHERE  d.sdg_id={0} ", sdgId);

            cmd.CommandText = sql;
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return null;
            }
            else
            {

                while (reader.Read())
                {
                    newSdg = new SdgObj(
                        reader["SDG_ID"].ToString(),
                        reader["SDG_NAME"].ToString(),
                        reader["SDG_STATUS"].ToString(),
                        reader["SDG_LOCATION"].ToString(),
                        reader["External_Reference"].ToString(),
                        new List<SampleObj>() { }
                        );
                }
                if (newSdg != null && newSdg.GetDep()    != Department.None)
                {
                    List<SampleObj> samples = GetSamples(newSdg.Id);
                 //   samples = samples.ToList();          //02/04/23
                    if (samples.Count > 0)
                    {

                        newSdg.samples = samples;
                        List<BlockObj> blocks = GetBlocks(string.Join(", ", samples.Select(s => s.Id)));

                        foreach (var item in samples)
                        {
                            item.SdgParent = newSdg;                        
                            var bl = blocks.Where(x => x.parent == item.Id);
                            item.Blocks.AddRange(bl);
                        }


                        if (blocks.Count > 0)
                        {

                            List<SlideObj> slides = GetSlides(string.Join(", ", blocks.Select(s => s.Id)));
                            if (slides.Count > 0)
                            {
                                foreach (SlideObj slide in slides)
                                {
                                    try
                                    {
                                        var spl = slide.Name.Split('.');
                                        string smpl_name = spl[0] + "." + spl[1];
                                        string alq_name = spl[0] + "." + spl[1] + "." + spl[2];

                                        SampleObj samp = newSdg.samples.FirstOrDefault(s => s.Name == smpl_name);

                                        BlockObj blck = samp.Blocks.FirstOrDefault(a => a.Name == alq_name);
                                        blck.SdgParent = newSdg;
                                        slide.SdgParent = newSdg;

                                        blck.slides.Add(slide);

                                    }
                                    catch (Exception en)
                                    {
                                        ///נופל כאן ספציפית עם סליידים של בלוק סמפל מסוים
                                        MessageBox.Show(en.Message);

                                    }
                                }
                            }

                        }
                    }
                }
            }
            
            HandleVirtual(newSdg, barcode);
            return newSdg;
        }

        private List<SampleObj> GetSamples(string sdg_id)
        {
            List<SampleObj> samples = new List<SampleObj>() { };

            sql = string.Format("SELECT s.sample_id, s.name sample_name, s.status sample_status, su.u_lab_location sample_location, s.sdg_id " +
                "FROM lims_sys.sample s INNER JOIN lims_sys.sample_user su ON su.sample_id = s.sample_id " +
                "WHERE sdg_id in ('{0}') ", sdg_id);

            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return samples;
            }
            else
            {
                while (reader.Read())
                {
                    samples.Add(new SampleObj(
                        reader["SAMPLE_ID"].ToString(),
                        reader["SAMPLE_NAME"].ToString(),
                        reader["SAMPLE_STATUS"].ToString(),
                        reader["SAMPLE_LOCATION"].ToString()

                        ));
                }
                return samples;
            }
        }
        private List<BlockObj> GetBlocks(string samples)
        {
            List<BlockObj> blocks = new List<BlockObj>() { };

            sql = string.Format("SELECT a.aliquot_id block_id, a.name block_name, a.status block_status, au.u_lab_location block_location ,a.sample_id ,au.U_IS_CELL_BLOCK  , Au.U_Aliquot_Station ,au.U_Glass_Type " +
                "FROM lims_sys.aliquot a INNER JOIN lims_sys.aliquot_user au ON a.aliquot_id = au.aliquot_id " +
                "WHERE a.sample_id in ({0})  " +
                "AND a.aliquot_id NOT IN (SELECT child_aliquot_id FROM lims_sys.aliquot_formulation WHERE child_aliquot_id = a.aliquot_id)", samples);



            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return blocks;
            }
            else
            {
                while (reader.Read())
                {
                    blocks.Add(new BlockObj(
                        reader["BLOCK_ID"].ToString(),
                        reader["BLOCK_NAME"].ToString(),
                        reader["BLOCK_STATUS"].ToString(),
                        reader["BLOCK_LOCATION"].ToString(),
                         reader["U_IS_CELL_BLOCK"].ToString(),
                        reader["U_Aliquot_Station"].ToString(),
                        reader["U_Glass_Type"].ToString(),
                        reader["SAMPLE_ID"].ToString()



                        ));
                }


                return blocks;
            }
        }
        private List<SlideObj> GetSlides(string blocks)
        {
            List<SlideObj> slides = new List<SlideObj>() { };

            sql = string.Format("SELECT ac.aliquot_id slide_id, ac.name slide_name, ac.status slide_status , acu.u_lab_location slide_location ,P.PARENT_ALIQUOT_ID parent_id , U_Aliquot_Station ,u_color_type " +
                "FROM lims_sys.aliquot ac INNER JOIN lims_sys.aliquot_user acu ON ac.aliquot_id = acu.aliquot_id  " +
                "inner join lims_sys.aliquot_formulation p on p.child_aliquot_id = ac.aliquot_id " +
                "WHERE p.parent_aliquot_id in ({0})", blocks);

            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return slides;
            }
            else
            {
                while (reader.Read())
                {
                    slides.Add(new SlideObj(
                        reader["SLIDE_ID"].ToString(),
                        reader["SLIDE_NAME"].ToString(),
                        reader["SLIDE_STATUS"].ToString(),
                        reader["SLIDE_LOCATION"].ToString(),
                         reader["U_Aliquot_Station"].ToString(),
                         reader["u_color_type"].ToString(),

                        //לבדוק חוקיות אם סלייד
                        reader["PARENT_ID"].ToString()
                        ));
                }
                return slides;
            }
        }


        private void HandleVirtual(SdgObj ed, string barcode)
        {


            if (virtualLogic == null)
                virtualLogic = new Logic.VirtualLogic(_connection);

            virtualLogic.HandleVirtual(ed, barcode);

        }

        public string HandleLongNumber(string theText, Department dep)
        {

            if (theText.Length > 11)
            {

                if (dep == Department.Pap)
                {
                    theText = "P" + theText.Substring(1, 6) + "/" + theText.Substring(7, 2) + "." + theText.Substring(10, 1);// ".1";
                }

                else if (dep == Department.Cyto)

                {
                    theText = "C" + theText.Substring(1, 6) + "/" + theText.Substring(7, 2) + "." + theText.Substring(10, 1);// ".1";
                                                                                                                             //    00158242002141
                                                                                                                             //    00158242001226
                }

            }
            return theText;
        }

        #endregion

        #region Box

        public BoxObj GetBoxByName(string boxName)
        {


            var sql = string.Format("select * from LIMS_SYS.u_box b " +
                          "inner join LIMS_SYS.u_box_user bu on b.u_box_id = bu.u_box_id " +
                          "where   b.name = '{0}'", boxName);


            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return null;
            }
            else
            {
                BoxObj BoxObj = null;
                while (reader.Read())
                {




                    BoxObj = new BoxObj(
                       reader["u_box_id"].ToString(),
                       reader["name"].ToString(),
                       reader["u_status"].ToString(),
                       reader["u_from_lab"].ToString(),
                       reader["u_to_lab"].ToString()
                       );

                }
                return BoxObj;
            }
        }

        public List<BoxObj> GetBoxes(string from_lab, string to_lab)
        {
            sql = string.Format("select b.u_box_id,b.name,bu.u_status,bu.u_from_lab,bu.u_to_lab from lims_sys.u_box b " +
                "inner join lims_sys.u_box_user bu on b.u_box_id = bu.u_box_id " +
                "where bu.u_status in('{0}','{1}') and bu.u_from_lab = '{2}' and bu.u_to_lab = '{3}'", BoxStatus.NEW, BoxStatus.PACKED, from_lab, to_lab);

            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return null;
            }
            else
            {
                List<BoxObj> BoxObj_list = new List<BoxObj>();
                while (reader.Read())
                {
                    BoxObj BoxObj = new BoxObj(
                        reader["u_box_id"].ToString(),
                        reader["name"].ToString(),
                        reader["u_status"].ToString(),
                        reader["u_from_lab"].ToString(),
                        reader["u_to_lab"].ToString()
                        );
                    BoxObj_list.Add(BoxObj);
                }
                return BoxObj_list;
            }
        }

        public BoxObj AddBox(string from_lab, string to_lab, double user)
        {
            string sign = "A";
            if (from_lab == "98")
            {
                sign = "D";
            }
            else if (from_lab == "991")
            {
                sign = "Z";

            }



            OracleTransaction transaction = null;
            try
            {
                sql = "select lims.sq_U_BOX.nextval from dual";

                transaction = _connection.BeginTransaction();
                cmd = new OracleCommand(sql, _connection, transaction);

                var U_BOX_ID = cmd.ExecuteScalar();
                var newName = DateTime.Now.Year + "-" + sign + "-" + U_BOX_ID;

                sql = string.Format("Insert into lims_sys.U_BOX (U_BOX_ID,NAME,DESCRIPTION,VERSION,VERSION_STATUS,GROUP_ID,PARENT_VERSION_ID) values ('{0}','{1}',null,'1','A',null,null)", U_BOX_ID, newName);
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                sql = string.Format("Insert into lims_sys.U_BOX_USER (U_BOX_ID,U_FROM_LAB,U_TO_LAB,U_STATUS,U_CREATED_ON,U_CREATED_BY) values ('{0}','{1}','{2}','{5}',{3},'{4}')", U_BOX_ID, from_lab, to_lab, "SYSDATE", user != 0 ? user.ToString() : null, BoxStatus.NEW);
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();

                transaction.Commit();

                BoxObj box = new BoxObj(
                    U_BOX_ID.ToString(),
                    newName,
                   BoxStatus.NEW,
                    from_lab,
                    to_lab
                    );

                return box;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception en)
                {
                    MessageBox.Show(en.Message); ;
                }
                return null;
            }

        }

        #endregion
        public void InsertTo_LabTrack(BoxObj box, string statusLabTrack, EntityDetails ed)
        {

            OracleTransaction transaction = null;
            try
            {
                sql = "select lims.sq_U_LAB_TRACK.nextval from dual";
                transaction = _connection.BeginTransaction();
                cmd = new OracleCommand(sql, _connection, transaction);

                //Get id
                var U_LAB_TRACK_ID = cmd.ExecuteScalar();

                sql = string.Format("Insert into lims_sys.U_LAB_TRACK (U_LAB_TRACK_ID,NAME,DESCRIPTION,VERSION,VERSION_STATUS,GROUP_ID,PARENT_VERSION_ID) values ('{0}','{0}',null,'1','A',null,null)",
                    U_LAB_TRACK_ID, U_LAB_TRACK_ID);
                cmd.CommandText = sql;
                //Insert U_LAB_TRACK
                cmd.ExecuteNonQuery();

                sql = string.Format("Insert into lims_sys.U_LAB_TRACK_USER (U_LAB_TRACK_ID,U_PACKED_ON,U_TRACK_TABLE_NAME,U_TRACK_ITEM_ID,U_BOX,U_STATUS,U_TRACK_ITEM_NAME,U_VIRTUAL_OF) " +
                    "values ('{0}',{1},'{2}','{3}','{4}','{5}','{6}','{7}')"
                    , U_LAB_TRACK_ID, "SYSDATE", ed.Table_Name, ed.Id, box.id, statusLabTrack, ed.Name, ed.Virtual_of);
                cmd.CommandText = sql;

                //Insert U_LAB_TRACK_USER
                cmd.ExecuteNonQuery();


                sql = string.Format("update lims_sys.U_BOX_USER set U_STATUS = '{0}' where U_BOX_ID = '{1}'", BoxStatus.PACKED, box.id);
                cmd.CommandText = sql;

                //Update box status
                cmd.ExecuteNonQuery();

                transaction.Commit();

                try
                {
                    Add2Log(ed);
                }
                catch (Exception)
                {

                    MessageBox.Show("Error on adding log");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception en)
                {
                    MessageBox.Show(en.Message); ;
                }
            }
        }
        public TrackItem GetTrackItemByBox(string boxName, string trackName)
        {

            List<TrackItem> tempTrackItem = new List<TrackItem>();

            var sql = string.Format("select * from LIMS_SYS.U_LAB_TRACK_USER where  u_box = '{0}'  and (U_TRACK_ITEM_NAME = '{1}' or U_Virtual_Of ='{1}') ", boxName, trackName);


            cmd = new OracleCommand(sql, _connection);
            OracleDataReader reader = cmd.ExecuteReader();

            //Checks if it exists
            if (!reader.HasRows)
            {
                return null;
            }
            else
            {

                while (reader.Read())
                {




                    tempTrackItem.Add(new TrackItem()
                    {
                        U_LAB_TRACK_ID = nte(reader["U_LAB_TRACK_ID"]),
                        U_TRACK_TABLE_NAME = nte(reader["U_TRACK_TABLE_NAME"]),
                        U_TRACK_ITEM_NAME = nte(reader["U_TRACK_ITEM_NAME"]),
                        U_TRACK_ITEM_ID = nte(reader["U_TRACK_ITEM_ID"]),
                        U_BOX = nte(nte(reader["U_BOX"])),
                        U_STATUS = nte(reader["U_STATUS"]),
                        U_OLD_STATUS = nte(reader["U_OLD_STATUS"]),
                        U_PACKED_ON = nte(reader["U_PACKED_ON"]),
                        U_ARRIVED_ON = nte(reader["U_ARRIVED_ON"]),
                        U_VIRTUAL_OF = nte(reader["U_VIRTUAL_OF"])
                    });

                }
                TrackItem mainTrack = tempTrackItem.Where(x => string.IsNullOrEmpty(x.U_VIRTUAL_OF)).FirstOrDefault();
                if (mainTrack != null)
                {
                    mainTrack.AddTrackVirtual(tempTrackItem.Where(x => string.IsNullOrEmpty(x.U_VIRTUAL_OF) == false).ToList());
                }

                return mainTrack;
            }
        }
        public bool ExistsInOtherBox(EntityDetails ed, string box)
        {
            sql = string.Format("Select 1 From LIMS_SYS.U_Lab_Track_User where U_Track_Item_Id='{0}' and U_Track_Table_Name='{1}'  and  lims.phrase_lookup_INFO('Lab Location', U_STATUS)='X' "
        , ed.Id, ed.Table_Name, box);

            cmd = new OracleCommand(sql, _connection);
            var res = cmd.ExecuteOracleScalar();
            return res != null;

        }

        public bool UpdateBoxStatus(string boxId, string statusToUpdate, double user, bool checkEntityExist)//send
        {

            try
            {
                string sqlUpdateStatus = "update LIMS_SYS.U_BOX_USER set U_STATUS = '{0}'";

                string sqlWhere = "where U_BOX_ID = '{2}'";

                if (checkEntityExist)
                {
                    //get all entities
                    sql = string.Format("select U_TRACK_TABLE_NAME,U_TRACK_ITEM_ID from LIMS_SYS.U_LAB_TRACK_USER where U_BOX =  '{0}'", boxId);

                    cmd = new OracleCommand(sql, _connection);
                    OracleDataReader reader = cmd.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        MessageBox.Show("לא נמצאו ישויות בארגז זה", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                    sql = sqlUpdateStatus + ",U_SEND_ON = {1} " + sqlWhere + " and U_SEND_ON is null";
                }
                else
                {
                    sql = sqlUpdateStatus + ",U_ARRIVED_ON = {1},U_ARRIVED_BY = {3} " + sqlWhere + " and U_ARRIVED_ON is null"; ;
                }




                sql = string.Format(sql, statusToUpdate, "SYSDATE", boxId, user != 0 ? user.ToString() : null);
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();



                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {

                }
                catch (Exception en)
                {
                    MessageBox.Show(en.Message); ;
                }
                return false;
            }

        }

        public bool UpdateEntityStatus(TrackItem titem, string toLab)
        {


            OracleTransaction transaction = null;

            try
            {
                List<TrackItem> tempTrackItem = BuildList(titem);

                transaction = _connection.BeginTransaction();



                foreach (var row in tempTrackItem)
                {



                    //update entity
                    sql = string.Format("update " + "LIMS_SYS." + row.U_TRACK_TABLE_NAME + "_USER set U_LAB_LOCATION = '{0}' where " + row.U_TRACK_TABLE_NAME + "_ID = '{1}'", toLab, row.U_TRACK_ITEM_ID);
                    cmd = new OracleCommand(sql, _connection, transaction);
                    cmd.ExecuteNonQuery();

                    //update labTracking
                    sql = string.Format("update LIMS_SYS.U_LAB_TRACK_USER set U_STATUS = '{0}', U_ARRIVED_ON = {1} where U_BOX = '{2}' and U_TRACK_ITEM_ID = '{3}'", toLab, "SYSDATE", row.U_BOX, row.U_TRACK_ITEM_ID);
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();



                }


                transaction.Commit();

                //Don't Take it to above foreach
                foreach (var row in tempTrackItem)
                {
                    Add2Log(row, " to " + GetLabLocation(toLab), Constants._appLogParam_rcv.Trim());
                }


                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                try
                {
                    transaction.Rollback();
                }
                catch (Exception en)
                {
                    MessageBox.Show(en.Message); ;
                }
                return false;
            }

        }

        private List<TrackItem> BuildList(TrackItem titem)
        {
            var tempTrackItem = new List<TrackItem>();
            tempTrackItem.Add(titem);
            foreach (var item in titem.TrackItemVirtuals)
            {
                tempTrackItem.Add(item);
            }

            return tempTrackItem;
        }

        public void UpdateEntitiesBox(string boxId, string status, string _fromLabName)
        {
            sql = string.Format("update LIMS_SYS.U_LAB_TRACK_USER set U_STATUS = '{0}' where  u_box={1}", status, boxId);
            cmd = new OracleCommand(sql, _connection);
            cmd.ExecuteNonQuery();

            //TODO:add 2 sdg log
            sql = string.Format("select * from LIMS_SYS.U_LAB_TRACK_USER  where  u_box={0}", boxId);
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();

            List<TrackItem> tempTrackItem = new List<TrackItem>();
            while (reader.Read())
            {




                tempTrackItem.Add(new TrackItem()
                {
                    U_LAB_TRACK_ID = nte(reader["U_LAB_TRACK_ID"]),
                    U_TRACK_TABLE_NAME = nte(reader["U_TRACK_TABLE_NAME"]),
                    U_TRACK_ITEM_NAME = nte(reader["U_TRACK_ITEM_NAME"]),
                    U_TRACK_ITEM_ID = nte(reader["U_TRACK_ITEM_ID"]),
                    U_BOX = nte(nte(reader["U_BOX"])),
                    U_STATUS = nte(reader["U_STATUS"]),
                    U_OLD_STATUS = nte(reader["U_OLD_STATUS"]),
                    U_PACKED_ON = nte(reader["U_PACKED_ON"]),
                    U_ARRIVED_ON = nte(reader["U_ARRIVED_ON"]),
                    U_VIRTUAL_OF = nte(reader["U_VIRTUAL_OF"])
                });

            }

            foreach (var item in tempTrackItem)
            {
                Add2Log(item, " From " + _fromLabName, Constants._appLogParam_snd.Trim());
            }
        }

        #region Sdg log
        Logic.Log_logic sdg_log_logic;
        private void Add2Log(TrackItem titem, string desc, string appCode)
        {


            if (sdg_log_logic == null)
            {
                sdg_log_logic = new Logic.Log_logic(_connection, SessionId, _diclabNames);
            }

            sdg_log_logic.Add2Log(titem, desc, appCode);


        }
        private void Add2Log(EntityDetails ed)
        {
            if (sdg_log_logic == null)
            {
                sdg_log_logic = new Logic.Log_logic(_connection, SessionId, _diclabNames);
            }

            sdg_log_logic.Add2Log(ed);
        }




        #endregion
        private string nte(object v)
        {
            if (v == null)
            {
                return "";
            }
            else
            {
                return v.ToString();
            }
        }

    }
}
