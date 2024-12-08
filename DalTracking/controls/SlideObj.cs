using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class SlideObj : EntityDetails
    {
        public string parent { get; set; }
        public SlideObj(string id, string block_name, string status, string labLocation, string entity,string parent)
        {
            this.id = id;
            this.name = block_name;
            this.status = status;
            this.labLocation = labLocation;
            this.entity = entity;
            this.parent = parent;
        }
    }
}
