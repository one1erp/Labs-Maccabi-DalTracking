using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalTracking
{
    public class BoxStatus
    {
        public const string NEW = "N";
        public const string PACKED = "P";
        public const string SENT = "S";
        public const string ARRIVED = "A";

    }
    public enum Department
    {
     None   ,His, Cyto, Pap, 

    }
    public enum Table_Name
    {
        SDG, SAMPLE, ALIQUOT,
        None
    }
   
    public class Constants
    {
        public static string _appLogParam_req = "TRK.REQ";
        public static string _appLogParam_rcv = "TRK.RCV";
        public static string _appLogParam_snd = "TRK.SND";


        
    }
}
