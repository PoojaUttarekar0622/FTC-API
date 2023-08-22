using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Model
{
    public class MESPASEventClassDeclarations
    {
        public class Enquirydata
        {
            public string sourceType { get; set; }
            public string owner { get; set; }

            public string eventType { get; set; }
            public string entityId { get; set; }

            public long eventId { get; set; }
            public string status { get; set; }
        }

        public class MSDItemData
        {
            public int msdItem_Id { get; set; }
            public string itemName { get; set; }
        }
        public class SNQItemData
        {
            public int snqItem_Id { get; set; }
            public string itemName { get; set; }
        }

        public class MakerData
        {
            public int maker_Id { get; set; }
            public string makerName { get; set; }

            public string makerCode { get; set; }

        }

        public class EquipmentData
        {
            public int equipment_Id { get; set; }
            public string equipmentName { get; set; }
            public string equipmentCode { get; set; }
        }
        public class Message
        {
            public string result { get; set; }
        }
    }
}
