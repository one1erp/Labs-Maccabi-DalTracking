using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Linq;
using System.Text;

namespace DalTracking.Logic
{
    internal class VirtualLogic
    {
        private OracleConnection connection;
        private EntityDetails ed;

        public VirtualLogic(OracleConnection connection)
        {
            this.connection = connection;
        }


        internal void HandleVirtual(SdgObj sdg, string barcode)
        {
             ed = sdg.GetObjByName(barcode);

            if (ed == null || ed.table_Name_const == Table_Name.None) return;




            //If user scan sample ,the sdg will selected too
            if (ed.table_Name_const == Table_Name.SAMPLE)
            {
                sdg.Virtual_of = barcode;
            }
            //-------------------------------------------


            if (ed.GetDep() == Department.His)
            {
                CheckBiopsyVirtual();
            }

            else if (ed.GetDep() == Department.Pap)
            {

                CheckPapVirtual();
            }
            else if (ed.GetDep() == Department.Cyto)
            {

                CheckCytoVirtual(sdg);
            }
            //
            //sdg NO STINE        
            //SELECT* FROM SDG 
            //substr(name,1,1)='C' and status in ('V','P')
            //and not exists (select 1 from lims_sys.sdg d,
            //lims_sys.sample s, lims_sys.aliquot a, lims_sys.test t,
            //lims_sys.result r where d.sdg_id = sdg.sdg_id and s.sdg_id=d.sdg_id
            //and a.sample_id=s.sample_id and t.aliquot_id=a.aliquot_id
            //and r.test_id=t.test_id and r.name='Color' and r.status='C' )
        }





        private void CheckPapVirtual()
        {

            if (ed.table_Name_const == Table_Name.SAMPLE)
            {

                SampleObj smpl = ed as SampleObj;

                if (smpl.Blocks.Count > 1)
                {
                    System.Windows.Forms.MessageBox.Show("לבדוק למה יש יותר מסלייד 1 ONE1");
                }
                else
                {
                    BlockObj block = smpl.Blocks.FirstOrDefault();
                    if (block == null) return;
                    else
                    {
                        //לאחר מסך קבלה לפני קובאס
                        //Before Cobas = Status is "V" 
                        //Before Stain =  AliquotStation > 9 

                        if (block.Status == "V" || block.AliquotStation > 9 || block.AliquotStation < 1)
                        {
                            block.Virtual_of = smpl.Name;
                        }
                        //else
                        //{
                        //    //אם קיימת תוצאת LBC 
                        //    var cmd = new OracleCommand(hasLBC_Test_sql, connection);
                        //    var res = cmd.ExecuteScalar();
                        //    if (res == null)
                        //    {
                        //        block.Virtual_of = smpl.Name;
                        //    }
                        //    else
                        //    {
                        //        if (B4Stain())
                        //        {
                        //            block.Virtual_of = smpl.Name;
                        //        }
                        //    }
                        //}
                    }

                    //  string hasLBC_Test_sql = string.Format("SELECT 1 FROM lims_sys.test LBC_Test WHERE LBC_Test.name = 'Clinical Features' AND LBC_Test.status <> 'X' and Lbc_Test.Aliquot_Id = {0}", block.Id);

                    //string cobasFinalResult_sql = string.Format("SELECT Finalresult.Original_Result  FROM lims_sys.test Cobas_Test, lims_sys.result finalResult " +
                    //      "WHERE  Finalresult.Test_Id = Cobas_Test.Test_Id  and Cobas_Test.name = 'Cobas HPV'  and Finalresult.name = 'HPV Final' " +
                    //      " AND Cobas_Test.status <> 'X'  and Cobas_Test.Aliquot_Id =  {0}", block.Id);






                }
            }
        }
        private void CheckCytoVirtual(SdgObj sdg)
        {


            //Cell block
            if (ed.table_Name_const == Table_Name.ALIQUOT && ed.EntityType == "Block")
            {
                BlockObj block = ed as BlockObj;
                if (block.CellBlock == "T")
                {
                    foreach (var slide in block.slides)
                    {
                        if (("VPC".Contains(slide.Status) && slide.GetDep() == Department.Cyto
                           && slide.AliquotStation < 5) || slide.Status == "X")//אם סלייד מבוטל הוא גם יהיה וירטואלי 24/1/24)

                        {
                            slide.Virtual_of = block.Name;
                        }

                    }
                }
            }
            else if (ed.table_Name_const == Table_Name.SAMPLE)
            {
                SampleObj smp = ed as SampleObj;

                foreach (var block in smp.Blocks)
                {
                    if (block.Glass_Type == "L")
                    {
                        string sql = "     select 1 from lims_sys.sdg d,lims_sys.sdg_log dl " +
                                    "  where dl.sdg_id = d.sdg_id" +
                                    "   and application_code = 'RM.SOK'" +
                                      " and d.sdg_id = " + sdg.Id;
                        var cmd = new OracleCommand(sql, connection);
                        var obj = cmd.ExecuteScalar();
                        if (obj == null)
                        {

                            block.Virtual_of = smp.Name;

                        }
                    }
                    else if (block.CellBlock == "T")
                    {

                    }

                }
            }

            //Nozel -Slides are not virtual Always

        }
        private void CheckBiopsyVirtual()
        {
            if (ed.table_Name_const == Table_Name.SAMPLE)
            {
                SampleObj smpl = ed as SampleObj;
                foreach (var blockItem in smpl.Blocks)
                {
                    if ((smpl.Status == "V" && blockItem.Status == "V")
                        || (blockItem.Status == "X"))//אם קסטה מבוטלת היא גם תהיה וירטואלית 24/1/24
                    {
                        blockItem.Virtual_of = smpl.Name;
                    }

                }
            }

            if (ed.table_Name_const == Table_Name.ALIQUOT && ed.EntityType == "Block")
            {
                BlockObj block = ed as BlockObj;

                foreach (var slide in block.slides)
                {
                    if (("VPC".Contains(slide.Status) && slide.GetDep() == Department.His
                       && slide.AliquotStation < 5) || slide.Status == "X")//אם סלייד מבוטל הוא גם יהיה וירטואלי 24/1/24

                    {
                        slide.Virtual_of = block.Name;
                    }



                }
            }
        }



    }
}