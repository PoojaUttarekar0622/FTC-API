using DuoVia.FuzzyStrings;
using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using static Helper.Data.DataContextClass;
using static Helper.Enum.Status;
using static Helper.Model.MSDClassDeclarations;
using System.Diagnostics;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Helper.Enum;
using System.Threading.Tasks;

namespace Helper.Model
{
    public class MSDEnquiry : IMSDEnquiry
    {
        #region
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        #endregion
        #region
        statusCode notstartedStatus = statusCode.notStartedStatus;
        statusCode verifiedStatus = statusCode.verifiedStatus;
        statusCode inprocessStatus = statusCode.inprocessStatus;
        statusCode successStatus = statusCode.successStatus;
        statusCode errorStatus = statusCode.errorStatus;
        statusCode updatedStatus = statusCode.updatedStatus;
        statusCode forwardToManualStatus = statusCode.forwardToManualStatus;
        statusCode rejectedStatus = statusCode.rejectedStatus;
        statusCode successManualStatus = statusCode.successManualStatus;
        #endregion
        #region
        public MSDEnquiry(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        #endregion
        #region
        public int GetDetailCountforError(long PK_MSDENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MSDENQUIRY_HDR_ID == PK_MSDENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        #endregion
        #region
        public int GetDetailCountforOther(long PK_MSDENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == PK_MSDENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        #endregion
        //hdrdata For Portal
        public List<Enquiryheaderdata> GetEnquiryHeader(int status)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            #region
            if (status == 3)
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                     join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                     join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                     from userm in usmt.DefaultIfEmpty()
                                     where (enqheader.STATUS == 3 || enqheader.STATUS == 2 || enqheader.STATUS == 6)
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
                                         enqheader.SAVE_AS_DRAFT,
                                         enqheader.SOURCE_TYPE,
                                         DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                         userm.LOGIN_NAME,
                                         userm.USER_NAME,
                                         enqheader.EMAIL_RECEIVED_AT
                                     }).OrderByDescending(c => c.PK_MSDENQUIRY_HDR_ID).ToList();

                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
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
                        objEnqheadr.PK_MSDENQUIRY_HDR_ID = data.PK_MSDENQUIRY_HDR_ID;
                        objEnqheadr.enqrefNo = data.ENQREF_NO;
                        objEnqheadr.shipName = data.SHIP_NAME;
                        objEnqheadr.owner = data.OWNER;
                        objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                        objEnqheadr.status = data.STATUS_DESCRIPTION;
                        objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                        objEnqheadr.sourceType = data.SOURCE_TYPE;
                        objEnqheadr.emailReceiveddate = data.EMAIL_RECEIVED_AT;
                        if (data.STATUS_DESCRIPTION == "Success")
                        {
                            DateTime date1 = data.CREATED_DATE;
                            DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                            TimeSpan duration = date2.Subtract(date1);
                            objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                        }
                        objEnqheadr.quotationNo = data.QUOTATION_NO;
                        objEnqheadr.docPath = data.DOC_PATH;
                        objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_MSDENQUIRY_HDR_ID);
                        objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_MSDENQUIRY_HDR_ID);
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
                        lstEnqheader.Add(objEnqheadr);
                    }
                }
            }
            #endregion
            #region
            else if (status == 4)
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                     join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                     join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                     from userm in usmt.DefaultIfEmpty()
                                     where (enqheader.STATUS == status || enqheader.STATUS == (int)successManualStatus)
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
                                         enqheader.SAVE_AS_DRAFT,
                                         enqheader.SOURCE_TYPE,
                                         enqheader.EMAIL_RECEIVED_AT,
                                         userm.LOGIN_NAME,
                                         userm.USER_NAME
                                     }).OrderByDescending(c => c.PK_MSDENQUIRY_HDR_ID).ToList().ToList();
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
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
                        objEnqheadr.PK_MSDENQUIRY_HDR_ID = data.PK_MSDENQUIRY_HDR_ID;
                        objEnqheadr.shipName = data.SHIP_NAME;
                        objEnqheadr.owner = data.OWNER;
                        objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                        objEnqheadr.status = data.STATUS_DESCRIPTION;
                        objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                        objEnqheadr.sourceType = data.SOURCE_TYPE;
                        objEnqheadr.emailReceiveddate = data.EMAIL_RECEIVED_AT;
                        if (data.STATUS_DESCRIPTION == "Success")
                        {
                            DateTime date1 = data.CREATED_DATE;
                            DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                            TimeSpan duration = date2.Subtract(date1);
                            objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                        }
                        objEnqheadr.quotationNo = data.QUOTATION_NO;
                        objEnqheadr.docPath = data.DOC_PATH;
                        objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_MSDENQUIRY_HDR_ID);
                        objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_MSDENQUIRY_HDR_ID);
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
                        lstEnqheader.Add(objEnqheadr);
                    }
                }
            }
            #endregion
            #region
            else
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                     join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                     join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                     from userm in usmt.DefaultIfEmpty()
                                     where enqheader.STATUS == status
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
                                         enqheader.SAVE_AS_DRAFT,
                                         enqheader.SOURCE_TYPE,
                                         enqheader.EMAIL_RECEIVED_AT,
                                         userm.LOGIN_NAME,
                                         userm.USER_NAME
                                     }).OrderByDescending(c => c.PK_MSDENQUIRY_HDR_ID).ToList().ToList();
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
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
                        objEnqheadr.PK_MSDENQUIRY_HDR_ID = data.PK_MSDENQUIRY_HDR_ID;
                        objEnqheadr.enqrefNo = data.ENQREF_NO;
                        objEnqheadr.shipName = data.SHIP_NAME;
                        objEnqheadr.owner = data.OWNER;
                        objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                        objEnqheadr.status = data.STATUS_DESCRIPTION;
                        objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                        objEnqheadr.sourceType = data.SOURCE_TYPE;
                        objEnqheadr.emailReceiveddate = data.EMAIL_RECEIVED_AT;
                        if (data.STATUS_DESCRIPTION == "Success")
                        {
                            DateTime date1 = data.CREATED_DATE;
                            DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                            TimeSpan duration = date2.Subtract(date1);
                            objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                        }
                        objEnqheadr.quotationNo = data.QUOTATION_NO;
                        objEnqheadr.docPath = data.DOC_PATH;
                        objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_MSDENQUIRY_HDR_ID);
                        objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_MSDENQUIRY_HDR_ID);
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
                        lstEnqheader.Add(objEnqheadr);
                    }
                }
            }
            #endregion
            return lstEnqheader;
        }
        public Enquiryheaderdata GetEnquiryDetails(long fkenquiryid)
        {
            #region
            Enquiryheaderdata lstEnqhdrDetails = new Enquiryheaderdata();
            #endregion
            #region
            var HdrData = (from enquiryhdr in _datacontext.TMSD_ENQUIRY_HDRTable where enquiryhdr.PK_MSDENQUIRY_HDR_ID == fkenquiryid select enquiryhdr).ToList();
            #endregion
            #region
            if (HdrData.Count > 0)
            {
                var EnquiryHdrdata = HdrData.First();
                if (EnquiryHdrdata.STATUS == 5)
                {
                    string[] enqdate = EnquiryHdrdata.ENQUIRY_DATE.Split(" ");
                    lstEnqhdrDetails = new Enquiryheaderdata
                    {
                        PK_MSDENQUIRY_HDR_ID = EnquiryHdrdata.PK_MSDENQUIRY_HDR_ID,
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
                        serialno = EnquiryHdrdata.SERIAL_NO,
                        leadTime = Convert.ToInt32(EnquiryHdrdata.LEAD_TIME),
                        leadTimeperiod = EnquiryHdrdata.LEAD_TIME_PERIOD,
                        leadTimeforitem = EnquiryHdrdata.LEAD_TIME_FOR_ITEM,
                        emailReceiveddate = Convert.ToDateTime(EnquiryHdrdata.EMAIL_RECEIVED_AT).ToString("MM/dd/yyyy hh:mm tt"),
                        emailFrom = EnquiryHdrdata.EMAIL_FROM,
                        sourceType = EnquiryHdrdata.SOURCE_TYPE,
                    };
                    var Dtldata = (from enquirydtl in _datacontext.TMSD_ENQUIRY_DTLTable
                                   where enquirydtl.FK_MSDENQUIRY_HDR_ID == lstEnqhdrDetails.PK_MSDENQUIRY_HDR_ID
                                   select new
                                   {
                                       enquirydtl.PK_MSDENQUIRY_DTL_ID,
                                       enquirydtl.FK_MSDENQUIRY_HDR_ID,
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
                    Dtldata = Dtldata.Where(x => x.STATUS == 6 || x.STATUS == 5);
                    var objdtlData = Dtldata.ToList();
                    List<Enquirydetailsdata> lstClsEnquiryDtl = new List<Enquirydetailsdata>();
                    if (objdtlData.Count > 0)
                    {
                        foreach (var item in objdtlData)
                        {
                            Enquirydetailsdata objEnqDetails = new Enquirydetailsdata
                            {
                                PK_MSDENQUIRY_DTL_ID = item.PK_MSDENQUIRY_DTL_ID,
                                fkEnquiryid = item.FK_MSDENQUIRY_HDR_ID,
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
                        }
                        lstEnqhdrDetails.itemDetails = lstClsEnquiryDtl;
                    }
                }
                else
                {
                    string[] enqdate = EnquiryHdrdata.ENQUIRY_DATE.Split(" ");
                    lstEnqhdrDetails = new Enquiryheaderdata
                    {
                        PK_MSDENQUIRY_HDR_ID = EnquiryHdrdata.PK_MSDENQUIRY_HDR_ID,
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
                        serialno = EnquiryHdrdata.SERIAL_NO,
                        leadTime = Convert.ToInt32(EnquiryHdrdata.LEAD_TIME),
                        leadTimeperiod = EnquiryHdrdata.LEAD_TIME_PERIOD,
                        leadTimeforitem = EnquiryHdrdata.LEAD_TIME_FOR_ITEM,
                        emailReceiveddate = Convert.ToDateTime(EnquiryHdrdata.EMAIL_RECEIVED_AT).ToString("MM/dd/yyyy hh:mm tt"),
                        emailFrom = EnquiryHdrdata.EMAIL_FROM,
                        sourceType = EnquiryHdrdata.SOURCE_TYPE,
                    };
                    var Dtldata = (from enquirydtl in _datacontext.TMSD_ENQUIRY_DTLTable
                                   where enquirydtl.FK_MSDENQUIRY_HDR_ID == lstEnqhdrDetails.PK_MSDENQUIRY_HDR_ID
                                   select new
                                   {
                                       enquirydtl.PK_MSDENQUIRY_DTL_ID,
                                       enquirydtl.FK_MSDENQUIRY_HDR_ID,
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
                                   }).ToList();
                    List<Enquirydetailsdata> lstClsEnquiryDtl = new List<Enquirydetailsdata>();
                    if (Dtldata.Count > 0)
                    {
                        foreach (var item in Dtldata)
                        {
                            Enquirydetailsdata objEnqDetails = new Enquirydetailsdata
                            {
                                PK_MSDENQUIRY_DTL_ID = item.PK_MSDENQUIRY_DTL_ID,
                                fkEnquiryid = item.FK_MSDENQUIRY_HDR_ID,
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
                                leadTime = Convert.ToInt32(EnquiryHdrdata.LEAD_TIME),
                                leadTimeperiod = EnquiryHdrdata.LEAD_TIME_PERIOD
                            };
                            lstClsEnquiryDtl.Add(objEnqDetails);
                        }
                        lstEnqhdrDetails.itemDetails = lstClsEnquiryDtl;
                    }
                }
            }
            #endregion
            return lstEnqhdrDetails;
        }
        //From Email Bot
        public MessageMSD InsertEnquiry(Enquiryheader objEnquirydtls)
        {
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Creating Enquiry";
            int isUpdatedWithML = 0;
            string isDuplicteEntry = "NO";
            var key = "E546C8DF278CD5931069B522E695D4F2";
            #region
            string MLLogicMSD = this._Configuration.GetSection("MLLogicMSD")["MLAllow"];
            string MakerBlank = this._Configuration.GetSection("MSDMakers")["MakerBlank"];
            string SourceType = this._Configuration.GetSection("SourceType")["SourceTypeName"];
            #endregion
            try
            {
                if (objEnquirydtls != null)
                {
                    if (objEnquirydtls.enqrefNo != "")
                    {
                        #region DuplicateEntry
                        if (objEnquirydtls.IsAdditionalRound == "Yes")
                        {
                            isDuplicteEntry = "NO";
                        }
                        else if (objEnquirydtls.status == this._Configuration.GetSection("DuplicateEnquiryMSDException")["StatusCode"] && this._Configuration.GetSection("DuplicateEnquiryMSDException")["DuplicateAllow"] == "YES")
                        {
                            isDuplicteEntry = "NO";
                        }
                        else
                        {
                            var DuplicateEntry = (from hdrdata in _datacontext.TMSD_ENQUIRY_HDRTable
                                                  where hdrdata.ENQREF_NO == objEnquirydtls.enqrefNo.Trim() && hdrdata.STATUS != 8
                                                  select new
                                                  {
                                                      hdrdata.ENQREF_NO
                                                  }).ToList();
                            if (DuplicateEntry.Count > 0)
                            {
                                isDuplicteEntry = "YES";
                            }
                        }
                        #endregion DuplicateEntry
                        if (isDuplicteEntry == "NO" || this._Configuration.GetSection("DuplicateEnquiryMSD")["DuplicateAllow"] == "YES")
                        {
                            string strPath = objEnquirydtls.docPath;
                            #region
                            var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                           select new
                                           {
                                               part.PART_NAME,
                                               part.PART_CODE
                                           }).Distinct().ToList();
                            #endregion
                            #region
                            TMSD_ENQUIRY_HDR objenqheader = new TMSD_ENQUIRY_HDR();
                            objenqheader.FK_PROCESS_ID = 1;
                            objenqheader.FK_INSTANCE_ID = 1;
                            objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                            objenqheader.OWNER = GetCustomerMapping(objEnquirydtls.owner);
                            objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                            objenqheader.SHIP_NAME = GetShipName(objEnquirydtls.shipName);
                            objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                            objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMSDDocumentFolder"] + "" + Path.GetFileName(strPath);
                            objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                            objenqheader.CREATED_BY = 1;
                            objenqheader.CREATED_DATE = DateTime.Now;
                            objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                            if (String.IsNullOrEmpty(objEnquirydtls.maker) || objEnquirydtls.maker == "None")
                            {
                                objenqheader.MAKER = MakerBlank;
                            }
                            else
                            {
                                objenqheader.MAKER = GetManufacturingMapping(objEnquirydtls.maker);
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
                            objenqheader.SUPPLIER_CODE = GetSupplierMapping(objEnquirydtls.maker, objEnquirydtls.supplierCode);
                            objenqheader.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                            objenqheader.LEAD_TIME = 0;
                            objenqheader.SAVE_AS_DRAFT = "";
                            objenqheader.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                            objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                            objenqheader.EMAIL_FROM = objEnquirydtls.emailFrom;
                            objenqheader.MAIL_BODY = objEnquirydtls.mailBody;
                            objenqheader.MAKER_INFO = objEnquirydtls.makerInfo;
                            objenqheader.STATUS = 0;
                            if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                            {
                                objenqheader.SOURCE_TYPE = objEnquirydtls.sourceType;
                                objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = objEnquirydtls.IsUpdatedEquipmentWithML;
                                objenqheader.IS_UPDATED_MAKER_WITH_ML = objEnquirydtls.IsUpdatedMakerWithML;
                            }
                            else
                            {
                                objenqheader.SOURCE_TYPE = SourceType;
                                objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = 0;
                                objenqheader.IS_UPDATED_MAKER_WITH_ML = 0;
                            }
                            if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                            {
                                objenqheader.EVENT_ID = objEnquirydtls.eventId;
                            }
                            else
                            {
                                objenqheader.EVENT_ID = 0;
                            }
                            objenqheader.ISADDITIONL_ROUND = objEnquirydtls.IsAdditionalRound;
                            objenqheader.ERROR_VERIFIED_COUNTER = 0;
                            objenqheader.EMAIL_SUBJECT = objEnquirydtls.emailSubject;
                            objenqheader.RFQ_PASSWORD = EnDecryptOperation.Encrypt(objEnquirydtls.rfqPassword, key); ;
                            objenqheader.REMARK = objEnquirydtls.hdrRemark;
                            objenqheader.AUTH_CODE = objEnquirydtls.authCode;
                            _datacontext.TMSD_ENQUIRY_HDRTable.Add(objenqheader);
                            _datacontext.SaveChanges();
                            #endregion
                            #region
                            long PKENQHDRID = objenqheader.PK_MSDENQUIRY_HDR_ID;
                            #endregion
                            TMSD_ENQUIRY_HDR_ORG objenqheaderorg = new TMSD_ENQUIRY_HDR_ORG();
                            objenqheaderorg.FK_PROCESS_ID = 1;
                            objenqheaderorg.FK_INSTANCE_ID = 1;
                            objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                            objenqheaderorg.OWNER = objEnquirydtls.owner;
                            objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                            objenqheaderorg.SHIP_NAME = GetShipName(objEnquirydtls.shipName);
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
                            if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                            {
                                objenqheaderorg.SOURCE_TYPE = objEnquirydtls.sourceType;
                            }
                            else
                            {
                                objenqheaderorg.SOURCE_TYPE = SourceType;
                            }
                            _datacontext.TMSD_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                            _datacontext.SaveChanges();
                            long PKENQHDRORGID = objenqheaderorg.PK_MSDENQUIRY_HDR_ID;
                            //save multiple header levels document path
                            if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                            {
                                foreach (var item in objEnquirydtls.docHdrDetails)
                                {
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
                            }
                            if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                            {
                                foreach (var item in objEnquirydtls.itemDetails)
                                {
                                    if (item.cost != "")
                                    {
                                        TMSD_ENQUIRY_HDR objenqhead = new TMSD_ENQUIRY_HDR();
                                        objenqhead.STATUS = 5;
                                        _datacontext.SaveChanges();
                                        string[] cost = item.cost.Split(".00");
                                        TMSD_ENQUIRY_DTL_ORG objenqdetailsorg = new TMSD_ENQUIRY_DTL_ORG();
                                        {
                                            objenqdetailsorg.FK_MSDENQUIRY_HDR_ID = PKENQHDRORGID;
                                            objenqdetailsorg.PART_CODE = item.partCode;
                                            objenqdetailsorg.PART_NAME = item.partName;
                                            objenqdetailsorg.QUANTITY = item.quantity;
                                            objenqdetailsorg.UNIT = item.unit;
                                            objenqdetailsorg.PRICE = item.price;
                                            objenqdetailsorg.COST = cost[0];
                                            objenqdetailsorg.UPDATED_DATE = DateTime.Now;
                                            objenqdetailsorg.SUPPLIER = item.supplier;
                                            objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetailsorg.ERROR_CODE = item.errorCode;
                                            objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                            objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                        };
                                        _datacontext.TMSD_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                        _datacontext.SaveChanges();
                                        //ML Logic For PartCode/Impa Code
                                        #region ML Logic to find IMPA code
                                        string partCode = item.partCode;
                                        string partName = item.partName;
                                        if (MLLogicMSD == "YES")
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
                                        string quantity = item.quantity;
                                        bool isDataValid = true;
                                        // if qty column is non numeric, omit the item
                                        if (!float.TryParse(quantity, out float qtyValue)
                                           )
                                        {
                                            isDataValid = false;
                                        }
                                        if (isDataValid && !String.IsNullOrEmpty(quantity))
                                        {
                                            TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                            {
                                                objenqdetails.FK_MSDENQUIRY_HDR_ID = PKENQHDRID;
                                                objenqdetails.PART_CODE = item.partCode;
                                                objenqdetails.PART_NAME = item.partName;
                                                objenqdetails.QUANTITY = quantity;
                                                objenqdetails.UNIT = GetUnitMapping(item.unit);
                                                objenqdetails.PRICE = item.price;
                                                objenqdetails.COST = cost[0];
                                                objenqdetails.UPDATED_DATE = DateTime.Now;
                                                objenqdetails.SUPPLIER = item.supplier;
                                                objenqdetails.STATUS = 5;
                                                objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                                objenqdetails.ERROR_CODE = item.errorCode;
                                                objenqdetails.ACCOUNT_NO = item.accountNo;
                                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                                objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                                objenqdetails.LEAD_TIME = 0;
                                                objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                                if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                                                {
                                                    objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = Convert.ToInt32(item.IsUpdatedMESPASItemsWithML);
                                                }
                                                else
                                                {
                                                    objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = 0;
                                                }
                                            };
                                            _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                            _datacontext.SaveChanges();
                                            long PKDTLID = objenqdetails.PK_MSDENQUIRY_DTL_ID;
                                            if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                            {
                                                if (item.docdtlDetails != null)
                                                {
                                                    foreach (var item2 in item.docdtlDetails)
                                                    {
                                                        T_DOCUMENT_DTL objdocdtl = new T_DOCUMENT_DTL();
                                                        {
                                                            objdocdtl.ENQUIRY_DTL_ID = PKDTLID;
                                                            objdocdtl.ERROR_DESCRIPTION = item2.errorDescription;
                                                            objdocdtl.DOC_PATH = item2.docPath;
                                                            objdocdtl.CREATEBY = 1;
                                                            objdocdtl.CREATEDAT = DateTime.Now;
                                                            objdocdtl.ISACTIVE = 1;
                                                        };
                                                        _datacontext.T_DOCUMENT_DTLTable.Add(objdocdtl);
                                                        _datacontext.SaveChanges();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string[] cost = item.cost.Split(".00");
                                        TMSD_ENQUIRY_DTL_ORG objenqdetailsorg = new TMSD_ENQUIRY_DTL_ORG();
                                        {
                                            objenqdetailsorg.FK_MSDENQUIRY_HDR_ID = PKENQHDRORGID;
                                            objenqdetailsorg.PART_CODE = item.partCode;
                                            objenqdetailsorg.PART_NAME = item.partName;
                                            objenqdetailsorg.QUANTITY = item.quantity;
                                            objenqdetailsorg.UNIT = item.unit;
                                            objenqdetailsorg.PRICE = item.price;
                                            objenqdetailsorg.COST = cost[0];
                                            objenqdetailsorg.UPDATED_DATE = DateTime.Now;
                                            objenqdetailsorg.SUPPLIER = item.supplier;
                                            objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                            objenqdetailsorg.ERROR_CODE = item.errorCode;
                                            objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                            objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                        };

                                        _datacontext.TMSD_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                        _datacontext.SaveChanges();
                                        //ML Logic For PartCode/Impa Code
                                        #region ML Logic to find IMPA code
                                        string partCode = item.partCode;
                                        string partName = item.partName;
                                        if (MLLogicMSD == "YES")
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
                                        string quantity = item.quantity;
                                        bool isDataValid = true;
                                        // if qty column is non numeric, omit the item

                                        if (!float.TryParse(quantity, out float qtyValue)
                                           )
                                        {
                                            isDataValid = false;
                                        }
                                        if (isDataValid && !String.IsNullOrEmpty(quantity))
                                        {
                                            TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                            {
                                                objenqdetails.FK_MSDENQUIRY_HDR_ID = PKENQHDRID;
                                                objenqdetails.PART_CODE = partCode;
                                                objenqdetails.PART_NAME = item.partName;
                                                objenqdetails.QUANTITY = quantity;
                                                objenqdetails.UNIT = GetUnitMapping(item.unit);
                                                objenqdetails.PRICE = item.price;
                                                objenqdetails.COST = cost[0];
                                                objenqdetails.UPDATED_DATE = DateTime.Now;
                                                objenqdetails.SUPPLIER = item.supplier;
                                                objenqdetails.STATUS = GetAutoverification(objEnquirydtls.owner, objEnquirydtls.status);//Convert.ToInt32(item.status);
                                                objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                                objenqdetails.ERROR_CODE = item.errorCode;
                                                objenqdetails.ACCOUNT_NO = item.accountNo;
                                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                                objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                                objenqdetails.LEAD_TIME = 0;
                                                objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                                if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                                                {
                                                    objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = Convert.ToInt32(item.IsUpdatedMESPASItemsWithML);
                                                }
                                                else
                                                {
                                                    objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = 0;
                                                }
                                            };
                                            _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                            _datacontext.SaveChanges();
                                            long PKDTLID = objenqdetails.PK_MSDENQUIRY_DTL_ID;
                                            if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                            {
                                                if (item.docdtlDetails != null)
                                                {
                                                    foreach (var item2 in item.docdtlDetails)
                                                    {
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

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #region added this logic 07 March 2023
                            if (objEnquirydtls.status == "10")
                            {
                                objenqheader.STATUS = 10;
                            }
                            else
                            {
                                if (objEnquirydtls.itemDetails.Count > 0)
                                {
                                    TMSD_ENQUIRY_HDR objdt = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == PKENQHDRID select hdr).FirstOrDefault();
                                     int currentstatus = GetAutoverification(objEnquirydtls.owner, objEnquirydtls.status);
                                     objdt.STATUS = currentstatus;
                                    _datacontext.SaveChanges();
                                    #region
                                    if (objdt.STATUS == (int)verifiedStatus)
                                    {
                                        string verfiedBy = this._Configuration.GetSection("BOTVerfiied")["VerifiedBy"];
                                        TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == PKENQHDRID select hdr).FirstOrDefault();
                                        if (headerobj != null)
                                        {
                                            headerobj.VERIFIED_BY = verfiedBy;
                                            _datacontext.SaveChanges();
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    objenqheader.STATUS = 1;
                                }
                            }

                            #endregion

                          
                            obj.result = "Enquiry Saved Successfully";
                            _hub.Clients.All.BroadcastMessage();

                        }
                        else
                        {
                            obj.result = "Duplicate Entry";
                            if(objEnquirydtls.owner.Trim().ToUpper() == "KUWAIT OIL TANKE*WEB")
                            {
                                TMSD_ENQUIRY_HDR objupdatemsd = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.ENQREF_NO == objEnquirydtls.enqrefNo select hdr).FirstOrDefault();
                                if (objupdatemsd!= null)
                                {
                                    objupdatemsd.RFQ_URL = objEnquirydtls.rfqUrl;
                                    _datacontext.SaveChanges();
                                }
                            }

                        }
                    }
                    else
                    {
                        string strPath = objEnquirydtls.docPath;
                        var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                       select new
                                       {
                                           part.PART_NAME,
                                           part.PART_CODE
                                       }).Distinct().ToList();
                        TMSD_ENQUIRY_HDR objenqheader = new TMSD_ENQUIRY_HDR();
                        objenqheader.FK_PROCESS_ID = 1;
                        objenqheader.FK_INSTANCE_ID = 1;
                        objenqheader.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                        objenqheader.OWNER = GetCustomerMapping(objEnquirydtls.owner);//GetCustomerMapping(objEnquirydtls.owner, objEnquirydtls.ownerEmailid);//
                        objenqheader.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                        objenqheader.SHIP_NAME = GetShipName(objEnquirydtls.shipName);
                        objenqheader.ENQREF_NO = objEnquirydtls.enqrefNo;
                        objenqheader.MAIL_BODY = objEnquirydtls.mailBody;
                        objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMSDDocumentFolder"] + "" + Path.GetFileName(strPath);
                        objenqheader.STATUS = 5;
                        objenqheader.QUOTATION_NO = objEnquirydtls.quotationNo;
                        objenqheader.CREATED_BY = 1;
                        objenqheader.CREATED_DATE = DateTime.Now;
                        objenqheader.ERROR_CODE = objEnquirydtls.errorCode;
                        if (String.IsNullOrEmpty(objEnquirydtls.maker))
                        {
                            objenqheader.MAKER = MakerBlank;
                        }
                        else
                        {
                            objenqheader.MAKER = GetManufacturingMapping(objEnquirydtls.maker);
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
                        objenqheader.SUPPLIER_CODE = GetSupplierMapping(objEnquirydtls.maker, objEnquirydtls.supplierCode);
                        objenqheader.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                        objenqheader.LEAD_TIME = 0;
                        objenqheader.SAVE_AS_DRAFT = "";
                        objenqheader.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                        objenqheader.RFQ_URL = objEnquirydtls.rfqUrl;
                        objenqheader.EMAIL_FROM = objEnquirydtls.emailFrom;
                        objenqheader.MAKER_INFO = objEnquirydtls.makerInfo;
                        objenqheader.ISADDITIONL_ROUND = objEnquirydtls.IsAdditionalRound;
                        objenqheader.EMAIL_SUBJECT = objEnquirydtls.emailSubject;
                        objenqheader.RFQ_PASSWORD = EnDecryptOperation.Encrypt(objEnquirydtls.rfqPassword, key); ;
                        objenqheader.REMARK = objEnquirydtls.hdrRemark;
                        objenqheader.AUTH_CODE = objEnquirydtls.authCode;
                        if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheader.SOURCE_TYPE = objEnquirydtls.sourceType;
                            objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = objEnquirydtls.IsUpdatedEquipmentWithML;
                            objenqheader.IS_UPDATED_MAKER_WITH_ML = objEnquirydtls.IsUpdatedMakerWithML;
                        }
                        else
                        {
                            objenqheader.SOURCE_TYPE = SourceType;
                            objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = 0;
                            objenqheader.IS_UPDATED_MAKER_WITH_ML = 0;
                        }
                        if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheader.EVENT_ID = objEnquirydtls.eventId;
                        }
                        else
                        {
                            objenqheader.EVENT_ID = 0;
                        }
                        objenqheader.ERROR_VERIFIED_COUNTER = 0;
                        _datacontext.TMSD_ENQUIRY_HDRTable.Add(objenqheader);
                        _datacontext.SaveChanges();
                        long PKENQHDRID = objenqheader.PK_MSDENQUIRY_HDR_ID;
                        TMSD_ENQUIRY_HDR_ORG objenqheaderorg = new TMSD_ENQUIRY_HDR_ORG();
                        objenqheaderorg.FK_PROCESS_ID = 1;
                        objenqheaderorg.FK_INSTANCE_ID = 1;
                        objenqheaderorg.ENQUIRY_DATE = objEnquirydtls.enquiryDate;
                        objenqheaderorg.OWNER = objEnquirydtls.owner;
                        objenqheaderorg.OWNER_EMAILID = objEnquirydtls.ownerEmailid;
                        objenqheaderorg.SHIP_NAME = GetShipName(objEnquirydtls.shipName);
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
                        if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheaderorg.SOURCE_TYPE = objEnquirydtls.sourceType;
                        }
                        else
                        {
                            objenqheaderorg.SOURCE_TYPE = SourceType;
                        }
                        _datacontext.TMSD_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                        _datacontext.SaveChanges();
                        long PKENQHDRORGID = objenqheader.PK_MSDENQUIRY_HDR_ID;
                        if (objEnquirydtls.docHdrDetails != null && PKENQHDRID != 0)
                        {
                            foreach (var item in objEnquirydtls.docHdrDetails)
                            {
                                    T_DOCUMENT_HDR objenqdetails = new T_DOCUMENT_HDR();
                                    {
                                        objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                        objenqdetails.DOC_PATH = item.docPath;
                                        objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                        objenqdetails.CREATEBY = 1;
                                        objenqdetails.CREATEDAT = DateTime.Now;
                                        objenqdetails.ISACTIVE = 1;
                                    };
                                    _datacontext.T_DOCUMENT_HDRTable.Add(objenqdetails);
                                    _datacontext.SaveChanges();
                            }
                        }
                        if (objEnquirydtls.itemDetails != null && PKENQHDRID != 0)
                        {
                            foreach (var item in objEnquirydtls.itemDetails)
                            {
                                string[] cost = item.cost.Split(".00");
                                TMSD_ENQUIRY_DTL_ORG objenqdetailsorg = new TMSD_ENQUIRY_DTL_ORG();
                                {
                                    objenqdetailsorg.FK_MSDENQUIRY_HDR_ID = PKENQHDRORGID;
                                    objenqdetailsorg.PART_CODE = item.partCode;
                                    objenqdetailsorg.PART_NAME = item.partName;
                                    objenqdetailsorg.QUANTITY = item.quantity;
                                    objenqdetailsorg.UNIT = item.unit;
                                    objenqdetailsorg.PRICE = item.price;
                                    objenqdetailsorg.COST = cost[0];
                                    objenqdetailsorg.UPDATED_DATE = DateTime.Now;
                                    objenqdetailsorg.SUPPLIER = item.supplier;
                                    objenqdetailsorg.SEQ_NO = Convert.ToInt32(item.seqNo);
                                    objenqdetailsorg.ERROR_CODE = item.errorCode;
                                    objenqdetailsorg.ACCOUNT_NO = item.accountNo;
                                    objenqdetailsorg.ACCOUNT_DESCRIPTION = item.accountDescription;
                                };

                                _datacontext.TMSD_ENQUIRY_DTL_ORGTable.Add(objenqdetailsorg);
                                _datacontext.SaveChanges();

                                //ML Logic For PartCode/Impa Code
                                #region ML Logic to find IMPA code
                                string partCode = item.partCode;
                                string partName = item.partName;
                                if (MLLogicMSD == "YES")
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
                                TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_MSDENQUIRY_HDR_ID = PKENQHDRID;
                                    objenqdetails.PART_CODE = item.partCode;
                                    objenqdetails.PART_NAME = item.partName;
                                    objenqdetails.QUANTITY = item.quantity;
                                    objenqdetails.UNIT = GetUnitMapping(item.unit);
                                    objenqdetails.PRICE = item.price;
                                    objenqdetails.COST = cost[0];
                                    objenqdetails.UPDATED_DATE = DateTime.Now;
                                    objenqdetails.SUPPLIER = item.supplier;
                                    objenqdetails.STATUS = 5;
                                    objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                    objenqdetails.ERROR_CODE = item.errorCode;
                                    objenqdetails.ACCOUNT_NO = item.accountNo;
                                    objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                    objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                    objenqdetails.LEAD_TIME = 0;
                                    objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                    if (objEnquirydtls.sourceType.ToUpper() == "MESPAS")
                                    {
                                        objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = Convert.ToInt32(item.IsUpdatedMESPASItemsWithML);
                                    }
                                    else
                                    {
                                        objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = 0;
                                    }
                                };
                                _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                                long PKDTLID = objenqdetails.PK_MSDENQUIRY_DTL_ID;
                                if (objEnquirydtls.itemDetails != null && PKDTLID != 0)
                                {
                                    if (item.docdtlDetails != null)
                                    {
                                        foreach (var item2 in item.docdtlDetails)
                                        {
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
                                        }
                                    }

                                }
                            }
                        }
                        obj.result = "Enquiry Saved Successfully";
                        _hub.Clients.All.BroadcastMessage();
                        var tuple = GetCustomerMap(objenqheader.OWNER, "");
                        string ownerEmailId = tuple.Item1;
                        SendErrorRFQNotification(objEnquirydtls.enqrefNo, objEnquirydtls.owner, "",objEnquirydtls.shipName,objEnquirydtls.emailSubject,objEnquirydtls.mailBody);
                    }
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.InnerException.Message;
            }
            return obj;
        }

        //From Portal
        public MessageMSD UpdateEnquiry(Enquiryheader objEnquiry)
        {
            #region
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Updating Enquiry";
            #endregion
            try
            {
                if (objEnquiry != null)
                {
                    #region
                    TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == objEnquiry.PK_MSDENQUIRY_HDR_ID select hdr).FirstOrDefault();
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
                    headerobj.OWNERSHIP = 0;
                    #region
                    if (Convert.ToInt32(objEnquiry.status) != 8)
                    {
                        if (headerobj.STATUS == 1)
                        {
                            headerobj.STATUS = 2;
                            headerobj.SAVE_AS_DRAFT = "";
                            headerobj.VERIFIED_AT = DateTime.Now.ToString();
                            headerobj.VERIFIED_BY = objEnquiry.verifiedBy;
                        }
                        else if (headerobj.STATUS == 5)
                        {
                            headerobj.STATUS = 6;
                            headerobj.SAVE_AS_DRAFT = "";
                            headerobj.UPDATED_AT = DateTime.Now.ToString();
                            headerobj.CORRECTED_BY = objEnquiry.correctedBy;
                        }
                        else if (headerobj.STATUS == 7)
                        {
                            headerobj.STATUS = 7;
                        }
                        else if (headerobj.STATUS == 2 || headerobj.STATUS == 6)
                        {
                            headerobj.STATUS = 4;
                            headerobj.UPDATED_AT = DateTime.Now.ToString();
                            headerobj.QUOTATION_CREATED_AT = objEnquiry.quotationCreatedat;
                        }
                        else
                        {
                            headerobj.STATUS = 5;
                            headerobj.ERROR_CODE = objEnquiry.errorCode;
                            headerobj.IN_ERROR_AT = DateTime.Now.ToString();
                            var tuple = GetCustomerMap(headerobj.OWNER, "");
                            string ownerEmailId = tuple.Item1;
                            SendErrorRFQNotification(objEnquiry.enqrefNo, objEnquiry.owner, "",objEnquiry.shipName,objEnquiry.emailSubject,objEnquiry.mailBody);
                            // when quotation submit then ref in error basket we will direct process it and set completed manuall status
                            if (headerobj != null)
                            {
                                if (!String.IsNullOrEmpty(headerobj.QUATATION_SUBMIT_DATE))
                                {
                                    headerobj.STATUS = 9;
                                    _datacontext.SaveChanges();
                                    TMSD_ENQUIRY_DTL objdtl1 = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.FK_MSDENQUIRY_HDR_ID == headerobj.PK_MSDENQUIRY_HDR_ID select dtl).FirstOrDefault();
                                    if (objdtl1 != null)
                                    {
                                        objdtl1.STATUS = 9;
                                        _datacontext.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        headerobj.STATUS = 8;
                    }
                    #endregion
                    _datacontext.SaveChanges();
                    #endregion
                    #region
                    if (objEnquiry.itemDetails != null)
                    {
                        #region
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (Convert.ToString(item.leadTime) == "")
                            {
                                item.leadTime = 0;
                            }
                            TMSD_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.PK_MSDENQUIRY_DTL_ID == item.PK_MSDENQUIRY_DTL_ID select dtl).FirstOrDefault();
                           
                            if (objdtl != null)
                            {

                                objdtl.PART_CODE = item.partCode;
                                objdtl.PART_NAME = item.partName;
                                objdtl.QUANTITY = item.quantity.ToString();
                                objdtl.UNIT = item.unit;
                                objdtl.PRICE = item.price;
                                objdtl.COST = item.cost;
                                objdtl.UPDATED_DATE = DateTime.Now;
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
                                dtlid[i] = item.PK_MSDENQUIRY_DTL_ID;
                            }
                            else
                            {
                                TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_MSDENQUIRY_HDR_ID = objEnquiry.PK_MSDENQUIRY_HDR_ID;
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
                                    objenqdetails.SEQ_NOTEXT = item.seqNoText;
                                    objenqdetails.ERROR_CODE = item.errorCode;
                                    objenqdetails.ACCOUNT_NO = item.accountNo;
                                    objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                    objenqdetails.LEAD_TIME = item.leadTime;
                                    objenqdetails.LEAD_TIME_PERIOD = item.leadTimeperiod;
                                    objenqdetails.IS_UPDATED_WITH_ML = 0;
                                };

                                _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                            i = i + 1;


                        }
                        #endregion
                    }
                    if (headerobj.STATUS == 2)
                    {
                        obj.result = "Enquiry Verified sucessfully.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    else if (headerobj.STATUS == 6)
                    {
                        obj.result = "Enquiry Updated sucessfully.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    else if (headerobj.STATUS == 8)
                    {
                        obj.result = "Enquiry Rejected.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    else if (headerobj.STATUS == 4 || headerobj.STATUS == 5)
                    {
                        obj.result = "Enquiry Updated.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        public MessageMSD UpdateSaveAsEnquiry(Enquiryheader objEnquiry)
        {
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Updating Enquiry";
            try
            {
                if (objEnquiry != null)
                {
                    #region
                    TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == objEnquiry.PK_MSDENQUIRY_HDR_ID select hdr).FirstOrDefault();
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
                    #region
                    if (headerobj.STATUS == 1 && objEnquiry.saveAsDraft == "saveInVerification")
                    {
                        headerobj.STATUS = 1; 
                        headerobj.SAVE_AS_DRAFT = "saveInVerification";
                        headerobj.VERIFIED_AT = DateTime.Now.ToString();
                        headerobj.VERIFIED_BY = objEnquiry.verifiedBy;
                    }
                    else if (headerobj.STATUS == 5 && objEnquiry.saveAsDraft == "saveInError") // error
                    {
                        headerobj.STATUS = 5; // updated
                        headerobj.SAVE_AS_DRAFT = "saveInError";
                        headerobj.UPDATED_AT = DateTime.Now.ToString();
                        headerobj.CORRECTED_BY = objEnquiry.correctedBy;
                    }
                    #endregion
                    _datacontext.SaveChanges();
                    #endregion

                    #region
                    if (objEnquiry.itemDetails != null)
                    {
                        #region
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (Convert.ToString(item.leadTime) == "")
                            {
                                item.leadTime = 0;
                            }
                            TMSD_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.PK_MSDENQUIRY_DTL_ID == item.PK_MSDENQUIRY_DTL_ID select dtl).FirstOrDefault();
                            if (objdtl != null)
                            {
                                objdtl.PART_CODE = item.partCode;
                                objdtl.PART_NAME = item.partName;
                                objdtl.QUANTITY = item.quantity.ToString();
                                objdtl.UNIT = item.unit;
                                objdtl.PRICE = item.price;
                                objdtl.COST = item.cost;
                                objdtl.UPDATED_DATE = DateTime.Now;
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
                                dtlid[i] = item.PK_MSDENQUIRY_DTL_ID;
                            }
                            else
                            {
                                TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_MSDENQUIRY_HDR_ID = objEnquiry.PK_MSDENQUIRY_HDR_ID;
                                    objenqdetails.PART_CODE = item.partCode;
                                    objenqdetails.PART_NAME = item.partName;
                                    objenqdetails.QUANTITY = item.quantity.ToString();
                                    objenqdetails.UNIT = item.unit;
                                    objenqdetails.PRICE = item.price;
                                    objenqdetails.COST = item.cost;
                                    objenqdetails.UPDATED_DATE = DateTime.Now;
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

                                _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                            i = i + 1;
                        }
                        #endregion
                    }
                    if (headerobj.STATUS == 1 && objEnquiry.saveAsDraft == "saveInVerification")
                    {
                        obj.result = "Enquiry Save As Draft sucessfully on Verification.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    else if (headerobj.STATUS == 5 && objEnquiry.saveAsDraft == "saveInError") // error
                    {
                        obj.result = "Enquiry Save As Draft sucessfully on Error.";
                        _hub.Clients.All.BroadcastMessage();
                        return obj;
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //Delete dtl item From portal
        public MessageMSD DeleteItems(List<Enquirydetailsdata> objEnquirydtls)
        {
            #region
            List<Enquirydetailsdata> EnqDetails = new List<Enquirydetailsdata>();
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Deleting Enquiry";
            #endregion
            #region
            try
            {
                foreach (var item in objEnquirydtls)
                {
                    TMSD_ENQUIRY_DTL objMsdDtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable
                                                  where dtl.PK_MSDENQUIRY_DTL_ID == item.PK_MSDENQUIRY_DTL_ID
                                                  select dtl).ToList().SingleOrDefault();
                    if (objMsdDtl != null)
                    {
                        _datacontext.TMSD_ENQUIRY_DTLTable.Remove(objMsdDtl);
                        _datacontext.SaveChanges();
                    }
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
        //From AS400 bot
        public MessageMSD UpdateEnqStatus(EnquiryheaderdataForas400 objEnquirydtls)
        {
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Updating Enquiry from AS400";
            string MLLogicMSD = this._Configuration.GetSection("MLLogicMSD")["MLAllow"];
            try
            {

                TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == objEnquirydtls.PK_MSDENQUIRY_HDR_ID select hdr).FirstOrDefault();

                #region
                if (Convert.ToInt32(objEnquirydtls.status) == 5)
                {
                    headerobj.STATUS = 5; //Error
                    headerobj.ERROR_CODE = objEnquirydtls.errorCode;
                    headerobj.QUOTATION_NO = objEnquirydtls.quotationNo;
                    headerobj.IN_ERROR_AT = DateTime.Now.ToString();


                    if (objEnquirydtls.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquirydtls.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquirydtls.itemDetails)
                        {
                            if (Convert.ToInt32(item.status) == 5)
                            {
                                TMSD_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.PK_MSDENQUIRY_DTL_ID == item.PK_MSDENQUIRY_DTL_ID select dtl).FirstOrDefault();
                                if (objdtl != null)
                                {

                                    objdtl.STATUS = 5;// Convert.ToInt32(item.status);
                                    objdtl.ERROR_CODE = item.errorCode;
                                    objdtl.UPDATED_DATE = item.UPDATED_DATE;
                                    _datacontext.SaveChanges();
                                    dtlid[i] = item.PK_MSDENQUIRY_DTL_ID;

                                }

                                i = i + 1;
                            }
                        }

                    }
                    _datacontext.SaveChanges();

                    var tuple = GetCustomerMap(headerobj.OWNER, "");

                    string ownerEmailId = tuple.Item1;
                    SendErrorRFQNotification(headerobj.ENQREF_NO, headerobj.OWNER, "",headerobj.SHIP_NAME,headerobj.EMAIL_SUBJECT,headerobj.MAIL_BODY);

                    // when quotation submit then ref in error basket we will direct process it and set completed manuall status
                    if (headerobj != null)
                    {
                        if (!String.IsNullOrEmpty(headerobj.QUATATION_SUBMIT_DATE))
                        {

                            headerobj.STATUS = 9;

                            _datacontext.SaveChanges();

                            TMSD_ENQUIRY_DTL objdtl1 = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.FK_MSDENQUIRY_HDR_ID == headerobj.PK_MSDENQUIRY_HDR_ID select dtl).FirstOrDefault();

                            if (objdtl1 != null)
                            {
                                objdtl1.STATUS = 9;

                                _datacontext.SaveChanges();
                            }
                        }
                    }
                }
                #endregion
                #region
                if (Convert.ToInt32(objEnquirydtls.status) == 4 || Convert.ToInt32(objEnquirydtls.status) == 5)
                {
                    if (Convert.ToInt32(objEnquirydtls.status) != 5)
                    {
                        headerobj.STATUS = 4; //Success
                        headerobj.QUOTATION_CREATED_AT = objEnquirydtls.quotationCreatedat;
                        headerobj.QUOTATION_NO = objEnquirydtls.quotationNo;

                    }
                    if (objEnquirydtls.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquirydtls.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquirydtls.itemDetails)
                        {
                            if (Convert.ToInt32(item.status) == 4) // if (Convert.ToInt32(item.status) == 4)
                            {
                                TMSD_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.PK_MSDENQUIRY_DTL_ID == item.PK_MSDENQUIRY_DTL_ID select dtl).FirstOrDefault();
                                if (objdtl != null)
                                {

                                    objdtl.STATUS = 4;// Convert.ToInt32(item.status);
                                    objdtl.ERROR_CODE = item.errorCode;
                                    objdtl.UPDATED_DATE = item.UPDATED_DATE;
                                    objdtl.IS_UPDATED_WITH_ML = 1;
                                    _datacontext.SaveChanges();
                                    dtlid[i] = item.PK_MSDENQUIRY_DTL_ID;
                                }
                            }
                            if (MLLogicMSD == "YES")
                            {
                                if (!string.IsNullOrEmpty(item.partCode))
                                {
                                    var EnqMlData = (from mlitems in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                                     where (mlitems.PART_CODE == item.partCode && mlitems.PART_NAME.ToUpper() == item.partName.ToUpper())
                                                     select new

                                                     {
                                                         mlitems.PART_NAME,
                                                         mlitems.PART_CODE
                                                     }).ToList();

                                    if (EnqMlData.Count == 0)
                                    {
                                        T_SNQ_ENQUIRY_ML_ITEMS objmlitems = new T_SNQ_ENQUIRY_ML_ITEMS();
                                        {
                                            objmlitems.PART_NAME = item.partName;
                                            objmlitems.PART_CODE = item.partCode;
                                            objmlitems.CREATEDAT = DateTime.Now;
                                            objmlitems.IS_ACTIVE = 1;
                                        };

                                        _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable.Add(objmlitems);
                                        _datacontext.SaveChanges();
                                    }
                                }
                            }
                            i = i + 1;
                        }

                    }

                    _datacontext.SaveChanges();
                }
                #endregion
                obj.result = "Enquiry Updated.";
                return obj;

            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //For AS400 bot
        public List<EnquiryheaderdataForas400> GetDetailsForAs400()
        {
            List<EnquiryheaderdataForas400> lstEnqHdrdtls = new List<EnquiryheaderdataForas400>();
            MakerSelection objmakerSelection = new MakerSelection(_datacontext,_Configuration);
            var HdrData = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                           where (hdr.STATUS == 2 || hdr.STATUS == 6)
                           select hdr).OrderBy(c => c.PK_MSDENQUIRY_HDR_ID).ToList();
            int NoofEnq = Convert.ToInt32(this._Configuration.GetSection("GetEntriesForAs400")["Take"]);
            HdrData = HdrData.Take(NoofEnq).ToList();
            if (HdrData.Count > 0)
            {
                foreach (var item in HdrData)
                {
                    #region
                    EnquiryheaderdataForas400 objClsEnquiryhdr = new EnquiryheaderdataForas400
                    {
                        PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID,
                        enquiryDate = item.ENQUIRY_DATE,
                        owner = item.OWNER.Trim(),
                        ownerEmailid = GetCustomerEmailMapping(item.OWNER),
                        as400UserId = GetCustomerAS400IdMapping(item.OWNER),
                        enqrefNo = item.ENQREF_NO,
                        shipName = item.SHIP_NAME,
                        maker = item.MAKER,
                        mfgCode = objmakerSelection.GetMFGCodeWithSwitchCase(item.MAKER),
                        type = item.TYPE,
                        equipment = item.EQUIPMENT,
                        serialNo = item.SERIAL_NO,
                        supplierCode = item.SUPPLIER_CODE,
                        status = item.STATUS.ToString(),
                        quotationNo = item.QUOTATION_NO,
                        leadTimeforitem = item.LEAD_TIME_FOR_ITEM,
                        leadTime = Convert.ToInt32(item.LEAD_TIME),
                        leadTimeperiod = item.LEAD_TIME_PERIOD,
                        makerInfo = item.MAKER_INFO,
                        rfqUrl = item.RFQ_URL,
                        docPath = item.DOC_PATH,
                        emailSubject = item.EMAIL_SUBJECT,
                        hdrRemark = item.REMARK,
                        authCode = item.AUTH_CODE,
                        totalNoOfItems = GetDetailCountforOther(item.PK_MSDENQUIRY_HDR_ID),
                     };
                    #endregion
                    if (!String.IsNullOrEmpty(objClsEnquiryhdr.owner))
                    {
                        objClsEnquiryhdr.owner = objClsEnquiryhdr.owner.Trim();
                    }
                    if (!String.IsNullOrEmpty(objClsEnquiryhdr.ownerEmailid))
                    {
                        objClsEnquiryhdr.ownerEmailid = objClsEnquiryhdr.ownerEmailid.Trim();
                    }
                    #region
                    List<EnquiryHeaderDocDetails> lstHdrdoc = new List<EnquiryHeaderDocDetails>();
                    var docdata = (from doc in _datacontext.T_DOCUMENT_HDRTable
                                   where doc.ENQUIRY_HDR_ID == item.PK_MSDENQUIRY_HDR_ID
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
                            if (!String.IsNullOrEmpty(objdoc.errorDescription))
                            {
                                objClsEnquiryhdr.IsAttachmentHasErrors = "Yes";
                            }
                            lstHdrdoc.Add(objdoc);
                        }
                        objClsEnquiryhdr.docHdrDetails = lstHdrdoc;
                    }
                    #endregion
                    #region
                    var Dtldata = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable
                                   where dtl.FK_MSDENQUIRY_HDR_ID == objClsEnquiryhdr.PK_MSDENQUIRY_HDR_ID
                                   select new
                                   {
                                       dtl.PK_MSDENQUIRY_DTL_ID,
                                       dtl.FK_MSDENQUIRY_HDR_ID,
                                       dtl.PART_CODE,
                                       dtl.PART_NAME,
                                       dtl.QUANTITY,
                                       dtl.UNIT,
                                       dtl.PRICE,
                                       dtl.COST,
                                       dtl.STATUS,
                                       dtl.SUPPLIER,
                                       dtl.UPDATED_DATE,
                                       dtl.ACCOUNT_NO,
                                       dtl.ACCOUNT_DESCRIPTION,
                                       dtl.SEQ_NO,
                                       dtl.LEAD_TIME,
                                       dtl.LEAD_TIME_PERIOD,
                                       dtl.SEQ_NOTEXT
                                   }).ToList();
                    List<EnquirydetailsdataForas400> lstClsEnquiryDtl = new List<EnquirydetailsdataForas400>();
                    if (Dtldata.Count > 0)
                    {
                        foreach (var item1 in Dtldata)
                        {
                            int qty = Convert.ToInt32(Convert.ToDouble(item1.QUANTITY));
                            EnquirydetailsdataForas400 objClsEnquirydtl = new EnquirydetailsdataForas400
                            {
                                PK_MSDENQUIRY_DTL_ID = item1.PK_MSDENQUIRY_DTL_ID,
                                fkEnquiryid = item1.FK_MSDENQUIRY_HDR_ID,
                                partCode = item1.PART_CODE,
                                partName = item1.PART_NAME.Trim(),
                                quantity = qty,
                                unit = item1.UNIT,
                                price = Convert.ToString(item1.PRICE),
                                cost = item1.COST,
                                supplier = item1.SUPPLIER,
                                UPDATED_DATE = item1.UPDATED_DATE,
                                accountNo = item1.ACCOUNT_NO,
                                accountDescription = item1.ACCOUNT_DESCRIPTION,
                                seqNo = item1.SEQ_NO.ToString(),
                                seqNoText = item1.SEQ_NOTEXT,
                                leadTime = Convert.ToInt32(item1.LEAD_TIME),
                                leadTimeperiod = item1.LEAD_TIME_PERIOD,
                                status = Convert.ToString(item1.STATUS)
                            };
                            lstClsEnquiryDtl.Add(objClsEnquirydtl);
                            List<EnquiryDetailDocDetails> lstdtldoc = new List<EnquiryDetailDocDetails>();
                            var docdtldata = (from doc in _datacontext.T_DOCUMENT_DTLTable
                                              where doc.ENQUIRY_DTL_ID == objClsEnquirydtl.PK_MSDENQUIRY_DTL_ID
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
                                    if (objClsEnquiryhdr.IsAttachmentHasErrors != "Yes")
                                    {
                                        if (!String.IsNullOrEmpty(objdoc.errorDescription))
                                        {
                                            objClsEnquiryhdr.IsAttachmentHasErrors = "Yes";
                                        }
                                    }
                                    lstdtldoc.Add(objdoc);
                                }
                                objClsEnquirydtl.docdtlDetails = lstdtldoc;
                            }
                        }
                        objClsEnquiryhdr.itemDetails = lstClsEnquiryDtl;
                        long totalitemscount = GetDetailCountforOther(item.PK_MSDENQUIRY_HDR_ID);
                        if (objClsEnquiryhdr.totalNoOfItems == totalitemscount)
                        {
                            lstEnqHdrdtls.Add(objClsEnquiryhdr);
                        }
                        else
                        {
                            Notification ns = new Notification(_Configuration);
                            ns.SendPendingRFQNotification(objClsEnquiryhdr.enqrefNo,objClsEnquiryhdr.totalNoOfItems, 2);
                        }
                    }
                    #endregion
                }
            }
            return lstEnqHdrdtls;
        }
        public List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (objsearchdata.FromDate != "")
            {
                #region
                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)notstartedStatus
                                select new
                                {
                                    enqheader.PK_MSDENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo);
                }
                if (objsearchdata.sourceType != "")
                {
                    Tasklist = Tasklist.Where(x => x.SOURCE_TYPE.ToUpper() == objsearchdata.sourceType.ToUpper());
                }
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
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.enquirydt.Split(" ");
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
                #endregion
            }
            else
            {
                #region
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
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo);
                }
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.ENQUIRY_DATE.Split(" ");
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
                #endregion
            }
        }
        public List<Enquiryheaderdata> GetErrorTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (objsearchdata.FromDate != "")
            {
                #region
                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)errorStatus
                                select new
                                {
                                    enqheader.PK_MSDENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo || x.QUOTATION_NO == objsearchdata.CustNameShipNameRefNo);
                }
                if (objsearchdata.sourceType != "")
                {
                    Tasklist = Tasklist.Where(x => x.SOURCE_TYPE.ToUpper() == objsearchdata.sourceType.ToUpper());
                }
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
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.enquirydt.Split(" ");
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
                #endregion
            }
            else
            {
                #region
                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)errorStatus
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
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo || x.QUOTATION_NO == objsearchdata.CustNameShipNameRefNo);
                }
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    string[] dt = item.ENQUIRY_DATE.Split(" ");
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
                #endregion
            }
        }
        //Get Completed Enquiry List
        public List<Enquiryheaderdata> GetCompletedTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            #region
            if (objsearchdata.FromDate != "")
            {
                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where (enqheader.STATUS == (int)successStatus || enqheader.STATUS == (int)successManualStatus)
                                select new
                                {
                                    enqheader.PK_MSDENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TMSD_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_MSDENQUIRY_HDR_ID == enqheader.PK_MSDENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo || x.QUOTATION_NO == objsearchdata.CustNameShipNameRefNo);
                }
                if (objsearchdata.sourceType != "")
                {
                    Tasklist = Tasklist.Where(x => x.SOURCE_TYPE.ToUpper() == objsearchdata.sourceType.ToUpper());
                }
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
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    if (!String.IsNullOrEmpty(item.enquirydt))
                    {
                        string[] dt = item.enquirydt.Split(" ");
                    }
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.duration = item.QUOTATION_CREATED_AT;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
            }
            else
            {

                var Tasklist = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where (enqheader.STATUS == (int)successStatus || enqheader.STATUS == (int)successManualStatus)
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
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo || x.QUOTATION_NO == objsearchdata.CustNameShipNameRefNo);
                }
                var objdata = Tasklist.ToList();
                foreach (var item in Tasklist)
                {
                    Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                    if (!String.IsNullOrEmpty(item.ENQUIRY_DATE))
                    {
                        string[] dt = item.ENQUIRY_DATE.Split(" ");
                        objEnqheadr.enquiryDate = dt[0];
                    }
                    else
                    {
                        objEnqheadr.enquiryDate = "";
                    }
                    objEnqheadr.PK_MSDENQUIRY_HDR_ID = item.PK_MSDENQUIRY_HDR_ID;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.ownerEmailid = item.OWNER_EMAILID;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.duration = item.QUOTATION_CREATED_AT;
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
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_MSDENQUIRY_HDR_ID).ToList();
            }
            #endregion
        }
        public MessageMSD Updateownership(EnqOwnership objOwnershipdtls)
        {
            Enquirydetailsdata EnqDetails = new Enquirydetailsdata();
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while taking Ownership of Enquiry";
            #region
            try
            {
                TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.PK_MSDENQUIRY_HDR_ID == objOwnershipdtls.PK_MSDENQUIRY_HDR_ID select hdr).FirstOrDefault();
                if (objOwnershipdtls.action.ToUpper() == "YES")
                {
                    headerobj.OWNERSHIP = objOwnershipdtls.Ownership;
                }
                else
                {
                    headerobj.OWNERSHIP = 0;
                }
                _datacontext.SaveChanges();
                obj.result = "Ownership taken successfully";
                _hub.Clients.All.BroadcastMessage();
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
            #endregion
        }
        public MessageMSD Releaseownership(EnqOwnership objEnquirydtls)
        {
            Enquirydetailsdata EnqDetails = new Enquirydetailsdata();
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while releasing Ownership of Enquiry";
            #region
            try
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                     where enqheader.OWNERSHIP == objEnquirydtls.Ownership
                                     select new
                                     {
                                         enqheader.PK_MSDENQUIRY_HDR_ID,
                                         enqheader.ENQUIRY_DATE,
                                         enqheader.ENQREF_NO,
                                         enqheader.SHIP_NAME,
                                         enqheader.OWNER,
                                         enqheader.OWNER_EMAILID,
                                         enqheader.OWNERSHIP,
                                     }).ToList();
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        TMSD_ENQUIRY_HDR objhdr = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.OWNERSHIP == data.OWNERSHIP select hdr).FirstOrDefault();
                        if (objhdr != null)
                        {
                            objhdr.OWNERSHIP = 0;
                            _datacontext.SaveChanges();
                        }
                    }
                }
                obj.result = "Ownership released successfully";
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
            #endregion
        }
        public EnquiryheaderForMSDBOT GetRFQURL(string RFQNO)
        {
            EnquiryheaderForMSDBOT lstEnqheader = new EnquiryheaderForMSDBOT();
            var decryptkey = "E546C8DF278CD5931069B522E695D4F2";
            #region
            var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                 where (enqheader.ENQREF_NO == RFQNO && (enqheader.STATUS != 7 || enqheader.STATUS != 8))

                                 select new
                                 {
                                     enqheader.PK_MSDENQUIRY_HDR_ID,
                                     enqheader.ENQREF_NO,
                                     enqheader.SHIP_NAME,
                                     enqheader.OWNER,
                                     enqheader.RFQ_URL,
                                     enqheader.QUOTATION_RECIVED_AT,
                                     enqheader.RFQ_PASSWORD,
                                     enqheader.AUTH_CODE,
                                     enqheader.EMAIL_SUBJECT,
                                     enqheader.EMAIL_PROCESSED_AT
                                 }).ToList();

            if (EnqHeaderdata.Count > 0)
            {
                var EnquiryHdrdata = EnqHeaderdata.LastOrDefault();
                var tuple = GetCustomerMap(EnquiryHdrdata.OWNER, "");
                lstEnqheader = new EnquiryheaderForMSDBOT
                {
                    msdEnquiryID = EnquiryHdrdata.PK_MSDENQUIRY_HDR_ID,
                    enqrefNo = EnquiryHdrdata.ENQREF_NO,
                    shipName = EnquiryHdrdata.SHIP_NAME,
                    owner = EnquiryHdrdata.OWNER,
                    rfqUrl = EnquiryHdrdata.RFQ_URL,
                    ownerEmailid = GetCustomerEmailMapping(EnquiryHdrdata.OWNER),
                    as400UserId = GetCustomerAS400IdMapping(EnquiryHdrdata.OWNER),
                    emailSubject = EnquiryHdrdata.EMAIL_SUBJECT,
                    emailProcessedat = EnquiryHdrdata.EMAIL_PROCESSED_AT,
                    rfqPassword = EnDecryptOperation.Decrypt(EnquiryHdrdata.RFQ_PASSWORD, decryptkey),
                    authCode = EnquiryHdrdata.AUTH_CODE,
                };

            }

            List<EnquiryDetailsForMSDBOT> lstClsEnquiryDtl = new List<EnquiryDetailsForMSDBOT>();

            EnquiryDetailsForMSDBOT objEnqDetails = new EnquiryDetailsForMSDBOT
            {
                seqNo = "",
                quantity = "",
                cost = "",
                leadTime = "",
                leadTimeperiod = ""

            };
            lstClsEnquiryDtl.Add(objEnqDetails);

            lstEnqheader.itemDetails = lstClsEnquiryDtl;

            return lstEnqheader;
            #endregion
        }
        public MessageMSD UpdatePriceandLeadtime(EnquiryheaderForMSDBOT objEnquirydtls)
        {
            MessageMSD obj = new MessageMSD();
            obj.result = "Error while Updating Price And Leadtime";
            try
            {
                long fkenqid = 0;
                TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable where hdr.ENQREF_NO == objEnquirydtls.enqrefNo && hdr.QUOTATION_NO == (objEnquirydtls.quotationNo).Replace("FTC-", "").Trim() select hdr).ToList().LastOrDefault(); 
                #region
                if (headerobj != null)
                {
                    if (objEnquirydtls.leadTime == "" || objEnquirydtls.leadTime == "null")
                    {
                        headerobj.LEAD_TIME = 0;
                    }
                    else
                    {
                        headerobj.LEAD_TIME = Convert.ToInt32(objEnquirydtls.leadTime);
                    }
                    headerobj.LEAD_TIME_PERIOD = objEnquirydtls.leadTimeperiod;
                    headerobj.LEAD_TIME_FOR_ITEM = objEnquirydtls.leadTimeforitem;
                    headerobj.QUOTATION_RECIVED_AT = objEnquirydtls.quotationReceiveddate;
                    headerobj.QUATATION_SUBMIT_DATE = objEnquirydtls.quotationSubmitdate;
                    fkenqid = headerobj.PK_MSDENQUIRY_HDR_ID;
                    _datacontext.SaveChanges();
                    if (objEnquirydtls.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquirydtls.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquirydtls.itemDetails)
                        {
                            string[] leadtime = new string[] { };
                            string leadtimeperiod = "";
                            if (item.leadTime == "" || item.leadTime == "null")
                            {
                                item.leadTime = "0";
                            }
                            else
                            {
                                leadtime = item.leadTime.Split(" ");
                                if (leadtime[1].ToUpper() == "DAY" || leadtime[1].ToUpper() == "DAYS")
                                {
                                    leadtimeperiod = "D";
                                }
                                if (leadtime[1].ToUpper() == "WEEK" || leadtime[1].ToUpper() == "WEEKS")
                                {
                                    leadtimeperiod = "W";
                                }
                                if (leadtime[1].ToUpper() == "MONTH" || leadtime[1].ToUpper() == "MONTHS")
                                {
                                    leadtimeperiod = "M";
                                }
                                if (leadtime[1].ToUpper() == "YEAR" || leadtime[1].ToUpper() == "YEARS")
                                {
                                    leadtimeperiod = "Y";
                                }

                            }
                            string cost = "";
                            string cost1 = "";
                            if (item.cost.Contains(","))
                            {
                                cost = item.cost.Replace(",", "");
                                if (cost.Contains(".00"))
                                {
                                    cost1 = cost.Replace(".00", "");
                                }
                            }
                            TMSD_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.FK_MSDENQUIRY_HDR_ID == fkenqid && dtl.SEQ_NO == Convert.ToInt32(item.seqNo) && dtl.QUANTITY == item.quantity select dtl).FirstOrDefault();
                            if (objdtl != null)
                            {
                                objdtl.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objdtl.AS400_DESCRIPTION = item.as400Description;
                                objdtl.QUANTITY = item.quantity;
                                objdtl.UNIT = item.unit;
                                objdtl.COST = cost1;
                                if (item.leadTime == "0")
                                {
                                    objdtl.LEAD_TIME = 0;
                                }
                                else
                                {
                                    objdtl.LEAD_TIME = Convert.ToInt32(leadtime[0]);
                                }
                                objdtl.LEAD_TIME_PERIOD = leadtimeperiod;
                                _datacontext.SaveChanges();
                                dtlid[i] = Convert.ToInt32(item.seqNo);
                            }
                            else
                            {
                                TMSD_ENQUIRY_DTL objenqdetails = new TMSD_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_MSDENQUIRY_HDR_ID = fkenqid;
                                    objenqdetails.PART_NAME = item.as400Description;
                                    objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                    objenqdetails.AS400_DESCRIPTION = item.as400Description;
                                    objenqdetails.QUANTITY = item.quantity;
                                    objenqdetails.UNIT = item.unit;
                                    objenqdetails.COST = cost1;
                                    objenqdetails.STATUS = 2;
                                    if (item.leadTime == "0")
                                    {
                                        objenqdetails.LEAD_TIME = 0;
                                    }
                                    else
                                    {
                                        objenqdetails.LEAD_TIME = Convert.ToInt32(leadtime[0]);
                                    }
                                    objenqdetails.LEAD_TIME_PERIOD = leadtimeperiod;
                                    objenqdetails.UPDATED_DATE = DateTime.Now;
                                };
                                _datacontext.TMSD_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                            i = i + 1;
                        }
                    }
                    obj.result = "Price and LeadTime Updated Successfully.";
                }
                else
                {
                    obj.result = "Price and LeadTime Not Updated Successfully.";
                }
                #endregion
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //For portal 
        public EnqOwnership Getuserownershipdtl(int PKHDRID)
        {
            #region
            EnqOwnership lstEnqheader = new EnqOwnership();
            var action = "";
            var EnqHeaderdata = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                 join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                 from userm in usmt.DefaultIfEmpty()
                                 where enqheader.PK_MSDENQUIRY_HDR_ID == PKHDRID
                                 select new
                                 {
                                     enqheader.PK_MSDENQUIRY_HDR_ID,
                                     enqheader.OWNERSHIP,
                                     userm.LOGIN_NAME,
                                     userm.USER_NAME
                                 }).FirstOrDefault();
            if (EnqHeaderdata != null)
            {
                if (EnqHeaderdata.OWNERSHIP != 0)
                {
                    action = "YES";
                }
                else
                {
                    action = "NO";
                }
                lstEnqheader = new EnqOwnership
                {
                    PK_MSDENQUIRY_HDR_ID = EnqHeaderdata.PK_MSDENQUIRY_HDR_ID,
                    Ownership = EnqHeaderdata.OWNERSHIP,
                    action = action,
                    loginId = EnqHeaderdata.LOGIN_NAME,
                    userName = EnqHeaderdata.USER_NAME
                };
            }
            return lstEnqheader;
            #endregion
        }
        #region verify error RFQs (only AS400 lag issue)
        public MessageMSD VerifyRFQ(string errorRFQ)
        {
            MessageMSD objmessage = new MessageMSD();
            string verfiedBy = this._Configuration.GetSection("ErrorRFQVerified")["VerifiedBy"];
            string verfiedAllow = this._Configuration.GetSection("ErrorRFQVerified")["Allow"];
            string ErrorVerifiedCount = this._Configuration.GetSection("ErrorRFQVerified")["ErrorVerifiedCount"];
            objmessage.result = "Data not available";
            try
            {
                #region
                if (errorRFQ.ToUpper() == "YES")
                {
                    if (verfiedAllow.ToUpper() == "YES")
                    {
                        #region Get msd header data
                        var MsdhdrData = (from enqheader in _datacontext.TMSD_ENQUIRY_HDRTable
                                          join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                          where (enqheader.STATUS == (int)errorStatus) && (enqheader.ERROR_CODE == "" || enqheader.ERROR_CODE == null)
                                          select new
                                          {
                                              enqheader.PK_MSDENQUIRY_HDR_ID,
                                              enqheader.ERROR_VERIFIED_COUNTER,
                                              enqheader.ENQREF_NO

                                          }).ToList();
                        #endregion

                        #region update header part
                        foreach (var item in MsdhdrData)
                        {
                            TMSD_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                                          where hdr.PK_MSDENQUIRY_HDR_ID == item.PK_MSDENQUIRY_HDR_ID
                                                          select hdr).FirstOrDefault();
                            if (item.ERROR_VERIFIED_COUNTER < Convert.ToInt16(ErrorVerifiedCount))
                            {
                                #region heade data
                                if (headerobj != null)
                                {
                                    headerobj.STATUS = 6;
                                    headerobj.CORRECTED_BY = verfiedBy;
                                    headerobj.ERROR_VERIFIED_COUNTER = headerobj.ERROR_VERIFIED_COUNTER + 1;
                                    _datacontext.SaveChanges();
                                }
                                #endregion

                                #region detail data
                                var MsdDtlData = (from enqheader in _datacontext.TMSD_ENQUIRY_DTLTable
                                                  join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                                  where (enqheader.STATUS == (int)errorStatus || enqheader.STATUS == (int)verifiedStatus) && enqheader.FK_MSDENQUIRY_HDR_ID == item.PK_MSDENQUIRY_HDR_ID
                                                   && (enqheader.ERROR_CODE == "" || enqheader.ERROR_CODE == null)
                                                  select new
                                                  {
                                                      enqheader.PK_MSDENQUIRY_DTL_ID,
                                                      enqheader.STATUS,

                                                  }).ToList();
                                #endregion

                                #region update dtl part

                                foreach (var item1 in MsdDtlData)
                                {
                                    TMSD_ENQUIRY_DTL dtlobj = (from dtl in _datacontext.TMSD_ENQUIRY_DTLTable where dtl.PK_MSDENQUIRY_DTL_ID == item1.PK_MSDENQUIRY_DTL_ID select dtl).FirstOrDefault();
                                    {
                                        if (dtlobj != null)
                                        {
                                            dtlobj.STATUS = 6;

                                            _datacontext.SaveChanges();
                                        }
                                    }
                                }
                                #endregion

                                objmessage.result = "Data updated successfully";
                            }
                            else
                            {
                                objmessage.result = "Not Update";
                                SendErrorVerifedRFQNotification(item.ENQREF_NO);
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        objmessage.result = "Data Not updated successfully";

                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                objmessage.result = ex.Message;
            }

            return objmessage;
        }
        #endregion
        #region
        public string GetCustomerMapping(string owner)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomer = "";
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomer = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                    where hdr.TEMPLATE_CUSTOMER_NAME.ToUpper() == owner.ToUpper()
                                    select hdr.AS400_CUSTOMER_NAME).ToList();
                if (findCustomer.Count > 0)
                {
                    mappedCustomer = findCustomer[0];
                }
                else
                {
                    mappedCustomer = owner;
                }
            }
            else
            {
                mappedCustomer = owner;
            }
            //end customer mapping
            #endregion customer mapping
            return mappedCustomer;
        }
        #endregion
        #region
        public string GetCustomerEmailMapping(string owner)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomerEmailID = "";
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomerEmailId = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                           join dtl in _datacontext.MCUSTOMERTable on hdr.FK_CUST_ID equals dtl.PK_CUSTOMER_ID
                                           where hdr.AS400_CUSTOMER_NAME.ToUpper().Trim() == owner.ToUpper().Trim() && dtl.DEPT_NAME.Trim().ToUpper() != "SNQ"
                                           select dtl.CUSTOMER_EMAILID).ToList();
                if (findCustomerEmailId.Count() > 0)
                {
                    mappedCustomerEmailID = findCustomerEmailId[0];
                }
            }
            //end customer mapping
            #endregion customer mapping
            return mappedCustomerEmailID;
        }
        #endregion

        #region
        public string GetCustomerAS400IdMapping(string owner)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomerAS400Id = "";
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomerAS400Id = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                           join dtl in _datacontext.MCUSTOMERTable on hdr.FK_CUST_ID equals dtl.PK_CUSTOMER_ID
                                           where hdr.AS400_CUSTOMER_NAME.ToUpper().Trim() == owner.ToUpper().Trim() && dtl.DEPT_NAME.Trim().ToUpper() !="SNQ"
                                           select dtl.AS400USER_ID).ToList();
                if (findCustomerAS400Id.Count() > 0)
                {
                    mappedCustomerAS400Id = findCustomerAS400Id[0];
                }
            }
            //end customer mapping
            #endregion customer mapping
            return mappedCustomerAS400Id;
        }
        #endregion
        #region tuples are used for multiple value return
        public Tuple<string, string> GetCustomerMap(string owner, string email)
        {
            string owneremaild = "", AS400UserId = "";
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomerEmailId = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                           join dtl in _datacontext.MCUSTOMERTable on hdr.FK_CUST_ID equals dtl.PK_CUSTOMER_ID
                                           where hdr.AS400_CUSTOMER_NAME.Trim().ToUpper() == owner.Trim().ToUpper()
                                           select new
                                           {
                                               dtl.CUSTOMER_EMAILID,
                                               dtl.AS400USER_ID,
                                           }).ToList();
                foreach (var item in findCustomerEmailId)
                    if (findCustomerEmailId.Count() > 0)
                    {
                        owneremaild = item.CUSTOMER_EMAILID;
                        AS400UserId = item.AS400USER_ID;
                    }
            }
            return new Tuple<string, string>(owneremaild, AS400UserId);
        }
        #endregion
        #region
        public string GetCustomerMapping(string owner, string ownerEmailId)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomer = "";
            string MTMEmailiD = this._Configuration.GetSection("MSDMTMEmailId")["MTMCustEmailId"];
            string MTMIndia = this._Configuration.GetSection("MSDMTMEmailId")["MTMIndia"];
            string MTMSingapore = this._Configuration.GetSection("MSDMTMEmailId")["MTMSingapore"];
            string MTMCust = this._Configuration.GetSection("MSDMTMEmailId")["MTMCust"];
            if (!string.IsNullOrEmpty(owner))
            {
                if (owner.Trim() != MTMCust)
                {
                    var findCustomer = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                        where hdr.TEMPLATE_CUSTOMER_NAME.ToUpper() == owner.ToUpper()
                                        select hdr.AS400_CUSTOMER_NAME).ToList();
                    if (findCustomer.Count > 0)
                    {
                        mappedCustomer = findCustomer[0];
                    }
                    else
                    {
                        mappedCustomer = owner;
                    }
                }
                else
                {
                    if (MTMEmailiD.Contains(ownerEmailId))
                    {
                        mappedCustomer = MTMIndia;
                    }
                    else
                    {
                        mappedCustomer = MTMSingapore;
                    }
                }
            }
            else
            {
                mappedCustomer = owner;
            }
            //end customer mapping
            #endregion customer mapping
            return mappedCustomer;
        }
        #endregion
        #region
        public string GetShipName(string shipName)
        {
            #region Shipname
            //Shipname mapping
            string Shipname = shipName;
            if (shipName != null)
            {
                if (shipName.Contains('-'))
                {
                    string[] words = shipName.Split('-');
                    Shipname = words[1].Trim();
                }
                else
                {
                    Shipname = shipName;
                }
            }
            return Shipname;
            //end Shipname mapping
            #endregion shipname
        }
        #endregion
        #region
        public string GetManufacturingMapping(string maker)
        {
            #region Manufacturer mapping
            //Manufacturer mapping
            string mappedManufacturer = "";
            string MakerMITSUBISHIKAKOKI = this._Configuration.GetSection("MSDMakers")["MakerMITSUBISHIKAKOKI"];
            string MakerMITSUBISHIHEAVY = this._Configuration.GetSection("MSDMakers")["MakerMITSUBISHIHEAVY"];
            if (!string.IsNullOrEmpty(maker))
            {
                if (maker == MakerMITSUBISHIKAKOKI || maker == MakerMITSUBISHIHEAVY)
                {
                    var findManufacturer = (from hdr in _datacontext.MMANUFACTURER_MAPPINGTable
                                            where hdr.TEMPLATE_MANUFACTURER_NAME.ToUpper() == maker.ToUpper()
                                            select hdr.AS400_MANUFACTURER_NAME).Distinct().ToList();
                    if (findManufacturer.Count > 0)
                    {
                        mappedManufacturer = findManufacturer[0];
                    }
                    else
                    {
                        mappedManufacturer = maker;
                    }
                }
                else
                {
                    var findManufacturer = (from hdr in _datacontext.MMANUFACTURER_MAPPINGTable
                                            where hdr.TEMPLATE_MANUFACTURER_NAME.ToUpper().Contains(maker.ToUpper())
                                            select hdr.AS400_MANUFACTURER_NAME).Distinct().ToList();
                    if (findManufacturer.Count > 0)
                    {
                        mappedManufacturer = findManufacturer[0];
                    }
                    else
                    {
                        mappedManufacturer = maker;
                    }
                }
            }
            else
            {
                mappedManufacturer = maker;
            }
            return mappedManufacturer;
            //end Manufacturer mapping
            #endregion Manufacturer mapping
        }
        #endregion
        #region
        public string GetSupplierMapping(string maker, string supplierCode)
        {
            #region SupplierCode mapping
            //SupplierCode mapping
            string mappedSupplierCode = "";
            string MakerMITSUBISHIKAKOKI = this._Configuration.GetSection("MSDMakers")["MakerMITSUBISHIKAKOKI"];
            string MakerMITSUBISHIHEAVY = this._Configuration.GetSection("MSDMakers")["MakerMITSUBISHIHEAVY"];
            if (!string.IsNullOrEmpty(maker))
            {
                if (maker == MakerMITSUBISHIKAKOKI || maker == MakerMITSUBISHIHEAVY)
                {
                    var findSupplierCode = (from hdr in _datacontext.MMANUFACTURER_SUPPLIER_MAPPINGTable
                                            join dtl in _datacontext.MMANUFACTURER_MAPPINGTable on hdr.FK_MANUFACTURER_ID equals dtl.FK_MANUFACTURER_ID
                                            where dtl.TEMPLATE_MANUFACTURER_NAME.ToUpper() == maker.ToUpper()
                                            select hdr.SUPPLIER_CODE).Distinct().ToList();
                    if (findSupplierCode.Count > 0)
                    {
                        mappedSupplierCode = findSupplierCode[0];
                    }
                    else
                    {
                        mappedSupplierCode = supplierCode;
                    }
                }
                else
                {
                    var findSupplierCode = (from hdr in _datacontext.MMANUFACTURER_SUPPLIER_MAPPINGTable
                                            join dtl in _datacontext.MMANUFACTURER_MAPPINGTable on hdr.FK_MANUFACTURER_ID equals dtl.FK_MANUFACTURER_ID
                                            where dtl.TEMPLATE_MANUFACTURER_NAME.ToUpper().Contains(maker.ToUpper())
                                            select hdr.SUPPLIER_CODE).Distinct().ToList();
                    if (findSupplierCode.Count > 0)
                    {
                        mappedSupplierCode = findSupplierCode[0];
                    }
                    else
                    {
                        mappedSupplierCode = supplierCode;
                    }
                }
            }
            else
            {
                mappedSupplierCode = supplierCode;
            }
            return mappedSupplierCode;
            //end SupplierCode mapping
            #endregion SupplierCode mapping
        }
        #endregion
        #region
        public string GetUnitMapping(string unit)
        {
            //unit mapping
            #region unit manipulation
            string AS400unit = "";
            if (!string.IsNullOrEmpty(unit))
            {
                var UnitOM = (from hdr in _datacontext.MUOMTable
                              where hdr.TEMPLATE_UOM.ToLower() == unit.ToString().ToLower()
                              select hdr.AS400_UOM).ToList();
                if (UnitOM.Count > 0)
                {
                    AS400unit = UnitOM[0];
                }
                else
                {
                    AS400unit = unit;
                }
            }
            else
            {
                AS400unit = unit;
            }
            return AS400unit;
            #endregion unit manipulation
            //end unit mapping
        }
        #endregion
        #region
        public int GetAutoverification(string owner, string status)
        {
            #region
            //Autoverification
            int autoverifiedstatus = 0;
            var autoverification = (from hdr in _datacontext.MCUSTOMERTable
                                    where hdr.CUSTOMERNAME.ToUpper() == owner.ToUpper()
                                    select hdr.AUTOVERIFICATION).SingleOrDefault();
            if (autoverification == "Y")
            {
                autoverifiedstatus = (int)verifiedStatus;
            }
            else
            {
                autoverifiedstatus = Convert.ToInt32(status);
            }
            return autoverifiedstatus;
            //Autoverification
            #endregion
        }
        #endregion
        #region
        public void SendErrorRFQNotification(string rfqNo, string customerName, string customeremailId,string vesselName,string emailTitle ,string emailbody)
        {
            try
            {
                string enviornment = this._Configuration.GetSection("MSDRFQNotification")["Name"];
                string portalenviornment = this._Configuration.GetSection("PortalURL")["Name"];
                string produrl = this._Configuration.GetSection("PortalURL")["PRODPortalURL"];
                string uaturl = this._Configuration.GetSection("PortalURL")["UATPortalURL"];
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("MSDRFQNotification")["FromMail"]);
                if (enviornment.ToUpper() == "PROD")
                {
                    if (!String.IsNullOrEmpty(customeremailId))
                    {
                        mail.To.Add(customeremailId);
                    }
                    else
                    {
                        mail.To.Add(this._Configuration.GetSection("MSDRFQNotification")["ToMail"]);
                    }
                }
                else
                {
                    mail.To.Add(this._Configuration.GetSection("MSDRFQNotification")["ToMail"]);
                }
                mail.CC.Add(this._Configuration.GetSection("MSDRFQNotification")["CCMail"]);
                string url = "";
                if (portalenviornment.ToUpper() == "PROD")
                {
                    url = produrl;
                }
                else
                {
                    url = uaturl;
                }
                if (rfqNo != "")
                {
                    mail.Subject = enviornment + " " + "MSD Enquiry / RFQ : " + rfqNo + " / < " + vesselName + "> / Process Temporary Pending";
                    mail.Body = "<p>Hello Team,<br /><br />RFQ No : " + rfqNo + " was once processed by RPA Creation BOT but due to <br/> some error, it went to the error work basket. <br/> Flologic Team will check and resolve the error.<br/> Please consider as “Pending” at this moment of this email. RFQ will be processed <br/> after it is resolved. <br/> We will also let you know if there's anything to be done by the FTC Team.<br/></p> " +
                        "<p> RFQ Email Title : " + emailTitle + " <br/>" +
                        "<p> RFQ Email Body : " + emailbody + " <br/><br/>" +
                       "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                else
                {
                    mail.Subject = enviornment + " " + "MSD Enquiry / Customer Name : " + customerName + " / < " + vesselName + "> / Process Temporary Pending";
                    mail.Body = "<p>Hello Team,<br /><br />Customer Name : " + customerName + " was once processed by RPA Creation BOT but due to <br/> some error, it went to the error work basket. <br/> Flologic Team will check and resolve the error.<br/> Please consider as “Pending” at this moment of this email. RFQ will be processed <br/> after it is resolved. <br/> We will also let you know if there's anything to be done by the FTC Team.<br/></p> " +
                        "<p> RFQ Email Title : " + emailTitle + " <br/>" +
                        "<p> RFQ Email Body : " + emailbody + " <br/><br/>" +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("MSDRFQNotification")["Username"], this._Configuration.GetSection("MSDRFQNotification")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
        #region
        public void SendErrorVerifedRFQNotification(string rfqNo)
        {
            try
            {
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                string enviornment = this._Configuration.GetSection("ErrorRFQVerified")["Name"];
                mail.From = new MailAddress(this._Configuration.GetSection("ErrorRFQVerified")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("ErrorRFQVerified")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("ErrorRFQVerified")["CCMail"]);
                if (rfqNo != "")
                {
                    mail.Subject = enviornment + " " + "MSD" + " " + "Enquiry - RFQ No : " + rfqNo + " In ErrorWork Basket - after two times verifying.";
                    mail.Body = "<p>Hello Team,<br /><br />RFQ No : " + rfqNo + " is processed by RPA BOT .Please correct with the appropriate data .<br/></p> " +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("ErrorRFQVerified")["Username"], this._Configuration.GetSection("ErrorRFQVerified")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
            }
        }
        #endregion
    }
}
