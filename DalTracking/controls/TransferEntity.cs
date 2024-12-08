using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class TransferEntity
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Entity { get; set; }
        public string LabLocation { get; set; }
        public string Status { get; set; }


        public TransferEntity(string Name ,string Id ,string Entity,string LabLocation ,string Status)
        {
            this.Name = Name;
            this.Id = Id;
            this.Entity = Entity;
            this.LabLocation = LabLocation;
            this.Status = Status;
        }

    }
}
