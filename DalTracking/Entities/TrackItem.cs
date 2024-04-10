using System.Collections.Generic;

namespace DalTracking
{
    public class TrackItem
    {


        public string U_LAB_TRACK_ID { get; set; }
        public string U_TRACK_TABLE_NAME { get; set; }
        public string U_TRACK_ITEM_ID { get; set; }
        public string U_BOX { get; set; }
        public string U_STATUS { get; set; }
        public string U_OLD_STATUS { get; set; }
        public string U_PACKED_ON { get; set; }
        public string U_ARRIVED_ON { get; set; }
        public string U_TRACK_ITEM_NAME { get; set; }
        public string U_VIRTUAL_OF { get; internal set; }

        public List<TrackItem> TrackItemVirtuals { get; private set; }
        public TrackItem()
        {
            TrackItemVirtuals = new List<TrackItem>();
        }
        public void AddTrackVirtual(List<TrackItem> _trackItemVirtuals
            )
        {
            TrackItemVirtuals.AddRange(_trackItemVirtuals);
        }

    }


}
