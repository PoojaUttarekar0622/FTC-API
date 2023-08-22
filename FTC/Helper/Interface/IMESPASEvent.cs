using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.MESPASEventClassDeclarations;


namespace Helper.Interface
{
   public interface IMESPASEvent
    {
        #region
        public long GetlatestEventId();
        public string GetCustomerMapping(string owner);
        public Helper.Model.MSDClassDeclarations.MessageMSD UpdateEventId(long eventId);
     
         public List<MSDItemData> GetMSDItemsList();
        public Message DeleteMSDItemsData(List<MSDItemData> objData);
        public Message SaveMSDItemsData(List<MSDItemData> objItemsData);

        public List<MakerData> GetMakerList();

        public List<EquipmentData> GetEquipmentList();
       
        public List<SNQItemData> GetSNQItemsList();
        public Message SaveSNQItemsData(List<SNQItemData> objSNQItemData);
        public Message DeleteSNQItemsData(List<SNQItemData> objSNQItemData);

        #endregion

    }
}
