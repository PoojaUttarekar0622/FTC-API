using Helper.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.ChangeCustomerSettingsClsDeclarations;
using static Helper.Model.SourceTypeClassDeclarations;
using System.Linq;
using Helper.Interface;
using static Helper.Data.DataContextClass;
using static Helper.Model.MLClassDeclarations;

namespace Helper.Model
{
    public class ChangeCustomerSettings : IChangeCustomerSettings
    {
        #region
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        #endregion
        #region
        public ChangeCustomerSettings(DataConnection datacontext, IConfiguration configuration)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
        }
        #endregion
        #region  get customer list i.e MESPAS ,RPA etc.
        public List<ChangeCustomerSettingsSummary> GeCustomerData()
        {
            #region created object of class
            List<ChangeCustomerSettingsSummary> lstCustomerData = new List<ChangeCustomerSettingsSummary>();
            #endregion
            try
            {
                #region get customer data for PIC change
                var customerData = (from p in _datacontext.MCUSTOMERTable
                                    join m in _datacontext.MCUSTOMER_MAPPINGTable on p.PK_CUSTOMER_ID equals m.FK_CUST_ID
                                    where p.DEPT_NAME == "MSD" || p.DEPT_NAME == "MESPAS"
                                      select new
                                      {
                                          p.PK_CUSTOMER_ID,
                                          p.CUSTOMERNAME,
                                          p.AS400USER_ID,
                                          p.CUSTOMER_EMAILID,
                                          m.AS400_CUSTOMER_NAME
                                      }).ToList();
                #endregion
                if (customerData != null)
                {
                    #region add data in list
                    foreach (var data in customerData)
                    {
                        ChangeCustomerSettingsSummary objdata = new ChangeCustomerSettingsSummary();
                        objdata.customerId = data.PK_CUSTOMER_ID;
                        objdata.templateCustomerName = data.CUSTOMERNAME;
                        if (!String.IsNullOrEmpty(data.AS400USER_ID))
                        {
                            objdata.AS400UserId = data.AS400USER_ID;
                        }
                        else
                        {
                            objdata.AS400UserId = "";
                        }
                        objdata.customerEmailId = data.CUSTOMER_EMAILID;
                        objdata.AS400CustomerName = data.AS400_CUSTOMER_NAME;
                        lstCustomerData.Add(objdata);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
            }
            return lstCustomerData;
        }
        #endregion

        #region
        public Message UpdateCustomerData(ChangeCustomerSettingsSummary objCustomerData)
        {
            Message obj = new Message();
            obj.result = "Error while updating customer data";
            Notification notification = new Notification(_Configuration);
            try
            {
                if (objCustomerData != null)
                {
                    var DuplicateEntry = (from hdrdata in _datacontext.MCUSTOMERTable
                                          where hdrdata.PK_CUSTOMER_ID == objCustomerData.customerId
                                          && hdrdata.AS400USER_ID.ToUpper().Trim() == objCustomerData.AS400UserId.ToUpper().Trim()
                                          && hdrdata.CUSTOMER_EMAILID.ToUpper().Trim() == objCustomerData.customerEmailId.ToUpper().Trim()
                                          select new
                                          {
                                              hdrdata.PK_CUSTOMER_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        MCUSTOMER objData = (from hdr in _datacontext.MCUSTOMERTable where hdr.PK_CUSTOMER_ID == objCustomerData.customerId select hdr).FirstOrDefault();
                        if (objData != null)
                        {
                            objData.AS400USER_ID = objCustomerData.AS400UserId;
                            objData.CUSTOMER_EMAILID = objCustomerData.customerEmailId;
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Data Updated Successfully";
                        notification.ReadExcel(objCustomerData);
                    }
                    else
                    {
                        obj.result = "Same data already updated";
                    }
                }
            }
            catch(Exception ex)
            {

            }
           return obj;
        }
        #endregion
    }
}
