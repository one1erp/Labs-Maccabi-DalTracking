using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DalTracking
{
    public abstract class EntityDetails
    {


        public string Id { get; set; }
        public string Name { get; set; }
        public string LabLocation { get; set; }
        public string Status { get; set; }
        public virtual string EntityType { get { return this.Table_Name; } }

        public string Table_Name { get; set; }
        public string External_Reference { get; set; }
        //   public string TableName { get;  }


        protected Department Dep;

        public Table_Name table_Name_const = DalTracking.Table_Name.None;
        private string virtual_of;





        public string Virtual_of
        {
            get { return virtual_of; }
            internal set
            {

                virtual_of = value;
                Debug.Print(virtual_of + " is virtual_of " + this.Name);
            }

        }

        public bool IsVirtual
        {
            get { return Virtual_of != null; }

        }
        public SdgObj SdgParent { get; set; }


        public Department GetDep()
        {
            if (Dep == Department.None)
            {
                if (SdgParent != null)
                {
                    return SdgParent.Dep;
                }
                else
                {
                    return Department.None;
                }
            }
            else
            {
                return Dep;
            }

        }

        public EntityDetails(string name)
        {


            string letter = "";
            if (Char.IsLetter(name[0]))
                letter = name[0].ToString();
            else if (Char.IsLetter(name[name.Length - 1]))
            {
                letter = name[name.Length - 1].ToString();
            }


            switch (letter)
            {
                case "B":
                    Dep = Department.His;
                    break;
                case "C":
                    Dep = Department.Cyto;
                    break;
                case "P":
                    Dep = Department.Pap;
                    break;
                default:

                    Dep = Department.None;
                    //   System.Windows.Forms.MessageBox.Show(".ישות לא חוקית", "", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    break;
            }


        }
    }





    //    protected abstract void setEntityName();

}


