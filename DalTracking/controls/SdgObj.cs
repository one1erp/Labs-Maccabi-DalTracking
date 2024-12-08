using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalTracking
{
    public class SdgObj : EntityDetails
    {

        public List<SampleObj> samples { get; set; }

        public SdgObj(string sdg_id, string sdg_name,string status,string labLocation,string entity, List<SampleObj> samples)
        {
            this.id = sdg_id;
            this.name = sdg_name;
            this.samples = samples;
            this.status = status;
            this.labLocation = labLocation;
            this.entity = entity;
        }
   
    
    }

}
