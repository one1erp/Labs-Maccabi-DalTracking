using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DalTracking
{
    public class SampleObj : EntityDetails//, IChildObj
    {
        public List<BlockObj> Blocks { get; set; }

        public SampleObj(string id, string sample_name, string status, string labLocation)
            : base(sample_name)
        {
            this.Id = id;
            this.Name = sample_name;
            this.Status = status;
            this.LabLocation = labLocation;
            Table_Name = "SAMPLE";
            table_Name_const = DalTracking.Table_Name.SAMPLE;
            Blocks = new List<BlockObj>();


        }




    }
}
