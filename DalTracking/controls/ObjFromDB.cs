using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class ObjFromDB
    {
        public string SDG_ID { get; set; }
        public string SAMPLE_ID { get; set; }
        public string BLOCK_ID { get; set; }
        public string SLIDE_ID { get; set; }
        public string SDG_NAME { get; set; }
        public string SAMPLE_NAME { get; set; }
        public string BLOCK_NAME { get; set; }
        public string SLIDE_NAME { get; set; }
        public string SDG_STATUS { get; set; }
        public string SAMPLE_STATUS { get; set; }
        public string BLOCK_STATUS { get; set; }
        public string SLIDE_STATUS { get; set; }
        public string BLOCK_GLASS_TYPE { get; set; }
        public string SLIDE_GLASS_TYPE { get; set; }
        public string U_LAST_UPDATE { get; set; }
        public string SDG_LOCATION { get; set; }
        public string SAMPL_LOCATION { get; set; }
        public string BLOCK_LOCATION { get; set; }
        public string SLIDE_LOCATION { get; set; }


        public ObjFromDB(string SDG_ID,string SAMPLE_ID ,string BLOCK_ID ,string SLIDE_ID, string SDGNAME, string SAMPLENAME, string BLOCKNAME, string SLIDENAME, string SDGSTATUS, string SAMPLESTATUS, string BLOCKSTATUS, string SLIDESTATUS, string BLOCKGLASSTYPE, string SLIDEGLASSTYPE, string U_LAST_UPDATE, string SDGLOCATION, string SAMPLLOCATION, string BLOCKLOCATION, string SLIDELOCATION)
        {
            this.SDG_ID = SDG_ID;
            this.SAMPLE_ID = SAMPLE_ID;
            this.BLOCK_ID = BLOCK_ID;
            this.SLIDE_ID = SLIDE_ID;
            this.SDG_NAME = SDGNAME;
            this.SAMPLE_NAME = SAMPLENAME;
            this.BLOCK_NAME = BLOCKNAME;
            this.SLIDE_NAME = SLIDENAME;
            this.SDG_STATUS = SDGSTATUS;
            this.SAMPLE_STATUS = SAMPLESTATUS;
            this.BLOCK_STATUS = BLOCKSTATUS;
            this.SLIDE_STATUS = SLIDESTATUS;
            this.BLOCK_GLASS_TYPE = BLOCKGLASSTYPE;
            this.SLIDE_GLASS_TYPE = SLIDEGLASSTYPE;
            this.U_LAST_UPDATE = U_LAST_UPDATE;
            this.SDG_LOCATION = SDGLOCATION;
            this.SAMPL_LOCATION = SAMPLLOCATION;
            this.BLOCK_LOCATION = BLOCKLOCATION;
            this.SLIDE_LOCATION = SLIDELOCATION;
        }
    }
}
