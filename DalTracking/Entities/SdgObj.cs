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



        public SdgObj(string sdg_id, string sdg_name, string status, string labLocation, string external_Reference, List<SampleObj> samples)
            : base(sdg_name)
        {
            this.Id = sdg_id;
            this.Name = sdg_name;
            this.samples = samples;
            this.Status = status;
            this.LabLocation = labLocation;
            this.External_Reference = external_Reference;
            Table_Name = "SDG";
            table_Name_const = DalTracking.Table_Name.SDG;
            this.SdgParent = this;
            //  SetDep(sdg_name);


        }

        public EntityDetails GetObjByName(string name)
        {
            if (this.Name == name)
            {
                return this;
            }
            else
            {
                foreach (var sample in samples)
                {
                    if (sample.Name == name)
                    {
                        return sample;
                    }
                    foreach (var aliq in sample.Blocks)
                    {
                        if (aliq.Name == name)
                        {
                            return aliq;
                        }
                        foreach (var slide in aliq.slides)
                        {
                            if (slide.Name == name)
                            {
                                return slide;
                            }
                        }
                    }
                }
            }
            return null;
        }



    }

}
