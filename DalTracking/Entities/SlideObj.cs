using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class SlideObj : EntityDetails{
        public int AliquotStation { get; private set; }
        public string Color_type { get; private set; }

        public string parent { get; set; }
        public SlideObj(string id, string slide_name,
            string status, string labLocation, string aliquotStation
            , string u_color_type, string parent) : base(slide_name)
        {
            this.Id = id;
            this.Name = slide_name;
            this.Status = status;
            this.LabLocation = labLocation;


            int nNumber = int.TryParse(aliquotStation, out nNumber) ? nNumber : -1;
            AliquotStation = nNumber;





            Color_type = u_color_type;
            //this.entity = entity;
            this.parent = parent;
            Table_Name = "ALIQUOT";
            table_Name_const = DalTracking.Table_Name.ALIQUOT;



        }

        public override string EntityType => "Slide";

    }
}
