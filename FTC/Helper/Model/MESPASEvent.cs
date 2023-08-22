using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using static Helper.Data.DataContextClass;
using static Helper.Model.MESPASEventClassDeclarations;
using static Helper.Model.MSDClassDeclarations;
namespace Helper.Model
{
    public class MESPASEvent : IMESPASEvent
    {
        #region created object of connection and configuration class
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        #endregion
        #region  created controller of that class
        public MESPASEvent(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        #endregion
        #region get latest event id
        public long GetlatestEventId()
        {
            long eventId = 0;
            eventId = _datacontext.M_EVENTTable.Where(o => o.PK_EVENTID == 1).Select(x => x.EVENT_ID).FirstOrDefault();
            return eventId;
        }
        #endregion
        #region
        public string GetCustomerMapping(string owner)
        {
            #region customer mapping
            string mappedCustomer = "";
            if (!string.IsNullOrEmpty(owner))
            {
                #region
                var findCustomer = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                    where hdr.TEMPLATE_CUSTOMER_NAME.ToUpper() == owner.ToUpper()
                                    select hdr.TEMPLATE_CUSTOMER_NAME).ToList();
                if (findCustomer.Count > 0)
                {
                    mappedCustomer = findCustomer[0];
                }
                #endregion
            }
            return mappedCustomer;
            #endregion customer mapping
        }
        #endregion
        #region update latest event id
        public MessageMSD UpdateEventId(long eventId)
        {
            #region
            Enquirydetailsdata EnqDetails = new Enquirydetailsdata();
            #endregion
            #region created of object of return result
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while updating event Id";
            #endregion
            try
            {
                #region
                M_EVENT objhdr = (from hdr in _datacontext.M_EVENTTable where hdr.ISACTIVE == 1 select hdr).FirstOrDefault();
                if (objhdr != null)
                {
                    objhdr.EVENT_ID = eventId;
                    objhdr.CREATED_AT = DateTime.Now;
                    _datacontext.SaveChanges();
                }
                obj.result = "latest Event Id updated successfully";
                #endregion
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        #endregion
        #region get items list of msd dept
        public List<MSDItemData> GetMSDItemsList()
        {
            List<MSDItemData> lstMSDItemData = new List<MSDItemData>();
            try
            {
                #region select msd items
                var MSDItemData = (from p in _datacontext.M_MSD_ITEMSTable
                                   where p.IS_ACTIVE == 1
                                   select new
                                   {
                                       p.PK_ITEM_ID,
                                       p.ITEM_NAME
                                   }).ToList();
                #endregion
                #region
                if (MSDItemData != null)
                {
                    foreach (var data in MSDItemData)
                    {
                        MSDItemData objdata = new MSDItemData();
                        objdata.msdItem_Id = data.PK_ITEM_ID;
                        objdata.itemName = data.ITEM_NAME;
                        lstMSDItemData.Add(objdata);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            #endregion
            return lstMSDItemData;
        }
        #endregion
        #region delete msd items from portal
        public Message DeleteMSDItemsData(List<MSDItemData> objData)
        {
            List<MSDItemData> lstMSDItemData = new List<MSDItemData>();
            #region created of object of return result
            Message obj = new Message();
            obj.result = "Error while Deleting Enquiry";
            #endregion
            #region
            foreach (var item in objData)
            {
                try
                {
                    M_MSD_ITEMS objDtl = (from dtl in _datacontext.M_MSD_ITEMSTable
                                          where dtl.PK_ITEM_ID == item.msdItem_Id
                                          select dtl).ToList().SingleOrDefault();
                    if (objDtl != null)
                    {
                        _datacontext.M_MSD_ITEMSTable.Remove(objDtl);
                        _datacontext.SaveChanges();
                    }
                    obj.result = "Data deleted successfully";
                }
                catch (Exception ex)
                {
                    obj.result = ex.Message;
                }
            }
            #endregion
            return obj;
        }
        #endregion
        #region save msd items 
        public Message SaveMSDItemsData(List<MSDItemData> objItemsData)
        {
            #region created of object of return result
            Message obj = new Message();
            obj.result = "Error while Creating Enquiry";
            #endregion
            if (objItemsData != null)
            {
                foreach (var item in objItemsData)
                {
                    #region check item is duplicate
                    var DuplicateEntry = (from hdrdata in _datacontext.M_MSD_ITEMSTable
                                          where hdrdata.ITEM_NAME.ToUpper() == item.itemName.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_ITEM_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        if (item.msdItem_Id == 0)
                        {
                            M_MSD_ITEMS objData = new M_MSD_ITEMS();
                            objData.ITEM_NAME = item.itemName;
                            objData.IS_ACTIVE = 1;
                            objData.CREATED_AT = DateTime.Now;
                            _datacontext.M_MSD_ITEMSTable.Add(objData);
                            _datacontext.SaveChanges();
                            obj.result = "Data Saved successfully";
                        }
                        else
                        {
                            M_MSD_ITEMS objData = (from hdr in _datacontext.M_MSD_ITEMSTable where hdr.PK_ITEM_ID == item.msdItem_Id select hdr).FirstOrDefault();
                            objData.ITEM_NAME = item.itemName;
                            objData.IS_ACTIVE = 1;
                            objData.CREATED_AT = DateTime.Now;
                            _datacontext.SaveChanges();
                            obj.result = "Data Updated Successfully";
                        }
                    }
                    else
                    {
                        obj.result = "Same entry already exist";
                    }
                    #endregion
                }
            }
            return obj;
        }
        #endregion
        #region get items list of msd dept
        public List<SNQItemData> GetSNQItemsList()
        {
            List<SNQItemData> lstSNQItemData = new List<SNQItemData>();
            try
            {
                #region select snq items
                var SNQItemData = (from p in _datacontext.M_SNQ_ITEMSTable
                                   where p.IS_ACTIVE == 1
                                   select new
                                   {
                                       p.PK_ITEM_ID,
                                       p.ITEM_NAME
                                   }).ToList();
                #endregion
                if (SNQItemData != null)
                {
                    #region
                    foreach (var data in SNQItemData)
                    {
                        SNQItemData objdata = new SNQItemData();
                        objdata.snqItem_Id = data.PK_ITEM_ID;
                        objdata.itemName = data.ITEM_NAME;
                        lstSNQItemData.Add(objdata);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
            }
            return lstSNQItemData;
        }
        #endregion
        #region
        public Message DeleteSNQItemsData(List<SNQItemData> objSNQItemData)
        {
            List<SNQItemData> lstSNQItemData = new List<SNQItemData>();
            Message obj = new Message();
            obj.result = "Error while Deleting Enquiry";
            foreach (var item in objSNQItemData)
            {
                try
                {
                    M_SNQ_ITEMS objDtl = (from dtl in _datacontext.M_SNQ_ITEMSTable
                                          where dtl.PK_ITEM_ID == item.snqItem_Id
                                          select dtl).ToList().SingleOrDefault();
                    if (objDtl != null)
                    {
                        _datacontext.M_SNQ_ITEMSTable.Remove(objDtl);
                        _datacontext.SaveChanges();
                    }
                    obj.result = "Data deleted successfully";
                }
                catch (Exception ex)
                {
                    obj.result = ex.Message;
                }
            }
            return obj;
        }
        #endregion
        #region
        public Message SaveSNQItemsData(List<SNQItemData> objSNQItemData)
        {
            #region created of object of return result
            Message obj = new Message();
            obj.result = "Error while Creating Enquiry";
            #endregion
            if (objSNQItemData != null)
            {
                foreach (var item in objSNQItemData)
                {
                    #region check item is duplicate
                    var DuplicateEntry = (from hdrdata in _datacontext.M_SNQ_ITEMSTable
                                          where hdrdata.ITEM_NAME.ToUpper() == item.itemName.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_ITEM_ID
                                          }).ToList();
                    #endregion
                    if (DuplicateEntry.Count == 0)
                    {
                        #region
                        if (item.snqItem_Id == 0)
                        {
                            M_SNQ_ITEMS objData = new M_SNQ_ITEMS();
                            objData.ITEM_NAME = item.itemName;
                            objData.IS_ACTIVE = 1;
                            objData.CREATED_AT = DateTime.Now;
                            _datacontext.M_SNQ_ITEMSTable.Add(objData);
                            _datacontext.SaveChanges();
                            obj.result = "Data Saved successfully";
                        }
                        else
                        {
                            M_SNQ_ITEMS objData = (from hdr in _datacontext.M_SNQ_ITEMSTable where hdr.PK_ITEM_ID == item.snqItem_Id select hdr).FirstOrDefault();
                            objData.ITEM_NAME = item.itemName;
                            objData.IS_ACTIVE = 1;
                            objData.CREATED_AT = DateTime.Now;
                            _datacontext.SaveChanges();
                            obj.result = "Data Updated Successfully";
                        }
                        #endregion
                    }
                    else
                    {
                        obj.result = "Same entry already exist";
                    }
                }
            }
            return obj;
        }
        #endregion
        #region get maker list for rfq checking
        public List<MakerData> GetMakerList()
        {
            #region created object list
            List<MakerData> lstMakerData = new List<MakerData>();
            #endregion
            try
            {
                #region select maker list
                var makerDataData = (from p in _datacontext.M_MAKERTable
                                     where p.IS_ACTIVE == 1
                                     select new
                                     {
                                         p.PK_MAKER_ID,
                                         p.MAKER_NAME
                                     }).ToList();
                #endregion
                if (makerDataData != null)
                {
                    #region
                    foreach (var data in makerDataData)
                    {
                        MakerData objdata = new MakerData();
                        objdata.maker_Id = data.PK_MAKER_ID;
                        objdata.makerName = data.MAKER_NAME.Trim();
                        lstMakerData.Add(objdata);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
            }
            return lstMakerData;
        }
        #endregion
        #region get equipment list for rfq checking
        public List<EquipmentData> GetEquipmentList()
        {
            #region created object of list
            List<EquipmentData> lstEquipmentData = new List<EquipmentData>();
            #endregion
            try
            {
                #region select equipment list
                var equipmentData = (from p in _datacontext.M_EQUIPMENTTable
                                     where p.IS_ACTIVE == 1
                                     select new
                                     {
                                         p.PK_EQUIPMENT_ID,
                                         p.EQUIPMENT_NAME
                                     }).ToList();
                #endregion
                #region
                if (equipmentData != null)
                {
                    foreach (var data in equipmentData)
                    {
                        EquipmentData objdata = new EquipmentData();
                        objdata.equipment_Id = data.PK_EQUIPMENT_ID;
                        objdata.equipmentName = data.EQUIPMENT_NAME.Trim();
                        lstEquipmentData.Add(objdata);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            return lstEquipmentData;
        }
        #endregion
    }
}
