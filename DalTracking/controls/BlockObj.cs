using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalTracking
{
    public class BlockObj : EntityDetails
    {
        public List<SlideObj> slides { get; set; }
        public string parent { get; set; }
        public BlockObj(string id, string block_name, string status, string labLocation, string entity,string parent, List<SlideObj> slides)
        {
            this.id = id;
            this.name = block_name;
            this.status = status;
            this.labLocation = labLocation;
            this.slides = slides;
            this.entity = entity;
            this.parent = parent;
        }
    }
}
