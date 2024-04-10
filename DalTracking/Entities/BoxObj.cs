using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class BoxObj
    {

        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string from_lab { get; set; }
        public string to_lab { get; set; }

        public BoxObj(string id, string name, string status, string from_lab, string to_lab)
        {
            this.id = id;
            this.name = name;
            this.status = status;
            this.from_lab = from_lab;
            this.to_lab = to_lab;
        }




    }


}
