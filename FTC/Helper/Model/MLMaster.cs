using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Helper.Model.MLClassDeclarations;
using static Helper.Data.DataContextClass;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
namespace Helper.Model
{
  public class MLMaster : IMLMaster
    {
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        public MLMaster(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        #region
        public List<MLMasterData1> GetMLMasterData(MLData objmlMaster)
        {
            List<MLMasterData1> lstMLMasterData = new List<MLMasterData1>();
            try
            {
                if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      }).ToList();
                       if (mldata != null)
                        {
                            foreach (var data in mldata)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      }).ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mldata)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM
                                      }).ToList();
                       if (mldata != null)
                        {
                            foreach (var data in mldata)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM; 
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM
                                      }).ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mldata)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lstMLMasterData;
        }
        #endregion
        #region
        public List<MLData2> GetIDwiseMLList(MLData objmlMaster)
        {
            List<MLData2> lstMLMasterData = new List<MLData2>();
            try
            {
                if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.PART_CODE.Trim() == objmlMaster.description.Trim() && p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      }).ToList();
                        var result1 = mldata;
                        if (result1 != null)
                        {
                            foreach (var data in result1)
                            {
                                MLData2 objdata = new MLData2();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                objdata.isSelected = true;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.PART_CODE.Trim() == objmlMaster.description.Trim() && p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      }).ToList();
                        var result1 = mldata;
                        if (result1 != null)
                        {
                            foreach (var data in result1)
                            {
                                MLData2 objdata = new MLData2();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                objdata.isSelected = true;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      where p.AS400_UOM.Trim() == objmlMaster.description.Trim()
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM
                                      }).ToList();
                        var result1 = mldata;
                        if (result1 != null)
                        {
                            foreach (var data in result1)
                            {
                                MLData2 objdata = new MLData2();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                objdata.isSelected = true;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      where p.AS400_UOM.Trim() == objmlMaster.description.Trim()
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM
                                      }).ToList();
                        var result1 = mldata;
                        if (result1 != null)
                        {
                            foreach (var data in result1)
                            {
                                MLData2 objdata = new MLData2();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                objdata.isSelected = true;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lstMLMasterData.Distinct().ToList();
        }
        #endregion
        #region
        public Message DeleteMlMaster(List<MLData> objmlMaster)
        {
            List<MLMasterData> lstMLMasterData = new List<MLMasterData>();
            Message obj = new Message();
            obj.result = "Error while Deleting Enquiry";
            foreach (var item in objmlMaster)
            {
                if (item.mlMasterType.ToUpper() == "IMPA" && item.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        T_SNQ_ENQUIRY_ML_ITEMS objDtl = (from dtl in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                                         where dtl.PK_ITEM_ID == item.itemIdPK
                                                         select dtl).ToList().SingleOrDefault();
                        if (objDtl != null)
                        {
                            _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Remove(objDtl);
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Data deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        obj.result = ex.Message;
                    }
                }
                else if (item.mlMasterType.ToUpper() == "IMPA" && item.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        T_SNQ_ENQUIRY_ML_ITEMS objDtl = (from dtl in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                                         where dtl.PK_ITEM_ID == item.itemIdPK
                                                         select dtl).ToList().SingleOrDefault();
                        if (objDtl != null)
                        {
                            _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Remove(objDtl);
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Data deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        obj.result = ex.Message;
                    }
                }
                else if (item.mlMasterType.ToUpper() == "UNIT" && item.processType.ToUpper() == "SNQ")
                {
                    try
                    {
                        MUOM objDtl = (from dtl in _datacontext.MUOMTable
                                       where dtl.PK_UOM_ID == item.itemIdPK
                                       select dtl).ToList().SingleOrDefault();
                        if (objDtl != null)
                        {
                            _datacontext.MUOMTable.Remove(objDtl);
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Data deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        obj.result = ex.Message;
                    }
                }
                else if (item.mlMasterType.ToUpper() == "UNIT" && item.processType.ToUpper() == "MSD")
                {
                    try
                    {
                        MUOM objDtl = (from dtl in _datacontext.MUOMTable
                                       where dtl.PK_UOM_ID == item.itemIdPK
                                       select dtl).ToList().SingleOrDefault();
                        if (objDtl != null)
                        {
                            _datacontext.MUOMTable.Remove(objDtl);
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Data deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        obj.result = ex.Message;
                    }
                }
            }
            return obj;
        }
        #endregion
        #region
        public Message SaveMLMaster(List<MLData> objmlMaster)
        {
            Message obj = new Message();
            obj.result = "Error while Creating Enquiry";
            if (objmlMaster != null)
            {
                foreach (var item in objmlMaster)
                {
                    if (item.mlMasterType.ToUpper() == "IMPA" && item.processType.ToUpper() == "SNQ")
                    {
                        var DuplicateEntry = (from hdrdata in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                              where hdrdata.PART_CODE == item.description && hdrdata.PART_NAME.ToUpper() == item.tempDescription.ToUpper()
                                              select new
                                              {
                                                  hdrdata.PK_ITEM_ID
                                              }).ToList();
                        if (DuplicateEntry.Count == 0)
                        {
                            if (item.itemIdPK == 0)
                            {
                                T_SNQ_ENQUIRY_ML_ITEMS objData = new T_SNQ_ENQUIRY_ML_ITEMS();
                                objData.PART_CODE = item.description;
                                objData.PART_NAME = item.tempDescription;
                                objData.IS_ACTIVE = 1;
                                objData.CREATEDAT = DateTime.Now;
                                _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Add(objData);
                                _datacontext.SaveChanges();
                                obj.result = "Data Updated successfully";
                            }
                            else
                            {
                                T_SNQ_ENQUIRY_ML_ITEMS objData = (from hdr in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable where hdr.PK_ITEM_ID == item.itemIdPK  select hdr).FirstOrDefault();
                                objData.PART_CODE = item.description;
                                objData.PART_NAME = item.tempDescription;
                                objData.IS_ACTIVE = 1;
                                objData.CREATEDAT = DateTime.Now;
                                _datacontext.SaveChanges();
                                obj.result = "Data Updated Successfully";
                            }
                        }
                        else
                        {
                            obj.result = "Same entry already exist";
                        }
                    }
                    else if (item.mlMasterType.ToUpper() == "IMPA" && item.processType.ToUpper() == "MSD")
                    {
                        var DuplicateEntry = (from hdrdata in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                              where hdrdata.PART_CODE == item.description && hdrdata.PART_NAME.ToUpper() == item.tempDescription.ToUpper()
                                              select new
                                              {
                                                  hdrdata.PK_ITEM_ID
                                              }).ToList();
                        if (DuplicateEntry.Count == 0)
                        {
                            if (item.itemIdPK == 0)
                            {
                                T_SNQ_ENQUIRY_ML_ITEMS objData = new T_SNQ_ENQUIRY_ML_ITEMS();
                                objData.PART_CODE = item.description;
                                objData.PART_NAME = item.tempDescription;
                                objData.IS_ACTIVE = 1;
                                objData.CREATEDAT = DateTime.Now;
                                _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Add(objData);
                                _datacontext.SaveChanges();
                                obj.result = "Data Updated successfully";
                            }
                            else
                            {
                                T_SNQ_ENQUIRY_ML_ITEMS objData = (from hdr in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable where hdr.PK_ITEM_ID == item.itemIdPK select hdr).FirstOrDefault();
                                objData.PART_CODE = item.description;
                                objData.PART_NAME = item.tempDescription;
                                objData.IS_ACTIVE = 1;
                                objData.CREATEDAT = DateTime.Now;
                                _datacontext.SaveChanges();
                                obj.result = "Data Updated Successfully";
                            }
                        }
                        else
                        {
                            obj.result = "Same entry already exist";
                            return obj;
                        }
                    }
                    else if (item.mlMasterType.ToUpper() == "UNIT" && item.processType.ToUpper() == "SNQ")
                    {
                        var DuplicateEntry = (from hdrdata in _datacontext.MUOMTable
                                              where (hdrdata.TEMPLATE_UOM.Trim() == item.tempDescription.Trim() && hdrdata.AS400_UOM.Trim() == item.description.Trim())
                                              select new
                                              {
                                                  hdrdata.PK_UOM_ID
                                              }).ToList();
                        if (DuplicateEntry.Count == 0)
                        {
                            if (item.itemIdPK == 0)
                            {
                                MUOM objData = new MUOM();
                                objData.AS400_UOM = item.description;
                                objData.TEMPLATE_UOM = item.tempDescription;
                                objData.CREATED_BY = 1;
                                objData.CREATED_DATE = DateTime.Now;
                                _datacontext.MUOMTable.Add(objData);
                                _datacontext.SaveChanges();
                                obj.result = "Data Saved successfully";
                            }
                            else
                            {
                                MUOM objData = (from hdr in _datacontext.MUOMTable
                                                where (hdr.PK_UOM_ID == item.itemIdPK)
                                                select hdr).FirstOrDefault();
                                if (objData != null)
                                {
                                    objData.TEMPLATE_UOM = item.tempDescription;
                                    objData.CREATED_BY = 1;
                                    objData.CREATED_DATE = DateTime.Now;
                                    _datacontext.SaveChanges();
                                   obj.result = "Data Saved successfully";
                                }
                            }
                        }
                        else
                        {
                           obj.result = "same entry already exist";
                        }
                    }
                    else if (item.mlMasterType.ToUpper() == "UNIT" && item.processType.ToUpper() == "MSD")
                    {
                        var DuplicateEntry = (from hdrdata in _datacontext.MUOMTable
                                              where (hdrdata.TEMPLATE_UOM.Trim() == item.tempDescription.Trim() && hdrdata.AS400_UOM.Trim() == item.description.Trim())
                                              select new
                                              {
                                                  hdrdata.PK_UOM_ID
                                              }).ToList();
                        if (DuplicateEntry.Count == 0)
                        {
                            if (item.itemIdPK == 0)
                            {
                                MUOM objData = new MUOM();
                                objData.AS400_UOM = item.description;
                                objData.TEMPLATE_UOM = item.tempDescription;
                                objData.CREATED_BY = 1;
                                objData.CREATED_DATE = DateTime.Now;
                                _datacontext.MUOMTable.Add(objData);
                                _datacontext.SaveChanges();
                                obj.result = "Data Updated successfully";
                            }
                            else
                            {
                                MUOM objData = (from hdr in _datacontext.MUOMTable
                                                where (hdr.PK_UOM_ID == item.itemIdPK)
                                                select hdr).FirstOrDefault();
                                if (objData != null)
                                {
                                    objData.TEMPLATE_UOM = item.tempDescription;
                                    objData.CREATED_BY = 1;
                                    objData.CREATED_DATE = DateTime.Now;
                                    _datacontext.SaveChanges();
                                    obj.result = "Data Updated successfully";
                                }
                            }
                        }
                        else
                        {
                            obj.result = "same entry already exist";
                            return obj;
                        }
                    }
                }
            }
            return obj;
        }
        #endregion
        #region
        public List<MLProcessType> GetMLProcessType()
        {
            List<MLProcessType> lstMLProcessType = new List<MLProcessType>();
            try
            {
                var mldata = (from p in _datacontext.M_PROCESSTable
                              where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_PROCESS_ID,
                                          p.PROCESS_TYPE,
                                      }).ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mldata)
                            {
                        MLProcessType objdata = new MLProcessType();
                                objdata.processTypeId = data.PK_PROCESS_ID;
                                objdata.processType = data.PROCESS_TYPE;
                                lstMLProcessType.Add(objdata);
                            }
                        }
            }
            catch (Exception ex)
            {
            }
            return lstMLProcessType;
        }
        #endregion
        #region
        public List<MLType> GetMLType(MLType objMLType)
        {
            List<MLType> lstMLProcessType = new List<MLType>();
            try
            {
                var mldata = (from p in _datacontext.M_PROCESSTable
                              join t in _datacontext.M_MLTYPETable on p.PK_PROCESS_ID equals t.FK_PROCESSTYPE_ID
                              where p.PROCESS_TYPE.Trim().ToUpper() == objMLType.processType.Trim().ToUpper() && t.IS_ACTIVE == 1
                              select new
                              {
                                  t.MLTYPE,
                                  t.PK_MLTYPE,
                                  t.FK_PROCESSTYPE_ID,
                                  p.PROCESS_TYPE
                              }).ToList();
                if (mldata != null)
                {
                    foreach (var data in mldata)
                    {
                        MLType objdata = new MLType();
                        objdata.mlTypeId = data.PK_MLTYPE;
                        objdata.processTypeId = data.FK_PROCESSTYPE_ID;
                        objdata.mlType = data.MLTYPE;
                        objdata.processType = data.PROCESS_TYPE;
                        lstMLProcessType.Add(objdata);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lstMLProcessType;
        }
        #endregion
        #region
        public List<PagignationData> GetPagignationData(PagignationData objPagignationData)
        {
            PagignationData itemss = new PagignationData();
            List<PagignationData> lstpagignationData = new List<PagignationData>();
            if (objPagignationData.mlMasterType.ToUpper() == "IMPA" && objPagignationData.processType.ToUpper() == "SNQ")
            {
                // Return List of Customer  
                var source = (from customer in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.
                            OrderBy(a => a.PART_CODE)
                              select customer).ToList();
                //Search Parameter [With null check]  
                // ------------------------------------ Search Parameter-------------------   
                if (!string.IsNullOrEmpty(objPagignationData.search))
                {
                    source = source.Where(a => a.PART_NAME.Contains(objPagignationData.search) || a.PART_CODE.Contains(objPagignationData.search)).ToList();
                }
                // ------------------------------------ Search Parameter-------------------  
                // Get's No of Rows Count   
                int count = source.Count();
                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = objPagignationData.pageNumber;
                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = objPagignationData.pageSize;
                // Display TotalCount to Records to User  
                int TotalCount = count;
                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
                // Returns List of Customer after applying Paging   
                var items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";
                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";
                // Object which we are going to send in header   
                var paginationMetadata = new
                {
                    totalCount = TotalCount,
                    pageSize = PageSize,
                    currentPage = CurrentPage,
                    totalPages = TotalPages,
                    previousPage,
                    nextPage,
                    QuerySearch = string.IsNullOrEmpty(objPagignationData.search) ?
                                  "No Parameter Passed" : objPagignationData.search
                };
                // Setting Header  
                HttpResponseMessage response = new HttpResponseMessage();
                response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
                // Returing List of Customers Collections
                // 
                foreach (var item in items)
                {
                    PagignationData obj = new PagignationData();
                    obj.itemIdPK = item.PK_ITEM_ID.ToString();
                    obj.description = item.PART_CODE.ToString();
                    obj.tempDescription = item.PART_NAME.ToString();
                    lstpagignationData.Add(obj);
                }
            }
            else if (objPagignationData.mlMasterType.ToUpper() == "UNIT" && objPagignationData.processType.ToUpper() == "SNQ")
            {
                // Return List of Customer  
                var source = (from customer in _datacontext.MUOMTable
                              select customer).ToList();
                //Search Parameter [With null check]  
                // ------------------------------------ Search Parameter-------------------   
                if (!string.IsNullOrEmpty(objPagignationData.search))
                {
                    source = source.Where(a => a.TEMPLATE_UOM.Contains(objPagignationData.search) || a.AS400_UOM.Contains(objPagignationData.search)).ToList();
                }
                // ------------------------------------ Search Parameter-------------------  
                // Get's No of Rows Count   
                int count = source.Count();
                // Parameter is passed from Query string if it is null then it default Value will be pageNumber:1  
                int CurrentPage = objPagignationData.pageNumber;
                // Parameter is passed from Query string if it is null then it default Value will be pageSize:20  
                int PageSize = objPagignationData.pageSize;
                // Display TotalCount to Records to User  
                int TotalCount = count;
                // Calculating Totalpage by Dividing (No of Records / Pagesize)  
                int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
                // Returns List of Customer after applying Paging   
               var  items = source.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
                // if CurrentPage is greater than 1 means it has previousPage  
                var previousPage = CurrentPage > 1 ? "Yes" : "No";
                // if TotalPages is greater than CurrentPage means it has nextPage  
                var nextPage = CurrentPage < TotalPages ? "Yes" : "No";
                // Object which we are going to send in header   
                var paginationMetadata = new
                {
                    totalCount = TotalCount,
                    pageSize = PageSize,
                    currentPage = CurrentPage,
                    totalPages = TotalPages,
                    previousPage,
                    nextPage,
                    QuerySearch = string.IsNullOrEmpty(objPagignationData.search) ?
                                  "No Parameter Passed" : objPagignationData.search
                };
                // Setting Header  
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
                //HttpResponseMessage response = new HttpResponseMessage();
                //response.Headers.Add("Paging-Headers", JsonConvert.SerializeObject(paginationMetadata));
                //// Returing List of Customers Collections  
                foreach (var item in items)
                {
                    PagignationData obj = new PagignationData();
                    obj.itemIdPK = item.PK_UOM_ID.ToString();
                    obj.description = item.AS400_UOM.ToString();
                    obj.tempDescription = item.TEMPLATE_UOM.ToString();
                    lstpagignationData.Add(obj);
                }
            }
            return lstpagignationData;
        }
        #endregion
        #region
        // description - partcode
        //tempdescription - partname
        public Message RegisterNewImpa(MLData objmlMaster)
        {
            Message obj = new Message();
            obj.result = "Error while Creating Enquiry";
            if (objmlMaster != null)
            {
                if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    var DuplicateEntry = (from hdrdata in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                          where hdrdata.PART_CODE == objmlMaster.description && hdrdata.PART_NAME.ToUpper() == objmlMaster.tempDescription.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_ITEM_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        T_SNQ_ENQUIRY_ML_ITEMS objData = new T_SNQ_ENQUIRY_ML_ITEMS();
                        objData.PART_CODE = objmlMaster.description;
                        objData.PART_NAME = objmlMaster.tempDescription;
                        objData.IS_ACTIVE = 1;
                        objData.CREATEDAT = DateTime.Now;
                        _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Add(objData);
                        _datacontext.SaveChanges();
                        obj.result = "Data Saved successfully";
                    }
                    else
                    {
                        obj.result = "Same entry already exist";
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "IMPA" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    var DuplicateEntry = (from hdrdata in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                          where hdrdata.PART_CODE == objmlMaster.description && hdrdata.PART_NAME.ToUpper() == objmlMaster.tempDescription.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_ITEM_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        T_SNQ_ENQUIRY_ML_ITEMS objData = new T_SNQ_ENQUIRY_ML_ITEMS();
                        objData.PART_CODE = objmlMaster.description;
                        objData.PART_NAME = objmlMaster.tempDescription;
                        objData.IS_ACTIVE = 1;
                        objData.CREATEDAT = DateTime.Now;
                        _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Add(objData);
                        _datacontext.SaveChanges();
                        obj.result = "Data Saved successfully";
                    }
                    else
                    {
                        obj.result = "Same entry already exist";
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "MSD")
                {
                    var DuplicateEntry = (from hdrdata in _datacontext.MUOMTable
                                          where hdrdata.TEMPLATE_UOM == objmlMaster.description && hdrdata.AS400_UOM.ToUpper() == objmlMaster.tempDescription.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_UOM_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        MUOM objData = new MUOM();
                        objData.TEMPLATE_UOM = objmlMaster.description;
                        objData.AS400_UOM = objmlMaster.tempDescription;
                        objData.CREATED_DATE = DateTime.Now;
                        _datacontext.MUOMTable.Add(objData);
                        _datacontext.SaveChanges();
                        obj.result = "Data Saved successfully";
                    }
                    else
                    {
                        obj.result = "Same entry already exist";
                    }
                }
                else if (objmlMaster.mlMasterType.ToUpper() == "UNIT" && objmlMaster.processType.ToUpper() == "SNQ")
                {
                    var DuplicateEntry = (from hdrdata in _datacontext.MUOMTable
                                          where hdrdata.TEMPLATE_UOM == objmlMaster.description && hdrdata.AS400_UOM.ToUpper() == objmlMaster.tempDescription.ToUpper()
                                          select new
                                          {
                                              hdrdata.PK_UOM_ID
                                          }).ToList();
                    if (DuplicateEntry.Count == 0)
                    {
                        MUOM objData = new MUOM();
                        objData.TEMPLATE_UOM = objmlMaster.description;
                        objData.AS400_UOM = objmlMaster.tempDescription;
                        objData.CREATED_DATE = DateTime.Now;
                        _datacontext.MUOMTable.Add(objData);
                        _datacontext.SaveChanges();
                        obj.result = "Data Saved successfully";
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
        public List<MLMasterData1> GetMLDataBySearch(searchdata objsearchdata)
        {
            List<MLMasterData1> lstMLMasterData = new List<MLMasterData1>();
            try
            {
                if (objsearchdata.mlMasterType.ToUpper() == "IMPA" && objsearchdata.processType.ToUpper() == "SNQ")
                {
                    if (objsearchdata.FromDate != "")
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME,
                                          p.CREATEDAT
                                      });
                        if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            mldata = mldata.Where(x => x.CREATEDAT.Date == fromDt.Date);
                        }
                        else if (objsearchdata.FromDate != "" && objsearchdata.ToDate != "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                            if (objsearchdata.FromDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATEDAT.Date >= fromDt);
                            }
                            if (objsearchdata.ToDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATEDAT.Date <= toDt);
                            }
                        }
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    else
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      });
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                }
                else if (objsearchdata.mlMasterType.ToUpper() == "IMPA" && objsearchdata.processType.ToUpper() == "MSD")
                {
                    if (objsearchdata.FromDate != "")
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME,
                                          p.CREATEDAT
                                      });
                        if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            mldata = mldata.Where(x => x.CREATEDAT.Date == fromDt.Date);
                        }
                        else if (objsearchdata.FromDate != "" && objsearchdata.ToDate != "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                            if (objsearchdata.FromDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATEDAT.Date >= fromDt);
                            }
                            if (objsearchdata.ToDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATEDAT.Date <= toDt);
                            }
                        }
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    else
                    {
                        var mldata = (from p in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                      where p.IS_ACTIVE == 1
                                      select new
                                      {
                                          p.PK_ITEM_ID,
                                          p.PART_CODE,
                                          p.PART_NAME
                                      });
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_ITEM_ID.ToString();
                                objdata.description = data.PART_CODE;
                                objdata.tempDescription = data.PART_NAME;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                }
                else if (objsearchdata.mlMasterType.ToUpper() == "UNIT" && objsearchdata.processType.ToUpper() == "SNQ")
                {
                    if (objsearchdata.FromDate != "")
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM,
                                          p.CREATED_DATE
                                      });
                        if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            mldata = mldata.Where(x => x.CREATED_DATE.Date == fromDt.Date);
                        }
                        else if (objsearchdata.FromDate != "" && objsearchdata.ToDate != "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                            if (objsearchdata.FromDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATED_DATE.Date >= fromDt);
                            }
                            if (objsearchdata.ToDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATED_DATE.Date <= toDt);
                            }
                        }
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    else
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM,
                                          p.CREATED_DATE
                                      });
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                }
                else if (objsearchdata.mlMasterType.ToUpper() == "UNIT" && objsearchdata.processType.ToUpper() == "MSD")
                {
                    if (objsearchdata.FromDate != "")
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM,
                                          p.CREATED_DATE
                                      });
                        if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            mldata = mldata.Where(x => x.CREATED_DATE.Date == fromDt.Date);
                        }
                        else if (objsearchdata.FromDate != "" && objsearchdata.ToDate != "")
                        {
                            DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                            DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                            if (objsearchdata.FromDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATED_DATE.Date >= fromDt);
                            }
                            if (objsearchdata.ToDate != "")
                            {
                                mldata = mldata.Where(x => x.CREATED_DATE.Date <= toDt);
                            }
                        }
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                    else
                    {
                        var mldata = (from p in _datacontext.MUOMTable
                                      select new
                                      {
                                          p.PK_UOM_ID,
                                          p.TEMPLATE_UOM,
                                          p.AS400_UOM,
                                          p.CREATED_DATE
                                      });
                        var mllist = mldata.ToList();
                        if (mldata != null)
                        {
                            foreach (var data in mllist)
                            {
                                MLMasterData1 objdata = new MLMasterData1();
                                objdata.itemIdPK = data.PK_UOM_ID.ToString();
                                objdata.description = data.AS400_UOM;
                                objdata.tempDescription = data.TEMPLATE_UOM;
                                lstMLMasterData.Add(objdata);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lstMLMasterData;
        }
    }
}  
