using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalTracking
{
    public class SampleObj : EntityDetails
    {
        public List<BlockObj> aliquots { get; set; }

        public SampleObj(string id, string sample_name, string status, string labLocation, string entity, List<BlockObj> aliquots)
        {
            this.id = id;
            this.name = sample_name;
            this.aliquots = aliquots;
            this.status = status;
            this.labLocation = labLocation;
            this.entity = entity;
        }
    }
}
