using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DalTracking
{
    public class BlockObj : EntityDetails//, IChildObj
    {
        public List<SlideObj> slides { get; set; }
        public string CellBlock { get; }
        public string parent { get; set; }
        public int AliquotStation { get; set; }
        public string Glass_Type { get; private set; }

        public BlockObj(string id, string block_name, string status,
            string labLocation, string cellBlock, string aliquotStation,string U_Glass_Type, string parent)
            : base(block_name)
        {
            this.Id = id;
            this.Name = block_name;
            this.Status = status;
            this.LabLocation = labLocation;
            this.slides = slides;
            this.CellBlock = cellBlock;


            int nNumber = int.TryParse(aliquotStation, out nNumber) ? nNumber : -1;
            AliquotStation = nNumber;

            this.Glass_Type = U_Glass_Type;
            this.parent = parent;
            Table_Name = "ALIQUOT";
            table_Name_const = DalTracking.Table_Name.ALIQUOT;
            slides = new List<SlideObj>();

        }
        public override string EntityType => WhichType();

    //    public SdgObj SdgParent { get;  set; }

        private string WhichType()
        {

            if (Dep == Department.Pap) return "Slide";
            if (Dep == Department.His) return "Block";
            else if (Dep == Department.Cyto && CellBlock == "T") return "Block";
            else
            {
                return "Slide";
            }

        }
    }
}
