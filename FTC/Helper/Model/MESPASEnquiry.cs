using DuoVia.FuzzyStrings;
using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Enum.Status;
using System.Linq;
using static Helper.Data.DataContextClass;
using System.IO;
using static Helper.Model.MESPASClassDeclarations;
using System.Net.Mail;
namespace Helper.Model
{
    public class MESPASEnquiry : IMESPASEnquiry
    {
        #region created object of connection and configuration class
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        #endregion
        #region created object of enum 
        statusCode notstartedStatus = statusCode.notStartedStatus;
        #endregion
        #region  created controller of that class
        public MESPASEnquiry(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        #endregion
        #region
        public Message InsertEnquiry(Enquiryheader objEnquirydtls)
        {
            #region created of object of return result
            Message obj = new Message();
            obj.result = "Error while Creating Enquiry";
            #endregion
            int isUpdatedWithML = 0;
            #region set dulicate flag
            string isDuplicteEntry = "NO";
            #endregion
            #region set ML Allow for MESPAS RFQ
            string MLLogicMESPAS = this._Configuration.GetSection("MLLogicMESPAS")["MLAllow"];
            #endregion
            #region I maker recivedblank from RFQ api save ****
            string MakerBlank = this._Configuration.GetSection("MSDMakers")["MakerBlank"];
            #endregion
            #region created object of MSD Class
            MSDEnquiry objenquiry = new MSDEnquiry(_datacontext, _Configuration, _hub);
            #endregion
            try
            {
                if (objEnquirydtls != null)
                {
                    if (objEnquirydtls.enqrefNo != "")
                    {
                        #region check rfq no empty or not..if it is empty rfq details sedn error page
                        var DuplicateEntry = (from hdrdata in _datacontext.T_MESPAS_ENQUIRY_HDRTable
                                              where hdrdata.ENQREF_NO == objEnquirydtls.enqrefNo.Trim() && hdrdata.STATUS != 8
                                              select new
                                              {
                                                  hdrdata.ENQREF_NO
                                              }).ToList();
                        if (DuplicateEntry.Count > 0)
                        {
                            isDuplicteEntry = "YES";
                        }
                        #endregion DuplicateEntry
                        #region
                        if (isDuplicteEntry == "NO" || this._Configuration.GetSection("DuplicateEnquirysMESPAS")["DuplicateAllow"] == "YES")
                        {
                            #region set document path to seprate variable
                            string strPath = objEnquirydtls.docPath;
                            #endregion
                            #region get list of snq items for ml checking
                            var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                           select new
                                           {
                                               part.PART_NAME,
                                               part.PART_CODE
                                           }).Distinct().ToList();
                            #endregion
                            #region stored MESPAS RFQ's Header part in this table
                            T_MESPAS_ENQUIRY_HDR objenqheader = new T_MESPAS_ENQUIRY_HDR();
                            objenqheader.FK_PROCESS_ID = 1;
                            objenqheader.FK_INSTANCE_ID = 1;
                            objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                            objenqheader.OWNER = objenquiry.GetCustomerMapping(objEnquirydtls.owner);
                            objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                            objenqheader.SHIP_NAME = objenquiry.GetShipName(objEnquirydtls.shipName);
                            objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                            objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMESPASDocumentFolder"] + "" + Path.GetFileName(strPath);
                            objenqheader.STATUS = Convert.ToInt32(objEnquirydtls.status);
                            objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                            objenqheader.CREATED_BY = 1;
                            objenqheader.CREATED_DATE = DateTime.Now;
                            objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                            #region checked maker balnk or not
                            if (objEnquirydtls.maker.Trim() == "")
                            {
                                objenqheader.MAKER = MakerBlank;
                            }
                            else
                            {
                                objenqheader.MAKER = objenquiry.GetManufacturingMapping(objEnquirydtls.maker);
                            }
                            #endregion
                            objenqheader.TYPE = objEnquirydtls.type;
                            objenqheader.EQUIPMENT = objEnquirydtls.equipment;
                            objenqheader.SERIAL_NO = objEnquirydtls.serialNo;
                            objenqheader.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                            objenqheader.NET_AMOUNT = objEnquirydtls.netAmount;
                            objenqheader.EMAIL_RECEIVED_AT = objEnquirydtls.emailReceivedat;
                            objenqheader.EMAIL_PROCESSED_AT = objEnquirydtls.emailProcessedat;
                            objenqheader.IN_ERROR_AT = objEnquirydtls.inErrorat;
                            objenqheader.VERIFIED_AT = objEnquirydtls.verifiedAt;
                            objenqheader.UPDATED_AT = objEnquirydtls.updatedAt;
                            objenqheader.QUOTATION_CREATED_AT = objEnquirydtls.quotationCreatedat;
                            objenqheader.PORT = objEnquirydtls.port;
                            objenqheader.MAPPING_PORT = objEnquirydtls.mappingPort;
                            objenqheader.SUPPLIER_CODE = objenquiry.GetSupplierMapping(objEnquirydtls.maker, objEnquirydtls.supplierCode);
                            objenqheader.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                            objenqheader.LEAD_TIME = 0;
                            objenqheader.SAVE_AS_DRAFT = "";
                            objenqheader.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                            objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                            objenqheader.EMAIL_FROM = objEnquirydtls.emailFrom;
                            objenqheader.MAKER_INFO = objEnquirydtls.makerInfo;
                            objenqheader.SOURCE_TYPE = objEnquirydtls.sourceType;
                            objenqheader.EVENT_ID = objEnquirydtls.eventId;
                            _datacontext.T_MESPAS_ENQUIRY_HDRTable.Add(objenqheader);
                            _datacontext.SaveChanges();
                            #endregion
                            #region  get id from hdr table
                            long PKENQHDRID = objenqheader.PK_MESPASENQUIRY_HDR_ID;
                            #endregion
                            #region stored MESPAS RFQ's Origin(Without any modifying) Header part in this table
                            T_MESPAS_ENQUIRY_HDR_ORG objenqheaderorg = new T_MESPAS_ENQUIRY_HDR_ORG();
                            objenqheaderorg.FK_PROCESS_ID = 1;
                            objenqheaderorg.FK_INSTANCE_ID = 1;
                            objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                            objenqheaderorg.OWNER = objEnquirydtls.owner;
                            objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                            objenqheaderorg.SHIP_NAME = objenquiry.GetShipName(objEnquirydtls.shipName);
                            objenqheaderorg.ENQREF_NO = objEnquirydtls.enqrefNo;
                            objenqheaderorg.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMESPASDocumentFolder"] + "" + Path.GetFileName(strPath);
                            objenqheaderorg.CREATED_BY = 1;
                            objenqheaderorg.CREATED_DATE = DateTime.Now;
                            objenqheaderorg.ERROR_CODE = objEnquirydtls.errorCode;
                            objenqheaderorg.MAKER = objEnquirydtls.maker;
                            objenqheaderorg.TYPE = objEnquirydtls.type;
                            objenqheaderorg.EQUIPMENT = objEnquirydtls.equipment;
                            objenqheaderorg.SERIAL_NO = objEnquirydtls.serialNo;
                            objenqheaderorg.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                            objenqheaderorg.NET_AMOUNT = objEnquirydtls.netAmount;
                            objenqheaderorg.RFQ_URL = objEnquirydtls.rfqUrl;
                            objenqheaderorg.MAKER_INFO = objEnquirydtls.makerInfo;
                            objenqheaderorg.SOURCE_TYPE = objEnquirydtls.sourceType;
                            _datacontext.T_MESPAS_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                            _datacontext.SaveChanges();
                            #endregion
                            #region get if from origin table
                            long PKENQHDRORGID = objenqheader.PK_MESPASENQUIRY_HDR_ID;
                            #endregion
                            #region save multiple document of RFQ's
                            if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                            {
                                foreach (var item in objEnquirydtls.docHdrDetails)
                                {
                                    #region stored header level documnet
                                    T_MESPAS_DOCUMENT_HDR objenqdetails = new T_MESPAS_DOCUMENT_HDR();
                                    {
                                        objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                        objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                        objenqdetails.DOC_PATH = item.docPath;
                                        objenqdetails.CREATEBY = 1;
                                        objenqdetails.CREATEDAT = DateTime.Now;
                                        objenqdetails.ISACTIVE = 1;
                                    };
                                    _datacontext.T_MESPAS_DOCUMENT_HDRTable.Add(objenqdetails);
                                    _datacontext.SaveChanges();
                                    #endregion
                                }
                            }
                            #endregion
                            #region save multiple items of RFQ's
                            if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                            {
                                foreach (var item in objEnquirydtls.itemDetails)
                                {
                                    #region check cost recived blank  if cost blank rfq going to error basket
                                    if (item.cost != "")
                                    {
                                        #region
                                        T_MESPAS_ENQUIRY_HDR objenqhead = new T_MESPAS_ENQUIRY_HDR();
                                        objenqhead.STATUS = 5;
                                        _datacontext.SaveChanges();
                                        #endregion
                                        string[] cost = item.cost.Split(".00");
                                        #region
                                        T_MESPAS_ENQUIRY_DTL_ORG objenqdetailsorg = new T_MESPAS_ENQUIRY_DTL_ORG();
                                        {
                                            objenqdetailsorg.FK_MESPASENQUIRY_HDR_ID = PKENQHDRORGID;
                                            objenqdetailsorg.PART_CODE = item.partCode;
                                            objenqdetailsorg.PART_NAME = item.partName;
                                            objenqdetailsorg.QUANTITY = item.quantity.ToString();
                                            objenqdetailsorg.UNIT = item.unit;
                                            objenqdetailsorg.PRICE = item.price;
                                            objenqdetailsorg.COST = cost[0];
                                            objenqdetailsorg.UPDATED_DATE = DateTime.Now.ToString();
                                            objenqdetailsorg.SUPPLIER = item.supplier;
                                            objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetailsorg.ERROR_CODE = item.errorCode;
                                            objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                            objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                        };
                                        _datacontext.T_MESPAS_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                        _datacontext.SaveChanges();
                                        #endregion
                                        //ML Logic For PartCode/Impa Code
                                        #region ML Logic to find IMPA code
                                        string partCode = item.partCode;
                                        string partName = item.partName;
                                        if (MLLogicMESPAS == "YES")
                                        {
                                            if ((partCode == null || partCode.Length != 6) && !string.IsNullOrEmpty(partName))
                                            {
                                                double percentage = 0;
                                                string matchingPercent = this._Configuration.GetSection("MatchingPercent")["MATCHINGVALUE"];
                                                for (int counter = 0; counter < lstpart.Count; counter++)
                                                {
                                                    var lcs = partName.LongestCommonSubsequence(lstpart[counter].PART_NAME);
                                                    if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2 && partCode.Length <= lstpart[counter].PART_CODE.Length)
                                                    {
                                                        percentage = lcs.Item2;
                                                        partCode = lstpart[counter].PART_CODE;
                                                        isUpdatedWithML = 1;
                                                    }
                                                }
                                            }
                                        }
                                        #endregion ML Logic to find IMPA code
                                        //ML Logic For PartCode/Impa Code
                                        #region stored RFQ's item details
                                        T_MESPAS_ENQUIRY_DTL objenqdetails = new T_MESPAS_ENQUIRY_DTL();
                                        {
                                            objenqdetails.FK_MESPASENQUIRY_HDR_ID = PKENQHDRID;
                                            objenqdetails.PART_CODE = item.partCode;
                                            objenqdetails.PART_NAME = item.partName;
                                            objenqdetails.QUANTITY = item.quantity.ToString();
                                            objenqdetails.UNIT = objenquiry.GetUnitMapping(item.unit);
                                            objenqdetails.PRICE = item.price;
                                            objenqdetails.COST = cost[0];
                                            objenqdetails.UPDATED_DATE = Convert.ToString(DateTime.Now);
                                            objenqdetails.SUPPLIER = item.supplier;
                                            objenqdetails.STATUS = 5;
                                            objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetails.ERROR_CODE = item.errorCode;
                                            objenqdetails.ACCOUNT_NO = item.accountNo;
                                            objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                            objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                            objenqdetails.LEAD_TIME = 0;
                                            objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                        };
                                        _datacontext.T_MESPAS_ENQUIRY_DTLTable.Add(objenqdetails);
                                        _datacontext.SaveChanges();
                                        #endregion
                                        #region get if from header table
                                        long PKDTLID = objenqdetails.PK_MESPASENQUIRY_DTL_ID;
                                        #endregion
                                        if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                        {
                                            #region
                                            if (item.docdtlDetails != null)
                                            {
                                                foreach (var item2 in item.docdtlDetails)
                                                {
                                                    #region stored item level documents 
                                                    T_MESPAS_DOCUMENT_DTL objdocdtl = new T_MESPAS_DOCUMENT_DTL();
                                                    {
                                                        objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                                        objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                                        objdocdtl.DOC_PATH = item2.docPath;
                                                        objdocdtl.CREATEBY = 1;
                                                        objdocdtl.CREATEDAT = DateTime.Now;
                                                        objdocdtl.ISACTIVE = 1;
                                                    };
                                                    _datacontext.T_MESPAS_DOCUMENT_DTLTable.Add(objdocdtl);
                                                    _datacontext.SaveChanges();
                                                    #endregion
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                    #endregion
                                    else
                                    {
                                        string[] cost = item.cost.Split(".00");
                                        #region dtl table
                                        T_MESPAS_ENQUIRY_DTL_ORG objenqdetailsorg = new T_MESPAS_ENQUIRY_DTL_ORG();
                                        {
                                            objenqdetailsorg.FK_MESPASENQUIRY_HDR_ID = PKENQHDRORGID;
                                            objenqdetailsorg.PART_CODE = item.partCode;
                                            objenqdetailsorg.PART_NAME = item.partName;
                                            objenqdetailsorg.QUANTITY = item.quantity.ToString();
                                            objenqdetailsorg.UNIT = item.unit;
                                            objenqdetailsorg.PRICE = item.price;
                                            objenqdetailsorg.COST = cost[0];
                                            objenqdetailsorg.UPDATED_DATE = DateTime.Now.ToString();
                                            objenqdetailsorg.SUPPLIER = item.supplier;
                                            objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetailsorg.ERROR_CODE = item.errorCode;
                                            objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                            objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                        };
                                        _datacontext.T_MESPAS_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                        _datacontext.SaveChanges();
                                        #endregion
                                        //ML Logic For PartCode/Impa Code
                                        #region ML Logic to find IMPA code
                                        string partCode = item.partCode;
                                        string partName = item.partName;
                                        if (MLLogicMESPAS == "YES")
                                        {
                                            if ((partCode == null || partCode.Length != 6) && !string.IsNullOrEmpty(partName))
                                            {
                                                double percentage = 0;
                                                string matchingPercent = this._Configuration.GetSection("MatchingPercent")["MATCHINGVALUE"];
                                                for (int counter = 0; counter < lstpart.Count; counter++)
                                                {
                                                    var lcs = partName.LongestCommonSubsequence(lstpart[counter].PART_NAME);
                                                    if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2 && partCode.Length <= lstpart[counter].PART_CODE.Length)
                                                    {
                                                        percentage = lcs.Item2;
                                                        partCode = lstpart[counter].PART_CODE;
                                                        isUpdatedWithML = 1;
                                                    }
                                                }
                                            }
                                        }
                                        #endregion ML Logic to find IMPA code
                                        //ML Logic For PartCode/Impa Code
                                        #region stored RFQ's items
                                        T_MESPAS_ENQUIRY_DTL objenqdetails = new T_MESPAS_ENQUIRY_DTL();
                                        {
                                            objenqdetails.FK_MESPASENQUIRY_HDR_ID = PKENQHDRID;
                                            objenqdetails.PART_CODE = partCode;
                                            objenqdetails.PART_NAME = item.partName;
                                            objenqdetails.QUANTITY = item.quantity.ToString();
                                            objenqdetails.UNIT = objenquiry.GetUnitMapping(item.unit);
                                            objenqdetails.PRICE = item.price;
                                            objenqdetails.COST = cost[0];
                                            objenqdetails.UPDATED_DATE = Convert.ToString(DateTime.Now);
                                            objenqdetails.SUPPLIER = item.supplier;
                                            objenqdetails.STATUS = Convert.ToInt32(item.status);
                                            objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetails.ERROR_CODE = item.errorCode;
                                            objenqdetails.ACCOUNT_NO = item.accountNo;
                                            objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                            objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                            objenqdetails.LEAD_TIME = 0;
                                            objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                        };
                                        _datacontext.T_MESPAS_ENQUIRY_DTLTable.Add(objenqdetails);
                                        _datacontext.SaveChanges();
                                        #endregion
                                        #region get dtl id for documnet saving
                                        long PKDTLID = objenqdetails.PK_MESPASENQUIRY_DTL_ID;
                                        #endregion
                                        #region
                                        if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                        {
                                            if (item.docdtlDetails != null)
                                            {
                                                foreach (var item2 in item.docdtlDetails)
                                                {
                                                    #region document dtls
                                                    T_MESPAS_DOCUMENT_DTL objdocdtl = new T_MESPAS_DOCUMENT_DTL();
                                                    {
                                                        objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                                        objdocdtl.DOC_PATH = item2.docPath;
                                                        objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                                        objdocdtl.CREATEBY = 1;
                                                        objdocdtl.CREATEDAT = DateTime.Now;
                                                        objdocdtl.ISACTIVE = 1;
                                                    };
                                                    _datacontext.T_MESPAS_DOCUMENT_DTLTable.Add(objdocdtl);
                                                    _datacontext.SaveChanges();
                                                    #endregion
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                            #region retrun success message
                            obj.result = "Enquiry Saved Successfully";
                            #endregion
                            #region send notification to client or flogic team when RFQ recived from MESPAS 
                            SendRecivedMespasRFQotifcation(objEnquirydtls.enqrefNo, objEnquirydtls.owner);
                            #endregion
                            #region  signalR implemeneted for data refresh within sec on portal
                            _hub.Clients.All.BroadcastMessage();
                            #endregion
                        }
                        else
                        {
                            #region return duplicate retrun message
                            obj.result = "Duplicate Entry";
                            #endregion
                            #region send notification to client or flogic team when RFQ recived two times from MESPAS 
                            SendDuplicateEnquiryNotification(objEnquirydtls.enqrefNo);
                            #endregion 
                        }
                        #endregion
                    }
                    #endregion
                    #region
                    else
                    {
                        #region set document path to seprate variable
                        string strPath = objEnquirydtls.docPath;
                        #endregion
                        #region get list of snq items for ml checking
                        var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                       select new
                                       {
                                           part.PART_NAME,
                                           part.PART_CODE
                                       }).Distinct().ToList();
                        #endregion
                        #region
                        T_MESPAS_ENQUIRY_HDR objenqheader = new T_MESPAS_ENQUIRY_HDR();
                        objenqheader.FK_PROCESS_ID = 1;
                        objenqheader.FK_INSTANCE_ID = 1;
                        objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                        objenqheader.OWNER = objenquiry.GetCustomerMapping(objEnquirydtls.owner);
                        objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                        objenqheader.SHIP_NAME = objenquiry.GetShipName(objEnquirydtls.shipName);
                        objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                        objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMESPASDocumentFolder"] + "" + Path.GetFileName(strPath);
                        objenqheader.STATUS = 5;
                        objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                        objenqheader.CREATED_BY = 1;
                        objenqheader.CREATED_DATE = DateTime.Now;
                        objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                        if (objEnquirydtls.maker == "")
                        {
                            objenqheader.MAKER = MakerBlank;
                        }
                        else
                        {
                            objenqheader.MAKER = objenquiry.GetManufacturingMapping(objEnquirydtls.maker);
                        }
                        objenqheader.TYPE = objEnquirydtls.type;
                        objenqheader.EQUIPMENT = objEnquirydtls.equipment;
                        objenqheader.SERIAL_NO = objEnquirydtls.serialNo;
                        objenqheader.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                        objenqheader.NET_AMOUNT = objEnquirydtls.netAmount;
                        objenqheader.EMAIL_RECEIVED_AT = objEnquirydtls.emailReceivedat;
                        objenqheader.EMAIL_PROCESSED_AT = objEnquirydtls.emailProcessedat;
                        objenqheader.IN_ERROR_AT = objEnquirydtls.inErrorat;
                        objenqheader.VERIFIED_AT = objEnquirydtls.verifiedAt;
                        objenqheader.UPDATED_AT = objEnquirydtls.updatedAt;
                        objenqheader.QUOTATION_CREATED_AT = objEnquirydtls.quotationCreatedat;
                        objenqheader.SUPPLIER_CODE = objenquiry.GetSupplierMapping(objEnquirydtls.maker, objEnquirydtls.supplierCode);
                        objenqheader.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                        objenqheader.LEAD_TIME = 0;
                        objenqheader.SAVE_AS_DRAFT = "";
                        objenqheader.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                        objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                        objenqheader.EMAIL_FROM = objEnquirydtls.emailFrom;
                        objenqheader.MAKER_INFO = objEnquirydtls.makerInfo;
                        objenqheader.PORT = objEnquirydtls.port;
                        objenqheader.MAPPING_PORT = objEnquirydtls.mappingPort;
                        objenqheader.SOURCE_TYPE = objEnquirydtls.sourceType;
                        objenqheader.EVENT_ID = objEnquirydtls.eventId;
                        _datacontext.T_MESPAS_ENQUIRY_HDRTable.Add(objenqheader);
                        _datacontext.SaveChanges();
                        #endregion
                        #region
                        long PKENQHDRID = objenqheader.PK_MESPASENQUIRY_HDR_ID;
                        #endregion
                        #region
                        T_MESPAS_ENQUIRY_HDR_ORG objenqheaderorg = new T_MESPAS_ENQUIRY_HDR_ORG();
                        objenqheaderorg.FK_PROCESS_ID = 1;
                        objenqheaderorg.FK_INSTANCE_ID = 1;
                        objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                        objenqheaderorg.OWNER = objEnquirydtls.owner;
                        objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                        objenqheaderorg.SHIP_NAME = objenquiry.GetShipName(objEnquirydtls.shipName);
                        objenqheaderorg.ENQREF_NO = objEnquirydtls.enqrefNo;
                        objenqheaderorg.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMSDDocumentFolder"] + "" + Path.GetFileName(strPath);
                        objenqheaderorg.CREATED_BY = 1;
                        objenqheaderorg.CREATED_DATE = DateTime.Now;
                        objenqheaderorg.ERROR_CODE = objEnquirydtls.errorCode;
                        objenqheaderorg.MAKER = objEnquirydtls.maker;
                        objenqheaderorg.TYPE = objEnquirydtls.type;
                        objenqheaderorg.EQUIPMENT = objEnquirydtls.equipment;
                        objenqheaderorg.SERIAL_NO = objEnquirydtls.serialNo;
                        objenqheaderorg.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                        objenqheaderorg.NET_AMOUNT = objEnquirydtls.netAmount;
                        objenqheaderorg.RFQ_URL = objEnquirydtls.rfqUrl;
                        objenqheaderorg.MAKER_INFO = objEnquirydtls.makerInfo;
                        objenqheaderorg.SOURCE_TYPE = objEnquirydtls.sourceType;
                        _datacontext.T_MESPAS_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                        _datacontext.SaveChanges();
                        #endregion
                        #region
                        long PKENQHDRORGID = objenqheader.PK_MESPASENQUIRY_HDR_ID;
                        #endregion
                        #region
                        if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                        {
                            foreach (var item in objEnquirydtls.docHdrDetails)
                            {
                                #region stored header level documnet
                                T_MESPAS_DOCUMENT_HDR objenqdetails = new T_MESPAS_DOCUMENT_HDR();
                                {
                                    objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                    objenqdetails.DOC_PATH = item.docPath;
                                    objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                    objenqdetails.CREATEBY = 1;
                                    objenqdetails.CREATEDAT = DateTime.Now;
                                    objenqdetails.ISACTIVE = 1;
                                };
                                _datacontext.T_MESPAS_DOCUMENT_HDRTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                                #endregion
                            }
                        }
                        #endregion
                        if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                        {
                            #region
                            foreach (var item in objEnquirydtls.itemDetails)
                            {
                                string[] cost = item.cost.Split(".00");
                                T_MESPAS_ENQUIRY_DTL_ORG objenqdetailsorg = new T_MESPAS_ENQUIRY_DTL_ORG();
                                {
                                    objenqdetailsorg.FK_MESPASENQUIRY_HDR_ID = PKENQHDRORGID;
                                    objenqdetailsorg.PART_CODE = item.partCode;
                                    objenqdetailsorg.PART_NAME = item.partName;
                                    objenqdetailsorg.QUANTITY = item.quantity.ToString();
                                    objenqdetailsorg.UNIT = item.unit;
                                    objenqdetailsorg.PRICE = item.price;
                                    objenqdetailsorg.COST = cost[0];
                                    objenqdetailsorg.UPDATED_DATE = DateTime.Now.ToString();
                                    objenqdetailsorg.SUPPLIER = item.supplier;
                                    objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                    objenqdetailsorg.ERROR_CODE = item.errorCode;
                                    objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                    objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                };
                                _datacontext.T_MESPAS_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                _datacontext.SaveChanges();
                                //ML Logic For PartCode/Impa Code
                                #region ML Logic to find IMPA code
                                string partCode = item.partCode;
                                string partName = item.partName;
                                if (MLLogicMESPAS == "YES")
                                {
                                    if ((partCode == null || partCode.Length != 6) && !string.IsNullOrEmpty(partName))
                                    {
                                        double percentage = 0;
                                        string matchingPercent = this._Configuration.GetSection("MatchingPercent")["MATCHINGVALUE"];
                                        for (int counter = 0; counter < lstpart.Count; counter++)
                                        {
                                            var lcs = partName.LongestCommonSubsequence(lstpart[counter].PART_NAME);
                                            if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2 && partCode.Length <= lstpart[counter].PART_CODE.Length)
                                            {
                                                percentage = lcs.Item2;
                                                partCode = lstpart[counter].PART_CODE;
                                                isUpdatedWithML = 1;
                                            }
                                        }
                                    }
                                }
                                #endregion ML Logic to find IMPA code
                                //ML Logic For PartCode/Impa Code
                                T_MESPAS_ENQUIRY_DTL objenqdetails = new T_MESPAS_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_MESPASENQUIRY_HDR_ID = PKENQHDRID;
                                    objenqdetails.PART_CODE = item.partCode;
                                    objenqdetails.PART_NAME = item.partName;
                                    objenqdetails.QUANTITY = item.quantity.ToString();
                                    objenqdetails.UNIT = objenquiry.GetUnitMapping(item.unit);
                                    objenqdetails.PRICE = item.price;
                                    objenqdetails.COST = cost[0];
                                    objenqdetails.UPDATED_DATE = Convert.ToString(DateTime.Now);
                                    objenqdetails.SUPPLIER = item.supplier;
                                    objenqdetails.STATUS = 5;
                                    objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                    objenqdetails.ERROR_CODE = item.errorCode;
                                    objenqdetails.ACCOUNT_NO = item.accountNo;
                                    objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                    objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                    objenqdetails.LEAD_TIME = 0;
                                    objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                };
                                _datacontext.T_MESPAS_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                                long PKDTLID = objenqdetails.PK_MESPASENQUIRY_DTL_ID;
                                if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                {
                                    if (item.docdtlDetails != null)
                                    {
                                        foreach (var item2 in item.docdtlDetails)
                                        {
                                            T_MESPAS_DOCUMENT_DTL objdocdtl = new T_MESPAS_DOCUMENT_DTL();
                                            {
                                                objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                                objdocdtl.DOC_PATH = item2.docPath;
                                                objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                                objdocdtl.CREATEBY = 1;
                                                objdocdtl.CREATEDAT = DateTime.Now;
                                                objdocdtl.ISACTIVE = 1;
                                            };
                                            _datacontext.T_MESPAS_DOCUMENT_DTLTable.Add(objdocdtl);
                                            _datacontext.SaveChanges();
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        obj.result = "Enquiry Saved Successfully";
                        _hub.Clients.All.BroadcastMessage();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
            }
            return obj;
        }
        #endregion
        #region get depart list i.e msd,snq
        public List<Department> GetDeptData()
        {
            #region created object of list
            List<Department> lstDepartment = new List<Department>();
            #endregion
            #region
            try
            {
                #region select detpart list from dept table
                var DepartmentData = (from p in _datacontext.M_DEPTTable
                                      where p.ISACTIVE == 1
                                      select new
                                      {
                                          p.PK_DEPT_ID,
                                          p.DEPT_NAME
                                      }).ToList();
                #endregion
                #region heck dept list not null
                if (DepartmentData != null)
                {
                    foreach (var data in DepartmentData)
                    {
                        #region add dept data in list
                        Department objdata = new Department();
                        objdata.deptId = data.PK_DEPT_ID;
                        objdata.deptName = data.DEPT_NAME;
                        lstDepartment.Add(objdata);
                        #endregion
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
            }
            #endregion
            return lstDepartment;
        }
        #endregion
        #region send notification to client of flogic team for duplicate RFQ recived
        public void SendDuplicateEnquiryNotification(string enqNo)
        {
            try
            {
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                #region set from ,to or cc email id
                mail.From = new MailAddress(this._Configuration.GetSection("DuplicateEnquiryMESPAS")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("DuplicateEnquiryMESPAS")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("DuplicateEnquiryMESPAS")["CCMail"]);
                #endregion
                #region set subject or body of the notification
                if (enqNo != "")
                {
                    mail.Subject = "Duplicate Enquiry Received";
                    mail.Body = "<p>Hello Team,<br /><br />Enquiry with reference number:- " + enqNo + " was already processed,this is duplicate enquiry received.<br/><br/>" +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                #endregion
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                #region set credetials od account
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("DuplicateEnquiryMESPAS")["Username"], this._Configuration.GetSection("DuplicateEnquiryMESPAS")["Password"]);
                #endregion
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region
        public Enquiryheader GetEnquiryDetails(long fkenquiryid)
        {
            Enquiryheader lstEnqhdrDetails = new Enquiryheader();
            var HdrData = (from enquiryhdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where enquiryhdr.PK_MESPASENQUIRY_HDR_ID == fkenquiryid select enquiryhdr).ToList();
            if (HdrData.Count > 0)
            {
                var EnquiryHdrdata = HdrData.First();
                if (EnquiryHdrdata.STATUS == 1)
                {
                    string[] enqdate = EnquiryHdrdata.ENQUIRY_DATE.Split(" ");
                    lstEnqhdrDetails = new Enquiryheader
                    {
                        PK_MESPASENQUIRY_HDR_ID = EnquiryHdrdata.PK_MESPASENQUIRY_HDR_ID,
                        enquiryDate = enqdate[0],
                        enqrefNo = EnquiryHdrdata.ENQREF_NO,
                        shipName = EnquiryHdrdata.SHIP_NAME,
                        owner = EnquiryHdrdata.OWNER,
                        ownerEmailid = EnquiryHdrdata.OWNER_EMAILID,
                        status = (from statusmst in _datacontext.M_STATUS_CODE where EnquiryHdrdata.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                        quotationNo = EnquiryHdrdata.QUOTATION_NO,
                        docPath = EnquiryHdrdata.DOC_PATH,
                        errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where EnquiryHdrdata.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                        maker = EnquiryHdrdata.MAKER,
                        type = EnquiryHdrdata.TYPE,
                        equipment = EnquiryHdrdata.EQUIPMENT,
                        serialNo = EnquiryHdrdata.SERIAL_NO,
                        leadTime = Convert.ToInt32(EnquiryHdrdata.LEAD_TIME),
                        leadTimeperiod = EnquiryHdrdata.LEAD_TIME_PERIOD,
                        leadTimeforitem = EnquiryHdrdata.LEAD_TIME_FOR_ITEM,
                        emailReceivedat = Convert.ToDateTime(EnquiryHdrdata.EMAIL_RECEIVED_AT).ToString("MM/dd/yyyy hh:mm tt"),
                        emailFrom = EnquiryHdrdata.EMAIL_FROM,
                        sourceType = EnquiryHdrdata.SOURCE_TYPE,
                        port = EnquiryHdrdata.PORT,
                        mappingPort = EnquiryHdrdata.MAPPING_PORT,
                        eventId = EnquiryHdrdata.EVENT_ID,
                        rfqUrl = EnquiryHdrdata.RFQ_URL,
                    };
                    List<EnquiryHeaderDocDetails> lstHdrdoc = new List<EnquiryHeaderDocDetails>();
                    var docdata = (from doc in _datacontext.T_MESPAS_DOCUMENT_HDRTable
                                   where doc.ENQUIRY_HDR_ID == lstEnqhdrDetails.PK_MESPASENQUIRY_HDR_ID
                                   select new
                                   {
                                       doc.DOC_ID,
                                       doc.ENQUIRY_HDR_ID,
                                       doc.DOC_PATH,
                                       doc.ERROR_DESCRIPTION,
                                   }).ToList();
                    if (docdata.Count > 0)
                    {
                        foreach (var docitem in docdata)
                        {
                            EnquiryHeaderDocDetails objdoc = new EnquiryHeaderDocDetails();
                            objdoc.docId = docitem.DOC_ID;
                            objdoc.enquiryHdrId = docitem.ENQUIRY_HDR_ID;
                            objdoc.docPath = docitem.DOC_PATH;
                            objdoc.errorDescription = docitem.ERROR_DESCRIPTION;
                            lstHdrdoc.Add(objdoc);
                        }
                        lstEnqhdrDetails.docHdrDetails = lstHdrdoc;
                    }
                    var Dtldata = (from enquirydtl in _datacontext.T_MESPAS_ENQUIRY_DTLTable
                                   where enquirydtl.FK_MESPASENQUIRY_HDR_ID == lstEnqhdrDetails.PK_MESPASENQUIRY_HDR_ID
                                   select new
                                   {
                                       enquirydtl.PK_MESPASENQUIRY_DTL_ID,
                                       enquirydtl.FK_MESPASENQUIRY_HDR_ID,
                                       enquirydtl.PART_CODE,
                                       enquirydtl.PART_NAME,
                                       enquirydtl.QUANTITY,
                                       enquirydtl.UNIT,
                                       enquirydtl.PRICE,
                                       enquirydtl.COST,
                                       enquirydtl.STATUS,
                                       enquirydtl.SEQ_NO,
                                       enquirydtl.SEQ_NOTEXT,
                                       enquirydtl.ERROR_CODE,
                                       enquirydtl.ACCOUNT_NO,
                                       enquirydtl.ACCOUNT_DESCRIPTION,
                                       enquirydtl.LEAD_TIME,
                                       enquirydtl.LEAD_TIME_PERIOD
                                   });
                    Dtldata = Dtldata.Where(x => x.STATUS == 1);
                    var objdtlData = Dtldata.ToList();
                    List<EnquiryDetails> lstClsEnquiryDtl = new List<EnquiryDetails>();
                    if (objdtlData.Count > 0)
                    {
                        foreach (var item in objdtlData)
                        {
                            EnquiryDetails objEnqDetails = new EnquiryDetails
                            {
                                PK_MESPASENQUIRY_DTL_ID = item.PK_MESPASENQUIRY_DTL_ID,
                                fkEnquiryid = item.FK_MESPASENQUIRY_HDR_ID,
                                partCode = item.PART_CODE,
                                partName = item.PART_NAME,
                                quantity = item.QUANTITY,
                                unit = item.UNIT,
                                price = Convert.ToString(item.PRICE),
                                cost = item.COST,
                                status = (from statusmst in _datacontext.M_STATUS_CODE where item.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                                seqNo = item.SEQ_NO.ToString(),
                                seqNoText = item.SEQ_NOTEXT,
                                errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where item.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                                accountNo = item.ACCOUNT_NO,
                                accountDescription = item.ACCOUNT_DESCRIPTION,
                                isSelected = true,
                                leadTime = Convert.ToInt32(item.LEAD_TIME),
                                leadTimeperiod = item.LEAD_TIME_PERIOD
                            };
                            lstClsEnquiryDtl.Add(objEnqDetails);
                            List<EnquiryDetailDocDetails> lstdtldoc = new List<EnquiryDetailDocDetails>();
                            var docdtldata = (from doc in _datacontext.T_MESPAS_DOCUMENT_DTLTable
                                              where doc.ENQUIRY_DTL_ID == item.PK_MESPASENQUIRY_DTL_ID
                                              select new
                                              {
                                                  doc.DOC_ID,
                                                  doc.ENQUIRY_DTL_ID,
                                                  doc.DOC_PATH,
                                                  doc.ERROR_DESCRIPTION,
                                              }).ToList();
                            if (docdtldata.Count > 0)
                            {
                                foreach (var docitem in docdtldata)
                                {
                                    EnquiryDetailDocDetails objdoc = new EnquiryDetailDocDetails();
                                    objdoc.docId = docitem.DOC_ID;
                                    objdoc.enquiryDtlId = docitem.ENQUIRY_DTL_ID;
                                    objdoc.docPath = docitem.DOC_PATH;
                                    objdoc.errorDescription = docitem.ERROR_DESCRIPTION;
                                    lstdtldoc.Add(objdoc);
                                }
                                objEnqDetails.docdtlDetails = lstdtldoc;
                            }
                        }
                        lstEnqhdrDetails.itemDetails = lstClsEnquiryDtl;
                    }
                }
            }
            return lstEnqhdrDetails;
        }
        #endregion
        #region
        //Get Items details Count for Error
        public int GetDetailCountforError(long PK_MESPASENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.T_MESPAS_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MESPASENQUIRY_HDR_ID == PK_MESPASENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        #endregion
        #region
        //Items Details Count for Inprocess,Not Started
        public int GetDetailCountforOther(long PK_MESPASENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.T_MESPAS_ENQUIRY_DTLTable.Count(t => t.FK_MESPASENQUIRY_HDR_ID == PK_MESPASENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        #endregion
        #region get rfq data by status
        public List<Enquiryheaderdata> GetEnquiryHeader(int status)
        {
            #region created object of list
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            #endregion
            #region select record form tables by status
            var EnqHeaderdata = (from enqheader in _datacontext.T_MESPAS_ENQUIRY_HDRTable
                                 join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                 join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                 from userm in usmt.DefaultIfEmpty()
                                 where enqheader.STATUS == status
                                 select new
                                 {
                                     enqheader.PK_MESPASENQUIRY_HDR_ID,
                                     enqheader.ENQUIRY_DATE,
                                     enqheader.ENQREF_NO,
                                     enqheader.SHIP_NAME,
                                     enqheader.OWNER,
                                     enqheader.OWNER_EMAILID,
                                     statusmst.STATUS_DESCRIPTION,
                                     enqheader.QUOTATION_NO,
                                     enqheader.DOC_PATH,
                                     enqheader.CREATED_DATE,
                                     enqheader.QUOTATION_CREATED_AT,
                                     enqheader.EMAIL_PROCESSED_AT,
                                     enqheader.OWNERSHIP,
                                     enqheader.SAVE_AS_DRAFT,
                                     enqheader.SOURCE_TYPE,
                                     userm.LOGIN_NAME,
                                     userm.USER_NAME
                                 }).OrderByDescending(c => c.PK_MESPASENQUIRY_HDR_ID).ToList().ToList();
            #endregion
            if (EnqHeaderdata.Count > 0)
            {
                foreach (var data in EnqHeaderdata)
                {
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    #region split date by requirnement wise
                    if (!string.IsNullOrEmpty(data.ENQUIRY_DATE))
                    {
                        if (data.ENQUIRY_DATE.Contains("/"))
                        {
                            string[] dt;
                            dt = data.ENQUIRY_DATE.Split(" ");
                            objEnqheadr.enquiryDate = dt[0];
                        }
                        else
                        {
                            DateTime dt1 = Convert.ToDateTime(data.ENQUIRY_DATE);
                            objEnqheadr.enquiryDate = dt1.ToString("MM/dd/yyyy");
                        }
                    }
                    else
                    {
                        objEnqheadr.enquiryDate = data.ENQUIRY_DATE;
                    }
                    #endregion
                    #region
                    objEnqheadr.PK_MESPASENQUIRY_HDR_ID = data.PK_MESPASENQUIRY_HDR_ID;
                    objEnqheadr.enqrefNo = data.ENQREF_NO;
                    objEnqheadr.shipName = data.SHIP_NAME;
                    objEnqheadr.owner = data.OWNER;
                    objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                    objEnqheadr.status = data.STATUS_DESCRIPTION;
                    objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                    objEnqheadr.sourceType = data.SOURCE_TYPE;
                    #region calculateduration for success RFQ's
                    if (data.STATUS_DESCRIPTION == "Success")
                    {
                        DateTime date1 = data.CREATED_DATE;
                        DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                        TimeSpan duration = date2.Subtract(date1);
                        objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                    }
                    #endregion
                    objEnqheadr.quotationNo = data.QUOTATION_NO;
                    objEnqheadr.docPath = data.DOC_PATH;
                    #region count of error items
                    objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_MESPASENQUIRY_HDR_ID);
                    #endregion
                    #region count of total items
                    objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_MESPASENQUIRY_HDR_ID);
                    #endregion
                    #region take ownership ofparticular RFQ
                    if (data.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        objEnqheadr.loginId = data.LOGIN_NAME;
                        objEnqheadr.userName = data.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    #endregion
                    lstEnqheader.Add(objEnqheadr);
                    #endregion
                }
            }
            return lstEnqheader;
        }
        #endregion
        #region get pending rfq list by filter wise
        public List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata)
        {
            #region created object of list class
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            #endregion
            if (objsearchdata != null)
            {
                #region select pedning rfq's list
                var Tasklist = (from enqheader in _datacontext.T_MESPAS_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)notstartedStatus
                                select new
                                {
                                    enqheader.PK_MESPASENQUIRY_HDR_ID,
                                    enquirydt = enqheader.ENQUIRY_DATE == null ? "" : enqheader.ENQUIRY_DATE,
                                    enqheader.ENQREF_NO,
                                    enqheader.SHIP_NAME,
                                    enqheader.OWNER,
                                    statusmst.STATUS_DESCRIPTION,
                                    enqheader.QUOTATION_NO,
                                    enqheader.CREATED_DATE,
                                    enqheader.QUOTATION_CREATED_AT,
                                    enqheader.EMAIL_PROCESSED_AT,
                                    enqheader.OWNERSHIP,
                                    enqheader.SOURCE_TYPE,
                                    enqheader.SAVE_AS_DRAFT,
                                    DETAIL_COUNT = _datacontext.T_MESPAS_ENQUIRY_DTLTable.Count(t => t.FK_MESPASENQUIRY_HDR_ID == enqheader.PK_MESPASENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.T_MESPAS_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MESPASENQUIRY_HDR_ID == enqheader.PK_MESPASENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                #endregion
                #region get rfq's by owner name,enq no & ship name.
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo);
                }
                #endregion
                #region get rfq's byfrom or to date.
                if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                {
                    DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                    Tasklist = Tasklist.Where(x => x.CREATED_DATE.Date == fromDt.Date);
                }
                else if (objsearchdata.FromDate != "" && objsearchdata.ToDate != "")
                {
                    DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                    DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                    if (objsearchdata.FromDate != "")
                    {
                        Tasklist = Tasklist.Where(x => x.CREATED_DATE.Date >= fromDt);
                    }
                    if (objsearchdata.ToDate != "")
                    {
                        Tasklist = Tasklist.Where(x => x.CREATED_DATE.Date <= toDt);
                    }
                }
                #endregion
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.enquirydt.Split(" ");
                    #region set data to variable and added in list
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MESPASENQUIRY_HDR_ID = item.PK_MESPASENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.saveAsDraft = item.SAVE_AS_DRAFT;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    #endregion
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.createdDate).ToList();
            }
            else
            {
                #region select pedning rfq's list
                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)notstartedStatus
                                select new
                                {
                                    enqheader.PK_MSDENQUIRY_HDR_ID,
                                    enqheader.ENQUIRY_DATE,
                                    enqheader.ENQREF_NO,
                                    enqheader.SHIP_NAME,
                                    enqheader.OWNER,
                                    enqheader.OWNER_EMAILID,
                                    statusmst.STATUS_DESCRIPTION,
                                    enqheader.QUOTATION_NO,
                                    enqheader.DOC_PATH,
                                    enqheader.CREATED_DATE,
                                    enqheader.QUOTATION_CREATED_AT,
                                    enqheader.EMAIL_PROCESSED_AT,
                                    enqheader.OWNERSHIP,
                                    enqheader.SOURCE_TYPE,
                                    DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                #endregion
                #region get rfq's by owner name,enq no & ship name.
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo);
                }
                #endregion
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.ENQUIRY_DATE.Split(" ");
                    #region
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MESPASENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.ownerEmailid = item.OWNER_EMAILID;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.docPath = item.DOC_PATH;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    #endregion
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.createdDate).ToList();
            }
        }
        #endregion
        #region delete RFQ's unrequired items from portal
        public Message DeleteItems(List<EnquiryDetails> objEnquirydtls)
        {
            #region created object of details  class
            List<EnquiryDetails> EnqDetails = new List<EnquiryDetails>();
            #endregion
            #region retrun result message
            Message obj = new Message();
            obj.result = "Error while Deleting Enquiry";
            #endregion
            #region
            try
            {
                foreach (var item in objEnquirydtls)
                {
                    #region select which items is delete 
                    T_MESPAS_ENQUIRY_DTL objMsdDtl = (from dtl in _datacontext.T_MESPAS_ENQUIRY_DTLTable
                                                      where dtl.PK_MESPASENQUIRY_DTL_ID == item.PK_MESPASENQUIRY_DTL_ID
                                                      select dtl).ToList().SingleOrDefault();
                    if (objMsdDtl != null)
                    {
                        _datacontext.T_MESPAS_ENQUIRY_DTLTable.Remove(objMsdDtl);
                        _datacontext.SaveChanges();
                    }
                    #endregion
                }
                obj.result = "Data deleted successfully";
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            #endregion
            return obj;
        }
        #endregion
        #region check this rfq is for MSD dept or SNQ dept.
        public Message UpdateEnquiry(Enquiryheader objEnquirydtls)
        {
            #region created object of retun result
            Message obj = new Message();
            obj.result = "Error while Updating Enquiry";
            #endregion
            try
            {
                #region if user select msd dept then RFQ' goes to msd dept
                if (objEnquirydtls != null && objEnquirydtls.deptName == "MSD")
                {
                    #region set data to tables.
                    TMSD_ENQUIRY_HDR objenqheader = new TMSD_ENQUIRY_HDR();
                    objenqheader.FK_PROCESS_ID = 1;
                    objenqheader.FK_INSTANCE_ID = 1;
                    objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                    objenqheader.OWNER = objEnquirydtls.owner;
                    objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                    objenqheader.SHIP_NAME = objEnquirydtls.shipName;
                    objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                    objenqheader.DOC_PATH = objEnquirydtls.docPath;
                    objenqheader.STATUS = Convert.ToInt32(objEnquirydtls.status);
                    objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                    objenqheader.CREATED_BY = 1;
                    objenqheader.CREATED_DATE = DateTime.Now;
                    objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                    objenqheader.MAKER = objEnquirydtls.maker;
                    objenqheader.TYPE = objEnquirydtls.type;
                    objenqheader.EQUIPMENT = objEnquirydtls.equipment;
                    objenqheader.SERIAL_NO = objEnquirydtls.serialNo;
                    objenqheader.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                    objenqheader.NET_AMOUNT = objEnquirydtls.netAmount;
                    objenqheader.EMAIL_RECEIVED_AT = objEnquirydtls.emailReceivedat;
                    objenqheader.EMAIL_PROCESSED_AT = objEnquirydtls.emailProcessedat;
                    objenqheader.IN_ERROR_AT = objEnquirydtls.inErrorat;
                    objenqheader.VERIFIED_AT = objEnquirydtls.verifiedAt;
                    objenqheader.UPDATED_AT = objEnquirydtls.updatedAt;
                    objenqheader.QUOTATION_CREATED_AT = objEnquirydtls.quotationCreatedat;
                    objenqheader.SUPPLIER_CODE = objEnquirydtls.supplierCode;
                    objenqheader.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                    objenqheader.LEAD_TIME = 0;
                    objenqheader.SAVE_AS_DRAFT = "";
                    objenqheader.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                    objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                    objenqheader.EMAIL_FROM = objEnquirydtls.emailFrom;
                    objenqheader.MAKER_INFO = objEnquirydtls.makerInfo;
                    objenqheader.SOURCE_TYPE = "MESPAS";
                    objenqheader.SAVE_AS_DRAFT = "";
                    objenqheader.VERIFIED_AT = DateTime.Now.ToString();
                    objenqheader.VERIFIED_BY = objEnquirydtls.verifiedBy;
                    objenqheader.EVENT_ID = objEnquirydtls.eventId;
                    objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                    objenqheader.ERROR_VERIFIED_COUNTER = 0;
                    _datacontext.TMSD_ENQUIRY_HDRTable.Add(objenqheader);
                    _datacontext.SaveChanges();
                    #endregion
                    #region get id from header table
                    long PKENQHDRID = objenqheader.PK_MSDENQUIRY_HDR_ID;
                    #endregion
                    #region 
                    TMSD_ENQUIRY_HDR_ORG objenqheaderorg = new TMSD_ENQUIRY_HDR_ORG();
                    objenqheaderorg.FK_PROCESS_ID = 1;
                    objenqheaderorg.FK_INSTANCE_ID = 1;
                    objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                    objenqheaderorg.OWNER = objEnquirydtls.owner;
                    objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                    objenqheaderorg.SHIP_NAME = (objEnquirydtls.shipName);
                    objenqheaderorg.ENQREF_NO = objEnquirydtls.enqrefNo;
                    objenqheaderorg.DOC_PATH = objEnquirydtls.docPath;
                    objenqheaderorg.CREATED_BY = 1;
                    objenqheaderorg.CREATED_DATE = DateTime.Now;
                    objenqheaderorg.ERROR_CODE = objEnquirydtls.errorCode;
                    objenqheaderorg.MAKER = objEnquirydtls.maker;
                    objenqheaderorg.TYPE = objEnquirydtls.type;
                    objenqheaderorg.EQUIPMENT = objEnquirydtls.equipment;
                    objenqheaderorg.SERIAL_NO = objEnquirydtls.serialNo;
                    objenqheaderorg.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                    objenqheaderorg.NET_AMOUNT = objEnquirydtls.netAmount;
                    objenqheaderorg.RFQ_URL = objEnquirydtls.rfqUrl;
                    objenqheaderorg.MAKER_INFO = objEnquirydtls.makerInfo;
                    objenqheaderorg.SOURCE_TYPE = "MESPAS";
                    _datacontext.TMSD_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                    _datacontext.SaveChanges();
                    #endregion
                    #region get id from header
                    long PKENQHDRORGID = objenqheader.PK_MSDENQUIRY_HDR_ID;
                    #endregion
                    #region
                    if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                    {
                        foreach (var item in objEnquirydtls.docHdrDetails)
                        {
                            #region set data to tables
                            T_DOCUMENT_HDR objenqdetails = new T_DOCUMENT_HDR();
                                {
                                    objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                    objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                    objenqdetails.DOC_PATH = item.docPath;
                                    objenqdetails.CREATEBY = 1;
                                    objenqdetails.CREATEDAT = DateTime.Now;
                                    objenqdetails.ISACTIVE = 1;
                                };
                                _datacontext.T_DOCUMENT_HDRTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                        #endregion
                    }
                    #endregion
                    #region
                    if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                    {
                        foreach (var item in objEnquirydtls.itemDetails)
                        {
                            #region set data to msd enquiry org tbl
                            TMSD_ENQUIRY_DTL_ORG objenqdetailsorg = new TMSD_ENQUIRY_DTL_ORG();
                            {
                                objenqdetailsorg.FK_MSDENQUIRY_HDR_ID = PKENQHDRORGID;
                                objenqdetailsorg.PART_CODE = item.partCode;
                                objenqdetailsorg.PART_NAME = item.partName;
                                objenqdetailsorg.QUANTITY = item.quantity.ToString();
                                objenqdetailsorg.UNIT = item.unit;
                                objenqdetailsorg.PRICE = item.price;
                                objenqdetailsorg.COST = item.cost;
                                objenqdetailsorg.UPDATED_DATE = DateTime.Now;
                                objenqdetailsorg.SUPPLIER = item.supplier;
                                objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objenqdetailsorg.ERROR_CODE = item.errorCode;
                                objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                            };
                            _datacontext.TMSD_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                            _datacontext.SaveChanges();
                            #endregion
                            #region set data to msd enquiry org tbl
                            TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                            {
                                objenqdetails.FK_MSDENQUIRY_HDR_ID = PKENQHDRID;
                                objenqdetails.PART_CODE = item.partCode;
                                objenqdetails.PART_NAME = item.partName;
                                objenqdetails.QUANTITY = item.quantity.ToString();
                                objenqdetails.UNIT = item.unit;
                                objenqdetails.PRICE = item.price;
                                objenqdetails.COST = item.cost;
                                objenqdetails.UPDATED_DATE = DateTime.Now;
                                objenqdetails.SUPPLIER = item.supplier;
                                objenqdetails.STATUS = Convert.ToInt32(item.status);
                                objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objenqdetails.ERROR_CODE = item.errorCode;
                                objenqdetails.ACCOUNT_NO = item.accountNo;
                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                objenqdetails.LEAD_TIME = 0;
                                objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                            };
                            _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                            _datacontext.SaveChanges();
                            #endregion
                            #region get dtl id from dtl table
                            long PKDTLID = objenqdetails.PK_MSDENQUIRY_DTL_ID;
                            #endregion
                            #region  document dtls
                            if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                            {
                                if (item.docdtlDetails != null)
                                {
                                    foreach (var item2 in item.docdtlDetails)
                                    {
                                        #region
                                        T_DOCUMENT_DTL objdocdtl = new T_DOCUMENT_DTL();
                                        {
                                            objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                            objdocdtl.DOC_PATH = item2.docPath;
                                            objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                            objdocdtl.CREATEBY = 1;
                                            objdocdtl.CREATEDAT = DateTime.Now;
                                            objdocdtl.ISACTIVE = 1;
                                        };
                                        _datacontext.T_DOCUMENT_DTLTable.Add(objdocdtl);
                                        _datacontext.SaveChanges();
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                    #region checked rfqs status 2 send to data to msd inprocess page
                    if (objenqheader.STATUS == 2)
                    {
                        obj.result = "Enquiry Verified sucessfully.";
                        #region
                        T_MESPAS_ENQUIRY_HDR headerobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.PK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (headerobj != null)
                        {
                            headerobj.STATUS = 2;
                            _datacontext.SaveChanges();
                        }
                        T_MESPAS_ENQUIRY_DTL dtlobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_DTLTable where hdr.FK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (headerobj != null)
                        {
                            dtlobj.STATUS = 2;
                            _datacontext.SaveChanges();
                        }
                        #endregion
                        _hub.Clients.All.BroadcastMessage();
                    }
                    #endregion
                }
                #endregion
                #endregion
                #region if user select snq dept then RFQ' goes to snq dept
                else if (objEnquirydtls != null && objEnquirydtls.deptName == "SNQ")
                {
                    TSNQ_ENQUIRY_HDR objenqheader = new TSNQ_ENQUIRY_HDR();
                    objenqheader.FK_PROCESS_ID = 1;
                    objenqheader.FK_INSTANCE_ID = 1;
                    objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                    objenqheader.OWNER = objEnquirydtls.owner;
                    objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                    objenqheader.SHIP_NAME = objEnquirydtls.shipName;
                    objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                    objenqheader.DOC_PATH = objEnquirydtls.docPath;
                    objenqheader.STATUS = Convert.ToInt32(objEnquirydtls.status);
                    objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                    objenqheader.CREATED_BY = 1;
                    objenqheader.CREATED_DATE = DateTime.Now;
                    objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                    objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                    objenqheader.MAKER = objEnquirydtls.maker;
                    objenqheader.TYPE = objEnquirydtls.type;
                    objenqheader.EQUIPMENT = objEnquirydtls.equipment;
                    objenqheader.SERIAL_NO = objEnquirydtls.serialNo;
                    objenqheader.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                    objenqheader.NET_AMOUNT = objEnquirydtls.netAmount;
                    objenqheader.EMAIL_RECEIVED_AT = objEnquirydtls.emailReceivedat;
                    objenqheader.EMAIL_PROCESSED_AT = objEnquirydtls.emailProcessedat;
                    objenqheader.IN_ERROR_AT = objEnquirydtls.inErrorat;
                    objenqheader.VERIFIED_AT = objEnquirydtls.verifiedAt;
                    objenqheader.UPDATED_AT = objEnquirydtls.updatedAt;
                    objenqheader.QUOTATION_CREATED_AT = objEnquirydtls.quotationCreatedat;
                    objenqheader.PORT = objEnquirydtls.port;
                    objenqheader.AS400_MAPPING_PORT = objEnquirydtls.mappingPort;
                    objenqheader.EVENT_ID = objEnquirydtls.eventId;
                    objenqheader.SAVE_AS_DRAFT = "";
                    objenqheader.SOURCE_TYPE = "MESPAS";
                    objenqheader.SAVE_AS_DRAFT = "";
                    objenqheader.VERIFIED_AT = DateTime.Now.ToString();
                    objenqheader.VERIFIED_BY = objEnquirydtls.verifiedBy;
                    _datacontext.TSNQ_ENQUIRY_HDRTable.Add(objenqheader);
                    _datacontext.SaveChanges();
                    long PKENQHDRID = objenqheader.PK_SNQENQUIRY_HDR_ID;
                    TSNQ_ENQUIRY_HDR_ORG objenqheaderorg = new TSNQ_ENQUIRY_HDR_ORG();
                    objenqheaderorg.FK_PROCESS_ID = 1;
                    objenqheaderorg.FK_INSTANCE_ID = 1;
                    objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                    objenqheaderorg.OWNER = objEnquirydtls.owner;
                    objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                    objenqheaderorg.SHIP_NAME = (objEnquirydtls.shipName);
                    objenqheaderorg.ENQREF_NO = objEnquirydtls.enqrefNo;
                    objenqheaderorg.DOC_PATH = objEnquirydtls.docPath;
                    objenqheaderorg.CREATED_BY = 1;
                    objenqheaderorg.CREATED_DATE = DateTime.Now;
                    objenqheaderorg.ERROR_CODE = objEnquirydtls.errorCode;
                    objenqheaderorg.MAKER = objEnquirydtls.maker;
                    objenqheaderorg.TYPE = objEnquirydtls.type;
                    objenqheaderorg.EQUIPMENT = objEnquirydtls.equipment;
                    objenqheaderorg.SERIAL_NO = objEnquirydtls.serialNo;
                    objenqheaderorg.DISCOUNT_AMOUNT = objEnquirydtls.discountAmount;
                    objenqheaderorg.NET_AMOUNT = objEnquirydtls.netAmount;
                    objenqheaderorg.SOURCE_TYPE = "MESPAS";
                    _datacontext.TSNQ_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                    _datacontext.SaveChanges();
                    #region
                    long PKENQHDRORGID = objenqheader.PK_SNQENQUIRY_HDR_ID;
                    #endregion
                    #region save multiple header levels document path
                    if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                    {
                        foreach (var item in objEnquirydtls.docHdrDetails)
                        {
                            #region set hdr documnet
                            T_SNQ_DOCUMENT_HDR objenqdetails = new T_SNQ_DOCUMENT_HDR();
                                {
                                    objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                    objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                    objenqdetails.DOC_PATH = item.docPath;
                                    objenqdetails.CREATEBY = 1;
                                    objenqdetails.CREATEDAT = DateTime.Now;
                                    objenqdetails.ISACTIVE = 1;
                                };
                                _datacontext.T_SNQ_DOCUMENT_HDRTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            #endregion
                        }
                    }
                    #endregion
                    if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                    {
                        foreach (var item in objEnquirydtls.itemDetails)
                        {
                            TSNQ_ENQUIRY_DTL_ORG objenqdetailsorg = new TSNQ_ENQUIRY_DTL_ORG();
                            {
                                objenqdetailsorg.FK_SNQENQUIRY_HDR_ID = PKENQHDRORGID;
                                objenqdetailsorg.PART_CODE = item.partCode;
                                objenqdetailsorg.PART_NAME = item.partName;
                                objenqdetailsorg.QUANTITY = item.quantity.ToString();
                                objenqdetailsorg.UNIT = item.unit;
                                objenqdetailsorg.PRICE = item.price;
                                objenqdetailsorg.COST = item.cost;
                                objenqdetailsorg.UPDATED_DATE = DateTime.Now;
                                objenqdetailsorg.SUPPLIER = item.supplier;
                                objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objenqdetailsorg.ERROR_CODE = item.errorCode;
                                objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                            };
                            _datacontext.TSNQ_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                            _datacontext.SaveChanges();
                            TSNQ_ENQUIRY_DTL objenqdetails = new TSNQ_ENQUIRY_DTL();
                            {
                                objenqdetails.FK_SNQENQUIRY_HDR_ID = PKENQHDRID;
                                objenqdetails.PART_CODE = item.partCode;
                                objenqdetails.PART_NAME = item.partName;
                                objenqdetails.QUANTITY = item.quantity.ToString();
                                objenqdetails.UNIT = item.unit;
                                objenqdetails.PRICE = item.price;
                                objenqdetails.COST = item.cost;
                                objenqdetails.UPDATED_DATE = DateTime.Now;
                                objenqdetails.SUPPLIER = item.supplier;
                                objenqdetails.STATUS = Convert.ToInt32(item.status);
                                objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objenqdetails.ERROR_CODE = item.errorCode;
                                objenqdetails.ACCOUNT_NO = item.accountNo;
                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                            };
                            _datacontext.TSNQ_ENQUIRY_DTLTable.Add(objenqdetails);
                            _datacontext.SaveChanges();
                            #region get id from snd detail tbl
                            long PKDTLID = objenqdetails.PK_SNQENQUIRY_DTL_ID;
                            #endregion
                            #region
                            if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                            {
                                if (item.docdtlDetails != null)
                                {
                                    foreach (var item2 in item.docdtlDetails)
                                    {
                                        #region set data to snqdocdtl table
                                        T_SNQ_DOCUMENT_DTL objdocdtl = new T_SNQ_DOCUMENT_DTL();
                                        {
                                            objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                            objdocdtl.DOC_PATH = item2.docPath;
                                            objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                            objdocdtl.CREATEBY = 1;
                                            objdocdtl.CREATEDAT = DateTime.Now;
                                            objdocdtl.ISACTIVE = 1;
                                        };
                                        _datacontext.T_SNQ_DOCUMENT_DTLTable.Add(objdocdtl);
                                        _datacontext.SaveChanges();
                                        #endregion
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                    #region checked rfqs status 2 send to data to snq inprocess page
                    if (objenqheader.STATUS == 2)
                    {
                        obj.result = "Enquiry Verified sucessfully.";
                        #region
                        T_MESPAS_ENQUIRY_HDR headerobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.PK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (headerobj != null)
                        {
                            headerobj.STATUS = 2;
                            _datacontext.SaveChanges();
                        }
                        T_MESPAS_ENQUIRY_DTL dtlobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_DTLTable where hdr.FK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (headerobj != null)
                        {
                            dtlobj.STATUS = 2;
                            _datacontext.SaveChanges();
                        }
                        #endregion
                        _hub.Clients.All.BroadcastMessage();
                    }
                    #endregion
                }
                else
                {
                    if (Convert.ToInt32(objEnquirydtls.status) == 8)
                    {
                        T_MESPAS_ENQUIRY_HDR headerobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.PK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (headerobj != null)
                        {
                            headerobj.STATUS = 8;
                            headerobj.REJECTED_BY = objEnquirydtls.rejectedBy;
                            _datacontext.SaveChanges();
                        }
                        T_MESPAS_ENQUIRY_DTL dtlobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_DTLTable where hdr.FK_MESPASENQUIRY_HDR_ID == objEnquirydtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                        if (dtlobj != null)
                        {
                            dtlobj.STATUS = 8;
                            _datacontext.SaveChanges();
                        }
                        obj.result = "Enquiry rejected sucessfully.";
                        _hub.Clients.All.BroadcastMessage();
                    }
                }
                return obj;
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        #endregion
        #region save as  dfraft functionality
        public Message UpdateSaveAsEnquiry(Enquiryheader objEnquiry)
        {
            #region created object of return message or result class
            Message obj = new Message();
            obj.result = "Error while Updating Enquiry";
            #endregion
            try
            {
                #region select date
                T_MESPAS_ENQUIRY_HDR headerobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.PK_MESPASENQUIRY_HDR_ID == objEnquiry.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                if (Convert.ToString(objEnquiry.leadTime) == "")
                {
                    objEnquiry.leadTime = 0;
                }
                headerobj.ENQUIRY_DATE = objEnquiry.enquiryDate;
                headerobj.OWNER = objEnquiry.owner;
                headerobj.OWNER_EMAILID = objEnquiry.ownerEmailid;
                headerobj.SHIP_NAME = objEnquiry.shipName;
                headerobj.ENQREF_NO = objEnquiry.enqrefNo;
                headerobj.QUOTATION_NO = objEnquiry.quotationNo;
                headerobj.MAKER = objEnquiry.maker;
                headerobj.TYPE = objEnquiry.type;
                headerobj.EQUIPMENT = objEnquiry.equipment;
                headerobj.SERIAL_NO = objEnquiry.serialNo;
                headerobj.DISCOUNT_AMOUNT = objEnquiry.discountAmount;
                headerobj.NET_AMOUNT = objEnquiry.netAmount;
                headerobj.LEAD_TIME_FOR_ITEM = objEnquiry.leadTimeforitem;
                headerobj.LEAD_TIME = objEnquiry.leadTime;
                headerobj.LEAD_TIME_PERIOD = objEnquiry.leadTimeperiod;
                headerobj.PORT = objEnquiry.port;
                headerobj.MAPPING_PORT = objEnquiry.mappingPort;
                headerobj.RFQ_URL = objEnquiry.rfqUrl;
                if (headerobj.STATUS == 1 && objEnquiry.saveAsDraft == "saveInVerification")  //not started
                {
                    headerobj.STATUS = 1; //Verified
                    headerobj.SAVE_AS_DRAFT = "saveInVerification";
                    headerobj.VERIFIED_AT = DateTime.Now.ToString();
                    headerobj.VERIFIED_BY = objEnquiry.verifiedBy;
                }
                _datacontext.SaveChanges();
                #endregion
                #region select rfq's items
                if (objEnquiry.itemDetails != null)
                {
                    long[] dtlid = new long[objEnquiry.itemDetails.Count];
                    int i = 0;
                    foreach (var item in objEnquiry.itemDetails)
                    {
                        if (Convert.ToString(item.leadTime) == "")
                        {
                            item.leadTime = 0;
                        }
                        T_MESPAS_ENQUIRY_DTL objdtl = (from dtl in _datacontext.T_MESPAS_ENQUIRY_DTLTable where dtl.PK_MESPASENQUIRY_DTL_ID == item.PK_MESPASENQUIRY_DTL_ID select dtl).FirstOrDefault();
                        #region
                        if (objdtl != null)
                        {
                            objdtl.PART_CODE = item.partCode;
                            objdtl.PART_NAME = item.partName;
                            objdtl.QUANTITY = item.quantity.ToString();
                            objdtl.UNIT = item.unit;
                            objdtl.PRICE = item.price;
                            objdtl.COST = item.cost;
                            objdtl.UPDATED_DATE = DateTime.Now.ToString();
                            objdtl.SUPPLIER = item.supplier;
                            objdtl.STATUS = Convert.ToInt32(item.status);
                            objdtl.SEQ_NO = Convert.ToInt32(item.seqNo);
                            objdtl.SEQ_NOTEXT = item.seqNoText;
                            objdtl.ERROR_CODE = item.errorCode;
                            objdtl.ACCOUNT_NO = item.accountNo;
                            objdtl.ACCOUNT_DESCRIPTION = item.accountDescription;
                            objdtl.LEAD_TIME = item.leadTime;
                            objdtl.LEAD_TIME_PERIOD = item.leadTimeperiod;
                            _datacontext.SaveChanges();
                            dtlid[i] = item.PK_MESPASENQUIRY_DTL_ID;
                        }
                        #endregion
                        #region
                        else
                        {
                            T_MESPAS_ENQUIRY_DTL objenqdetails = new T_MESPAS_ENQUIRY_DTL();
                            {
                                objenqdetails.FK_MESPASENQUIRY_HDR_ID = objEnquiry.PK_MESPASENQUIRY_HDR_ID;
                                objenqdetails.PART_CODE = item.partCode;
                                objenqdetails.PART_NAME = item.partName;
                                objenqdetails.QUANTITY = item.quantity.ToString();
                                objenqdetails.UNIT = item.unit;
                                objenqdetails.PRICE = item.price;
                                objenqdetails.COST = item.cost;
                                objenqdetails.UPDATED_DATE = DateTime.Now.ToString();
                                objenqdetails.SUPPLIER = item.supplier;
                                objenqdetails.STATUS = Convert.ToInt32(item.status);
                                if (item.seqNo == "")
                                {
                                    objenqdetails.SEQ_NO = Convert.ToInt32((string)null);
                                }
                                else
                                {
                                    objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                }
                                objenqdetails.SEQ_NOTEXT = item.seqNoText;
                                objenqdetails.ERROR_CODE = item.errorCode;
                                objenqdetails.ACCOUNT_NO = item.accountNo;
                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                objenqdetails.LEAD_TIME = item.leadTime;
                                objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                objenqdetails.IS_UPDATED_WITH_ML = 0;
                            };
                            _datacontext.T_MESPAS_ENQUIRY_DTLTable.Add(objenqdetails);
                            _datacontext.SaveChanges();
                        }
                        i = i + 1;
                        #endregion
                    }
                }
                #endregion
                #region retrun msg 
                if (headerobj.STATUS == 1 && objEnquiry.saveAsDraft == "saveInVerification")
                {
                    obj.result = "Enquiry Save As Draft sucessfully on Verification.";
                    _hub.Clients.All.BroadcastMessage();
                    return obj;
                }
                #endregion
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        #endregion
        #region  update onwer ship of RFQ's
        public Message Updateownership(EnqOwnership objOwnershipdtls)
        {
            #region created object of return message or result class
            Message obj = new Message();
            obj.result = "Error while taking Ownership of Enquiry";
            #endregion
            try
            {
                #region
                T_MESPAS_ENQUIRY_HDR headerobj = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.PK_MESPASENQUIRY_HDR_ID == objOwnershipdtls.PK_MESPASENQUIRY_HDR_ID select hdr).FirstOrDefault();
                if (objOwnershipdtls.action.ToUpper() == "YES")
                {
                    headerobj.OWNERSHIP = objOwnershipdtls.Ownership;
                }
                else
                {
                    headerobj.OWNERSHIP = 0;
                }
                _datacontext.SaveChanges();
                #endregion
                obj.result = "Ownership taken successfully";
                _hub.Clients.All.BroadcastMessage();
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        #endregion
        #region release ownership of rfq's
        public Message Releaseownership(EnqOwnership objEnquirydtls)
        {
            #region created object of return message or result class
            Message obj = new Message();
            obj.result = "Error while releasing Ownership of Enquiry";
            #endregion
            try
            {
                #region
                var EnqHeaderdata = (from enqheader in _datacontext.T_MESPAS_ENQUIRY_HDRTable
                                     where enqheader.OWNERSHIP == objEnquirydtls.Ownership
                                     select new
                                     {
                                         enqheader.PK_MESPASENQUIRY_HDR_ID,
                                         enqheader.ENQUIRY_DATE,
                                         enqheader.ENQREF_NO,
                                         enqheader.SHIP_NAME,
                                         enqheader.OWNER,
                                         enqheader.OWNER_EMAILID,
                                         enqheader.OWNERSHIP,
                                     }).ToList();
                #endregion
                #region check data available or not
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        T_MESPAS_ENQUIRY_HDR objhdr = (from hdr in _datacontext.T_MESPAS_ENQUIRY_HDRTable where hdr.OWNERSHIP == data.OWNERSHIP select hdr).FirstOrDefault();
                        if (objhdr != null)
                        {
                            objhdr.OWNERSHIP = 0;
                            _datacontext.SaveChanges();
                        }
                    }
                }
                #endregion
                obj.result = "Ownership released successfully";
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        #endregion
        #region get ownership details by user id wise
        public EnqOwnership Getuserownershipdtl(int PKHDRID)
        {
            #region created object of class
            EnqOwnership lstEnqheader = new EnqOwnership();
            #endregion
            var action = "";
            #region
            var EnqHeaderdata = (from enqheader in _datacontext.T_MESPAS_ENQUIRY_HDRTable
                                 join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                 from userm in usmt.DefaultIfEmpty()
                                 where enqheader.PK_MESPASENQUIRY_HDR_ID == PKHDRID
                                 select new
                                 {
                                     enqheader.PK_MESPASENQUIRY_HDR_ID,
                                     enqheader.OWNERSHIP,
                                     userm.LOGIN_NAME,
                                     userm.USER_NAME
                                 }).FirstOrDefault();
            #endregion
            if (EnqHeaderdata != null)
            {
                #region
                if (EnqHeaderdata.OWNERSHIP != 0)
                {
                    action = "YES";
                }
                else
                {
                    action = "NO";
                }
                #endregion
                #region
                lstEnqheader = new EnqOwnership
                {
                    PK_MESPASENQUIRY_HDR_ID = EnqHeaderdata.PK_MESPASENQUIRY_HDR_ID,
                    Ownership = EnqHeaderdata.OWNERSHIP,
                    action = action,
                    loginId = EnqHeaderdata.LOGIN_NAME,
                    userName = EnqHeaderdata.USER_NAME
                };
                #endregion
            }
            return lstEnqheader;
        }
        #endregion
        #region send notifcation to client or flologic team for mespas req recived
        public void SendRecivedMespasRFQotifcation(string rfqNo, string customerName)
        {
            try
            {
                #region set envoirnment i.e UAT or PROD
                string enviornment = this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["Name"];
                string portalenviornment = this._Configuration.GetSection("PortalURL")["Name"];
                string produrl = this._Configuration.GetSection("PortalURL")["PRODPortalURL"];
                string uaturl = this._Configuration.GetSection("PortalURL")["UATPortalURL"];
                #endregion
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                #region set from,to or cc email id
                mail.From = new MailAddress(this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["CCMail"]);
                #endregion
                string url = "";
                #region checked prod env or uat env
                if (portalenviornment.ToUpper() == "PROD")
                {
                    url = produrl;
                }
                else
                {
                    url = uaturl;
                }
                #endregion
                #region checked rfq no is blank or not and sent notication
                if (rfqNo != "")
                {
                    mail.Subject = enviornment + " " + "Enquiry - RFQ No : " + rfqNo + " In Mespass Verification Screen ";
                    mail.Body = "<p>Hello Team,<br /><br />RFQ No : " + rfqNo + " is received from MESPAS . please select the department and check data then click the Verify button on PORTAL.<br/></p> " +
                        "<p> Portal URL : <a href=" + url + "> AutomationPortal </a><br/><br/>" +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                else
                {
                    mail.Subject = enviornment + " " + "Enquiry - Customer Name : " + customerName + " In Mespass Verification Screen ";
                    mail.Body = "<p>Hello Team,<br/><br/>Customer Name : " + customerName + " is received from MESPAS . please select the department and check data then click the Verify button on PORTAL.<br/></p> " +
                        "<p> Portal URL : <a href=" + url + "> AutomationPortal </a><br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                #endregion
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["Username"], this._Configuration.GetSection("RFQRecivedOnMESPASVerification")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                string result = "";
                result = ex.Message;
            }
        }
        #endregion
    }
}
