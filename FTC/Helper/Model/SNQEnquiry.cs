using Helper.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Helper.Data.DataContextClass;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Globalization;
using static Helper.Model.SNQClassDeclarations;
using Helper.Interface;
using DuoVia.FuzzyStrings;
using static Helper.Enum.Status;
using Microsoft.AspNetCore.SignalR;
using Helper.Hub_Config;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
namespace Helper.Model
{
    public class SNQEnquiry : ISNQEnquiry
    {
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        statusCode notstartedStatus = statusCode.notStartedStatus;
        statusCode verifiedStatus = statusCode.verifiedStatus;
        statusCode inprocessStatus = statusCode.inprocessStatus;
        statusCode successStatus = statusCode.successStatus;
        statusCode errorStatus = statusCode.errorStatus;
        statusCode updatedStatus = statusCode.updatedStatus;
        statusCode forwardToManualStatus = statusCode.forwardToManualStatus;
        statusCode rejectedStatus = statusCode.rejectedStatus;
        public SNQEnquiry(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        //Get Items details Count for Error
        public int GetDetailCountforError(long PK_SNQENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == PK_SNQENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        //Items Details Count for Inprocess,Not Started
        public int GetDetailCountforOther(long PK_SNQENQUIRY_HDR_ID)
        {
            int DETAIL_COUNT = 0;
            DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == PK_SNQENQUIRY_HDR_ID);
            return DETAIL_COUNT;
        }
        //For portal 
        public List<Enquiryheaderdata> GetEnquiryHeader(int status)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (status == 3)
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                     join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                     join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                     from userm in usmt.DefaultIfEmpty()
                                     where (enqheader.STATUS == 3 || enqheader.STATUS == 2 || enqheader.STATUS == 6)
                                     select new
                                     {
                                         enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                     }).OrderByDescending(c => c.PK_SNQENQUIRY_HDR_ID).ToList();
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        // string[] dt = data.ENQUIRY_DATE.Split(" ");
                        Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                        string[] dt;
                        //  dt = data.ENQUIRY_DATE.Split(" ");
                        if (!string.IsNullOrEmpty(data.ENQUIRY_DATE))
                        {
                            if (data.ENQUIRY_DATE.Contains("/"))
                            {
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
                        objEnqheadr.PK_SNQENQUIRY_HDR_ID = data.PK_SNQENQUIRY_HDR_ID;
                        // objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                        objEnqheadr.enqrefNo = data.ENQREF_NO;
                        objEnqheadr.shipName = data.SHIP_NAME;
                        objEnqheadr.owner = data.OWNER;
                        objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                        objEnqheadr.status = data.STATUS_DESCRIPTION;
                        objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                        objEnqheadr.sourceType = data.SOURCE_TYPE;
                        if (data.STATUS_DESCRIPTION == "Success")
                        {
                            DateTime date1 = data.CREATED_DATE;
                            DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                            TimeSpan duration = date2.Subtract(date1);
                            objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                        }
                        objEnqheadr.quotationNo = data.QUOTATION_NO;
                        objEnqheadr.docPath = data.DOC_PATH;
                        objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_SNQENQUIRY_HDR_ID);
                        objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_SNQENQUIRY_HDR_ID);
                        if (data.OWNERSHIP != 0)
                        {
                            objEnqheadr.action = "YES";
                            // objEnqheadr.pkuserId = data.PK_USER_ID;
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
            else
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                     join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                     join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                     from userm in usmt.DefaultIfEmpty()
                                     where enqheader.STATUS == status
                                     select new
                                     {
                                         enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                     }).OrderByDescending(c => c.PK_SNQENQUIRY_HDR_ID).ToList();
                if (EnqHeaderdata.Count > 0)
                {
                    foreach (var data in EnqHeaderdata)
                    {
                        Enquiryheaderdata objEnqheadr = new Enquiryheaderdata();
                        string[] dt;
                        //  dt = data.ENQUIRY_DATE.Split(" ");
                        if (!string.IsNullOrEmpty(data.ENQUIRY_DATE))
                        {
                            if (data.ENQUIRY_DATE.Contains("/"))
                            {
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
                        objEnqheadr.PK_SNQENQUIRY_HDR_ID = data.PK_SNQENQUIRY_HDR_ID;
                        // objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                        objEnqheadr.enqrefNo = data.ENQREF_NO;
                        objEnqheadr.shipName = data.SHIP_NAME;
                        objEnqheadr.owner = data.OWNER;
                        objEnqheadr.ownerEmailid = data.OWNER_EMAILID;
                        objEnqheadr.status = data.STATUS_DESCRIPTION;
                        objEnqheadr.saveAsDraft = data.SAVE_AS_DRAFT;
                        objEnqheadr.sourceType = data.SOURCE_TYPE;
                        if (data.STATUS_DESCRIPTION == "Success")
                        {
                            DateTime date1 = data.CREATED_DATE;
                            DateTime date2 = Convert.ToDateTime(data.QUOTATION_CREATED_AT);
                            TimeSpan duration = date2.Subtract(date1);
                            objEnqheadr.duration = duration.Minutes.ToString() + " " + "minutes";
                        }
                        objEnqheadr.quotationNo = data.QUOTATION_NO;
                        objEnqheadr.docPath = data.DOC_PATH;
                        objEnqheadr.TotalNoOfErrorItems = GetDetailCountforError(data.PK_SNQENQUIRY_HDR_ID);
                        objEnqheadr.TotalNoOfItems = GetDetailCountforOther(data.PK_SNQENQUIRY_HDR_ID);
                        if (data.OWNERSHIP != 0)
                        {
                            objEnqheadr.action = "YES";
                            //  objEnqheadr.pkuserId = data.PK_USER_ID;
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
            return lstEnqheader;
        }
        //For portal
        public Enquiryheaderdata GetEnquiryDetails(long fkenquiryid)
        {
            Enquiryheaderdata lstEnqhdrDetails = new Enquiryheaderdata();
            var HdrData = (from enquiryhdr in _datacontext.TSNQ_ENQUIRY_HDRTable where enquiryhdr.PK_SNQENQUIRY_HDR_ID == fkenquiryid select enquiryhdr).ToList();
            if (HdrData.Count > 0)
            {
                var EnquiryHdrdata = HdrData.First();
                if (EnquiryHdrdata.STATUS == 5)
                {
                    lstEnqhdrDetails = new Enquiryheaderdata
                    {
                        PK_SNQENQUIRY_HDR_ID = EnquiryHdrdata.PK_SNQENQUIRY_HDR_ID,
                        enquiryDate = EnquiryHdrdata.ENQUIRY_DATE,
                        enqrefNo = EnquiryHdrdata.ENQREF_NO,
                        shipName = EnquiryHdrdata.SHIP_NAME,
                        owner = EnquiryHdrdata.OWNER,
                        ownerEmailid = EnquiryHdrdata.OWNER_EMAILID,
                        status = (from statusmst in _datacontext.M_STATUS_CODE where EnquiryHdrdata.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                        quotationNo = EnquiryHdrdata.QUOTATION_NO,
                        docPath = EnquiryHdrdata.DOC_PATH,
                        errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where EnquiryHdrdata.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                        port = EnquiryHdrdata.PORT,
                        // deliveryDate = ParseStringtoDate(EnquiryHdrdata.DELIVERY_DATE).ToString(),
                        deliveryDate = (EnquiryHdrdata.DELIVERY_DATE == "" || EnquiryHdrdata.DELIVERY_DATE == null) ? "" : ParseStringToDate.ParseStringtoDate(EnquiryHdrdata.DELIVERY_DATE).ToString("dd/MM/yyyy"),
                        mappingPort = EnquiryHdrdata.AS400_MAPPING_PORT,
                        emailReceiveddate = Convert.ToDateTime(EnquiryHdrdata.EMAIL_RECEIVED_AT).ToString("MM/dd/yyyy hh:mm tt"),
                        sourceType = EnquiryHdrdata.SOURCE_TYPE,
                    };
                    var Dtldata = (from enquirydtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                   where enquirydtl.FK_SNQENQUIRY_HDR_ID == lstEnqhdrDetails.PK_SNQENQUIRY_HDR_ID
                                   select new
                                   {
                                       enquirydtl.PK_SNQENQUIRY_DTL_ID,
                                       enquirydtl.FK_SNQENQUIRY_HDR_ID,
                                       enquirydtl.PART_CODE,
                                       enquirydtl.PART_NAME,
                                       enquirydtl.QUANTITY,
                                       enquirydtl.UNIT,
                                       enquirydtl.PRICE,
                                       enquirydtl.STATUS,
                                       enquirydtl.SEQ_NO,
                                       enquirydtl.ERROR_CODE,
                                       enquirydtl.ACCOUNT_NO,
                                       enquirydtl.ACCOUNT_DESCRIPTION,
                                       enquirydtl.SEQ_NOTEXT
                                   });
                    Dtldata = Dtldata.Where(x => x.STATUS == 0 || x.STATUS == 1 || x.STATUS == 2 || x.STATUS == 5 || x.STATUS == 6);
                    var objdtlData = Dtldata.ToList();
                    List<Enquirydetailsdata> lstClsEnquiryDtl = new List<Enquirydetailsdata>();
                    if (objdtlData.Count > 0)
                    {
                        foreach (var item in objdtlData)
                        {
                            //bool isSelected = true;
                            Enquirydetailsdata objEnqDetails = new Enquirydetailsdata
                            {
                                PK_SNQENQUIRY_DTL_ID = item.PK_SNQENQUIRY_DTL_ID,
                                fkEnquiryid = item.FK_SNQENQUIRY_HDR_ID.ToString(),
                                partCode = item.PART_CODE,
                                partName = item.PART_NAME,
                                quantity = item.QUANTITY,
                                unit = item.UNIT,
                                price = item.PRICE,
                                status = (from statusmst in _datacontext.M_STATUS_CODE where item.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                                seqNo = item.SEQ_NO.ToString(),
                                errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where item.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                                accountNo = item.ACCOUNT_NO,
                                accountDescription = item.ACCOUNT_DESCRIPTION,
                                isSelected = true,
                                seqNoText = item.SEQ_NOTEXT
                            };
                            lstClsEnquiryDtl.Add(objEnqDetails);
                        }
                        lstEnqhdrDetails.itemDetails = lstClsEnquiryDtl;
                    }
                }
                else
                {
                    string dt;
                    lstEnqhdrDetails = new Enquiryheaderdata
                    {
                        PK_SNQENQUIRY_HDR_ID = EnquiryHdrdata.PK_SNQENQUIRY_HDR_ID,
                        enquiryDate = EnquiryHdrdata.ENQUIRY_DATE,
                        enqrefNo = EnquiryHdrdata.ENQREF_NO,
                        shipName = EnquiryHdrdata.SHIP_NAME,
                        owner = EnquiryHdrdata.OWNER,
                        ownerEmailid = EnquiryHdrdata.OWNER_EMAILID,
                        status = (from statusmst in _datacontext.M_STATUS_CODE where EnquiryHdrdata.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                        quotationNo = EnquiryHdrdata.QUOTATION_NO,
                        docPath = EnquiryHdrdata.DOC_PATH,
                        errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where EnquiryHdrdata.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                        port = EnquiryHdrdata.PORT,
                        deliveryDate = (EnquiryHdrdata.DELIVERY_DATE == "" || EnquiryHdrdata.DELIVERY_DATE == null) ? "" : ParseStringToDate.ParseStringtoDate(EnquiryHdrdata.DELIVERY_DATE).ToString("dd/MM/yyyy"),
                        // DateConversion(EnquiryHdrdata.DELIVERY_DATE),
                        mappingPort = EnquiryHdrdata.AS400_MAPPING_PORT,
                        emailReceiveddate = Convert.ToDateTime(EnquiryHdrdata.EMAIL_RECEIVED_AT).ToString("MM/dd/yyyy hh:mm tt"),
                        sourceType = EnquiryHdrdata.SOURCE_TYPE,
                    };
                    var Dtldata = (from enquirydtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                   where enquirydtl.FK_SNQENQUIRY_HDR_ID == lstEnqhdrDetails.PK_SNQENQUIRY_HDR_ID
                                   select new
                                   {
                                       enquirydtl.PK_SNQENQUIRY_DTL_ID,
                                       enquirydtl.FK_SNQENQUIRY_HDR_ID,
                                       enquirydtl.PART_CODE,
                                       enquirydtl.PART_NAME,
                                       enquirydtl.QUANTITY,
                                       enquirydtl.UNIT,
                                       enquirydtl.PRICE,
                                       enquirydtl.STATUS,
                                       enquirydtl.SEQ_NO,
                                       enquirydtl.ERROR_CODE,
                                       enquirydtl.ACCOUNT_NO,
                                       enquirydtl.ACCOUNT_DESCRIPTION,
                                       enquirydtl.SEQ_NOTEXT
                                   }).ToList();
                    //Dtldata = Dtldata.Where(x => x.STATUS == 2 || x.STATUS == 5 || x.STATUS == 6);
                    //var objdtlData = Dtldata.ToList();
                    List<Enquirydetailsdata> lstClsEnquiryDtl = new List<Enquirydetailsdata>();
                    if (Dtldata.Count > 0)
                    {
                        foreach (var item in Dtldata)
                        {
                            //bool isSelected = true;
                            Enquirydetailsdata objEnqDetails = new Enquirydetailsdata
                            {
                                PK_SNQENQUIRY_DTL_ID = item.PK_SNQENQUIRY_DTL_ID,
                                fkEnquiryid = item.FK_SNQENQUIRY_HDR_ID.ToString(),
                                partCode = item.PART_CODE,
                                partName = item.PART_NAME,
                                quantity = item.QUANTITY,
                                unit = item.UNIT,
                                price = item.PRICE,
                                status = (from statusmst in _datacontext.M_STATUS_CODE where item.STATUS == statusmst.STATUS_CODE select statusmst.STATUS_DESCRIPTION).SingleOrDefault(),
                                seqNo = item.SEQ_NO.ToString(),
                                errorCode = (from errcodmst in _datacontext.M_ERROR_CODE where item.ERROR_CODE == errcodmst.ERROR_CODE select errcodmst.ERROR_DESCRIPTION).SingleOrDefault(),
                                accountNo = item.ACCOUNT_NO,
                                accountDescription = item.ACCOUNT_DESCRIPTION,
                                isSelected = true,
                                seqNoText = item.SEQ_NOTEXT
                            };
                            lstClsEnquiryDtl.Add(objEnqDetails);
                        }
                        lstEnqhdrDetails.itemDetails = lstClsEnquiryDtl;
                    }
                }
            }
            return lstEnqhdrDetails;
        }
        //From Bot
        public MessageSNQ InsertEnquiry(Enquiryheader objEnquiry)
        {
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while Creating Enquiry";
            int isUpdatedWithML = 0;
            string defaultAccountCode = this._Configuration.GetSection("DefaultAccountCode")["OtherAccountCode"];
            string jpnOwner = this._Configuration.GetSection("Owners")["JPNOwner"];
            string europOwner = this._Configuration.GetSection("Owners")["EUROPEOwner"];
            string hkOwner = this._Configuration.GetSection("Owners")["HKOwner"];
            string duplicateEntry = this._Configuration.GetSection("DuplicateEnquirySNQ")["DuplicateAllow"];
            string MLLogicSNQ = this._Configuration.GetSection("MLLogicSNQ")["MLAllow"];
            string SourceType = this._Configuration.GetSection("SourceType")["SourceTypeName"];
            try
            {
                if (objEnquiry != null)
                {
                    #region DuplicateEntry
                    //Duplicateentry Start
                    if (objEnquiry.enqrefNo != "")
                    {
                            var DuplicateEntry = (from hdrdata in _datacontext.TSNQ_ENQUIRY_HDRTable
                                                  where hdrdata.ENQREF_NO == objEnquiry.enqrefNo.Trim() && hdrdata.STATUS != 8
                                                  select new
                                                  {
                                                      hdrdata.ENQREF_NO
                                                  }).ToList();
                            //Duplicateentry End
                            #endregion DuplicateEntry
                            if (duplicateEntry.ToUpper() == "YES" || DuplicateEntry.Count == 0)
                            {
                                string strPath = objEnquiry.docPath;
                                //For partcode ml
                                var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                               where part.IS_ACTIVE == 1
                                               select new
                                               {
                                                   part.PART_NAME,
                                                   part.PART_CODE
                                               }).Distinct().ToList();
                                //For Unit ml
                                var lstunit = (from unit in _datacontext.MUOMTable
                                               select new
                                               {
                                                   unit.TEMPLATE_UOM,
                                                   unit.AS400_UOM
                                               }).Distinct().ToList();
                                TSNQ_ENQUIRY_HDR_ORG objenqheaderorg = new TSNQ_ENQUIRY_HDR_ORG();
                                objenqheaderorg.FK_PROCESS_ID = 1;
                                objenqheaderorg.FK_INSTANCE_ID = 1;
                                objenqheaderorg.ENQUIRY_DATE = objEnquiry.enquiryDate;
                                objenqheaderorg.OWNER = objEnquiry.owner;
                                objenqheaderorg.OWNER_EMAILID = objEnquiry.ownerEmailid;
                                objenqheaderorg.SHIP_NAME = objEnquiry.shipName;
                                objenqheaderorg.ENQREF_NO = objEnquiry.enqrefNo;
                                objenqheaderorg.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISSNQDocumentFolder"] + "" + Path.GetFileName(strPath);
                                objenqheaderorg.CREATED_BY = 1;
                                objenqheaderorg.CREATED_DATE = DateTime.Now;
                                objenqheaderorg.ERROR_CODE = objEnquiry.errorCode;
                                objenqheaderorg.MAKER = objEnquiry.maker;
                                objenqheaderorg.TYPE = objEnquiry.type;
                                objenqheaderorg.EQUIPMENT = objEnquiry.equipment;
                                objenqheaderorg.SERIAL_NO = objEnquiry.serialNo;
                                objenqheaderorg.DISCOUNT_AMOUNT = objEnquiry.discountAmount;
                                objenqheaderorg.NET_AMOUNT = objEnquiry.netAmount;
                                objenqheaderorg.PORT = objEnquiry.port;
                                objenqheaderorg.DELIVERY_DATE = objEnquiry.deliveryDate;
                                objenqheaderorg.AS400_MAPPING_PORT = GetPortMapping(objEnquiry.shipName, objEnquiry.port);
                                if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                                {
                                    objenqheaderorg.SOURCE_TYPE = objEnquiry.sourceType;
                                }
                                else
                                {
                                    objenqheaderorg.SOURCE_TYPE = SourceType;
                                }
                                _datacontext.TSNQ_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                                _datacontext.SaveChanges();
                                long PKENQHDRORGID = objenqheaderorg.PK_SNQENQUIRY_HDR_ID;
                                TSNQ_ENQUIRY_HDR objenqheader = new TSNQ_ENQUIRY_HDR();
                                objenqheader.FK_PROCESS_ID = 1;
                                objenqheader.FK_INSTANCE_ID = 1;
                                objenqheader.ENQUIRY_DATE = objEnquiry.enquiryDate;
                                objenqheader.OWNER = GetCustomerMapping(objEnquiry.owner);
                                objenqheader.OWNER_EMAILID = objEnquiry.ownerEmailid;
                                objenqheader.SHIP_NAME = objEnquiry.shipName;
                                objenqheader.ENQREF_NO = objEnquiry.enqrefNo;
                                objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISSNQDocumentFolder"] + "" + Path.GetFileName(strPath);
                                objenqheader.STATUS = 0;//GetAutoverification(objEnquiry.owner, objEnquiry.status);//Convert.ToInt32(objEnquiry.status);
                                objenqheader.QUOTATION_NO = objEnquiry.quotationNo;
                                objenqheader.CREATED_BY = 1;
                                objenqheader.CREATED_DATE = DateTime.Now;
                                objenqheader.ERROR_CODE = objEnquiry.errorCode;
                                objenqheader.MAKER = objEnquiry.maker;
                                objenqheader.TYPE = objEnquiry.type;
                                objenqheader.EQUIPMENT = objEnquiry.equipment;
                                objenqheader.SERIAL_NO = objEnquiry.serialNo;
                                objenqheader.DISCOUNT_AMOUNT = objEnquiry.discountAmount;
                                objenqheader.NET_AMOUNT = objEnquiry.netAmount;
                                objenqheader.EMAIL_RECEIVED_AT = objEnquiry.emailReceivedat;
                                objenqheader.EMAIL_PROCESSED_AT = objEnquiry.emailProcessedat;
                                objenqheader.IN_ERROR_AT = "";
                                objenqheader.VERIFIED_AT = "";
                                objenqheader.UPDATED_AT = "";
                                objenqheader.QUOTATION_CREATED_AT = "";
                                objenqheader.SAVE_AS_DRAFT = "";
                                objenqheader.PORT = objEnquiry.port;
                                objenqheader.DELIVERY_DATE = objEnquiry.deliveryDate;
                                objenqheader.AS400_MAPPING_PORT = GetPortMapping(objEnquiry.shipName, objEnquiry.port);
                                objenqheader.MAIL_BODY = objEnquiry.mailBody;
                                objenqheader.MAIL_SUBJECT = objEnquiry.emailSubject;
                                objenqheader.RFQ_URL = objEnquiry.rfqUrl;
                                objenqheader.OWNERSHIP = 0;
                                objenqheader.ERROR_VERIFIED_COUNTER = 0;
                                if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                                {
                                    objenqheader.SOURCE_TYPE = objEnquiry.sourceType;
                                    objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = objEnquiry.IsUpdatedEquipmentWithML;
                                    objenqheader.IS_UPDATED_MAKER_WITH_ML = objEnquiry.IsUpdatedMakerWithML;
                                }
                                else
                                {
                                    objenqheader.SOURCE_TYPE = SourceType;
                                    objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = 0;
                                    objenqheader.IS_UPDATED_MAKER_WITH_ML = 0;
                                }
                                if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                                {
                                    objenqheader.EVENT_ID = objEnquiry.eventId;
                                }
                                else
                                {
                                    objenqheader.EVENT_ID = 0;
                                }
                                _datacontext.TSNQ_ENQUIRY_HDRTable.Add(objenqheader);
                                _datacontext.SaveChanges();
                                long PKENQHDRID = objenqheader.PK_SNQENQUIRY_HDR_ID;
                               /* if (objenqheader.STATUS == (int)verifiedStatus)
                                {
                                    string verfiedBy = this._Configuration.GetSection("BOTVerfiied")["VerifiedBy"];
                                    TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == PKENQHDRID select hdr).FirstOrDefault();
                                    if (headerobj != null)
                                    {
                                        headerobj.VERIFIED_BY = verfiedBy;
                                        _datacontext.SaveChanges();
                                    }
                                }*/
                                //save multiple header levels document path
                                if (objEnquiry.docHdrDetails != null && PKENQHDRID != 0)
                                {
                                    foreach (var item in objEnquiry.docHdrDetails)
                                    {
                                        T_SNQ_DOCUMENT_HDR objenqdetails = new T_SNQ_DOCUMENT_HDR();
                                        {
                                            objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                            objenqdetails.DOC_PATH = item.docPath;
                                            objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                            objenqdetails.CREATEBY = 1;
                                            objenqdetails.CREATEDAT = DateTime.Now;
                                            objenqdetails.ISACTIVE = 1;
                                        };
                                        _datacontext.T_SNQ_DOCUMENT_HDRTable.Add(objenqdetails);
                                        _datacontext.SaveChanges();
                                    }
                                }
                             if (objEnquiry.itemDetails != null && PKENQHDRID != 0)
                                {
                                    int count = 0;
                                    foreach (var item in objEnquiry.itemDetails)
                                    {
                                        #region
                                        TSNQ_ENQUIRY_DTL_ORG objenqdetailsorg = new TSNQ_ENQUIRY_DTL_ORG();
                                        {
                                            objenqdetailsorg.FK_SNQENQUIRY_HDR_ID = PKENQHDRORGID;
                                            objenqdetailsorg.PART_CODE = item.partCode;
                                            objenqdetailsorg.PART_NAME = item.partName;
                                            objenqdetailsorg.QUANTITY = item.quantity;
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
                                        #endregion
                                        //ML Logic For PartCode/Impa Code
                                        #region ML Logic to find IMPA code
                                        string partCode = item.partCode;
                                        string partName = item.partName;
                                        if (partCode == null)
                                        {
                                            partCode = " ";
                                        }
                                        if (MLLogicSNQ == "YES")
                                        {
                                            if ((GetCustomerMapping(objEnquiry.owner) == jpnOwner
                                                || GetCustomerMapping(objEnquiry.owner) == europOwner ||
                                                GetCustomerMapping(objEnquiry.owner) == hkOwner) && partCode.Length > 6)
                                            {
                                                partName = partName + " " + item.partCode;
                                                partCode = "";
                                            }
                                            else if ((partCode == null || partCode.Length != 6) && !string.IsNullOrEmpty(partName))
                                            {
                                                double percentage = 0;
                                                string matchingPercent = this._Configuration.GetSection("MatchingPercent")["MATCHINGVALUE"];
                                                for (int counter = 0; counter < lstpart.Count; counter++)
                                                {
                                                    var lcs = partName.LongestCommonSubsequence(lstpart[counter].PART_NAME);
                                                    if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2 && partCode.Length <= lstpart[counter].PART_CODE.Length)
                                                    {
                                                        //Logger1.Activity("Part Name:" + " " + partName + "," + " Previous Part Code:" + " " + partCode + "," + " New Part Code:" + " " + lstpart[counter].PARTCODE + "," + " Percentage:" + lcs.Item2 + "," + " " + " LCS string" + lcs.Item1);
                                                        percentage = lcs.Item2;
                                                        partCode = lstpart[counter].PART_CODE;
                                                        isUpdatedWithML = 1;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                partCode = item.partCode;
                                            }
                                        }
                                        #endregion ML Logic to find IMPA code
                                        //ML Logic For PartCode/Impa Code
                                        //unit mapping
                                        #region unit manipulation
                                        string AS400unit = "";
                                        string ItmDescwithUnit = "";
                                        if (!string.IsNullOrEmpty(item.unit))
                                        {
                                            var UnitOM = (from hdr in _datacontext.MUOMTable
                                                          where hdr.TEMPLATE_UOM.ToLower() == item.unit.ToString().ToLower()
                                                          select hdr.AS400_UOM).ToList();
                                            if (UnitOM.Count > 0)
                                            {
                                                AS400unit = UnitOM[0];
                                                ItmDescwithUnit = partName;
                                            }
                                            else if (item.unit.ToString().Length > 3)
                                            {
                                                ItmDescwithUnit = partName + " " + item.unit;
                                                double percentage = 0;
                                                string matchingPercent = this._Configuration.GetSection("MatchingPercentForUnit")["MATCHINGVALUEFORUNIT"];
                                                if (lstunit != null)
                                                {
                                                    for (int counter = 0; counter < lstunit.Count; counter++)
                                                    {
                                                        var lcs = item.unit.ToString().ToUpper().Substring(0, 4).LongestCommonSubsequence(lstunit[counter].TEMPLATE_UOM.ToUpper());
                                                        if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                                        {
                                                            percentage = lcs.Item2;
                                                            AS400unit = lstunit[counter].AS400_UOM;
                                                        }
                                                    }
                                                }
                                                if (AS400unit == "")
                                                {
                                                    AS400unit = item.unit;
                                                }
                                            }
                                            else
                                            {
                                                AS400unit = item.unit;
                                                ItmDescwithUnit = partName;
                                            }
                                        }
                                        else
                                        {
                                            AS400unit = item.unit;
                                            ItmDescwithUnit = partName;
                                        }
                                        #endregion unit manipulation
                                        //end unit mapping
                                        #region
                                        string quantity = "";
                                        if (item.quantity.Contains(","))
                                        {
                                            quantity = item.quantity.Replace(",", "");
                                        }
                                        else
                                        {
                                            quantity = item.quantity.Trim();
                                        }
                                        int qty = quantity.Count(x => x == '.');
                                        #endregion
                                        #region
                                        if (qty > 1)
                                        {
                                            quantity = quantity.Remove(quantity.IndexOf('.'), 1);
                                        }
                                        else
                                        {
                                            quantity = item.quantity.Trim();
                                        }
                                        #endregion
                                        if (!String.IsNullOrEmpty(quantity))
                                        {
                                            string accno = GetAccountCodeMapping(item.accountNo, item.accountDescription, objEnquiry.owner);
                                            var query = (from seq in _datacontext.TSNQ_ENQUIRY_DTLTable where seq.ACCOUNT_NO == accno && seq.FK_SNQENQUIRY_HDR_ID == PKENQHDRID select seq.SEQ_NO);//.ToList().LastOrDefault();
                                            int maxseqNo;
                                            maxseqNo = query.Any() ? query.Max() : 0;
                                            if (maxseqNo == 0)
                                            {
                                                maxseqNo = 0;
                                            }
                                            maxseqNo = maxseqNo + 1;
                                            #region
                                            TSNQ_ENQUIRY_DTL objenqdetails = new TSNQ_ENQUIRY_DTL();
                                            {
                                                objenqdetails.FK_SNQENQUIRY_HDR_ID = PKENQHDRID;
                                                objenqdetails.PART_CODE = partCode;
                                                objenqdetails.PART_NAME = ItmDescwithUnit;
                                                objenqdetails.QUANTITY = quantity;
                                                objenqdetails.UNIT = AS400unit;
                                                objenqdetails.PRICE = item.price;
                                                objenqdetails.COST = item.cost;
                                                objenqdetails.UPDATED_DATE = DateTime.Now;
                                                objenqdetails.SUPPLIER = item.supplier;
                                                objenqdetails.STATUS = GetAutoverification(objenqheader.OWNER, objEnquiry.status);// Convert.ToInt32(item.status);
                                                objenqdetails.SEQ_NO = maxseqNo;//Convert.ToInt32(item.seqNo);
                                                objenqdetails.ERROR_CODE = item.errorCode;
                                                objenqdetails.ACCOUNT_NO = accno;
                                                objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                                objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                            };
                                            #endregion
                                            _datacontext.TSNQ_ENQUIRY_DTLTable.Add(objenqdetails);
                                            _datacontext.SaveChanges();
                                            #region
                                            long PKDTLID = objenqdetails.PK_SNQENQUIRY_DTL_ID;
                                            if (item.docdtlDetails != null && PKDTLID != 0)
                                            {
                                                foreach (var item2 in item.docdtlDetails)
                                                {
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
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                    // 10 items in enquiry. if in this enquiry 1 items has is different account code out of 10 then we set default account code
                                    if (count > 0)
                                    {
                                        var Dtldata = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                                       where dtl.FK_SNQENQUIRY_HDR_ID == PKENQHDRID
                                                       select new
                                                       {
                                                           dtl.PK_SNQENQUIRY_DTL_ID,
                                                           dtl.FK_SNQENQUIRY_HDR_ID,
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
                                                           dtl.SEQ_NOTEXT
                                                       }).ToList();
                                        if (Dtldata.Count > 0)
                                        {
                                            foreach (var item in Dtldata)
                                            {
                                                TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == item.PK_SNQENQUIRY_DTL_ID select dtl).FirstOrDefault();
                                                if (objdtl != null)
                                                {
                                                    objdtl.ACCOUNT_NO = defaultAccountCode;
                                                    _datacontext.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                            if (objEnquiry.itemDetails.Count > 0)
                            {
                                TSNQ_ENQUIRY_HDR objdt = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == PKENQHDRID select hdr).FirstOrDefault();
                                int currentstatus = GetAutoverification(objEnquiry.owner, objEnquiry.status);
                                objdt.STATUS = currentstatus;
                                _datacontext.SaveChanges();
                                #region
                                if (objdt.STATUS == (int)verifiedStatus)
                                {
                                    string verfiedBy = this._Configuration.GetSection("BOTVerfiied")["VerifiedBy"];
                                    TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == PKENQHDRID select hdr).FirstOrDefault();
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
                            obj.result = "Enquiry Saved Successfully";
                                _hub.Clients.All.BroadcastMessage();
                            }
                            else
                            {
                                obj.result = "Duplicate Entry";
                            }
                    }
                    else
                    {
                        string strPath = objEnquiry.docPath;
                        var lstpart = (from part in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                       where part.IS_ACTIVE == 1
                                       select new
                                       {
                                           part.PART_NAME,
                                           part.PART_CODE
                                       }).Distinct().ToList();
                        //For Unit ml
                        var lstunit = (from unit in _datacontext.MUOMTable
                                       select new
                                       {
                                           unit.TEMPLATE_UOM,
                                           unit.AS400_UOM
                                       }).Distinct().ToList();
                        TSNQ_ENQUIRY_HDR_ORG objenqheaderorg = new TSNQ_ENQUIRY_HDR_ORG();
                        objenqheaderorg.FK_PROCESS_ID = 1;
                        objenqheaderorg.FK_INSTANCE_ID = 1;
                        objenqheaderorg.ENQUIRY_DATE = objEnquiry.enquiryDate;
                        objenqheaderorg.OWNER = objEnquiry.owner;
                        objenqheaderorg.OWNER_EMAILID = objEnquiry.ownerEmailid;
                        objenqheaderorg.SHIP_NAME = objEnquiry.shipName;
                        objenqheaderorg.ENQREF_NO = objEnquiry.enqrefNo;
                        objenqheaderorg.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISSNQDocumentFolder"] + "" + Path.GetFileName(strPath);
                        objenqheaderorg.CREATED_BY = 1;
                        objenqheaderorg.CREATED_DATE = DateTime.Now;
                        objenqheaderorg.ERROR_CODE = objEnquiry.errorCode;
                        objenqheaderorg.MAKER = objEnquiry.maker;
                        objenqheaderorg.TYPE = objEnquiry.type;
                        objenqheaderorg.EQUIPMENT = objEnquiry.equipment;
                        objenqheaderorg.SERIAL_NO = objEnquiry.serialNo;
                        objenqheaderorg.DISCOUNT_AMOUNT = objEnquiry.discountAmount;
                        objenqheaderorg.NET_AMOUNT = objEnquiry.netAmount;
                        objenqheaderorg.PORT = objEnquiry.port;
                        objenqheaderorg.DELIVERY_DATE = objEnquiry.deliveryDate;
                        objenqheaderorg.AS400_MAPPING_PORT = GetPortMapping(objEnquiry.shipName, objEnquiry.port);
                        if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheaderorg.SOURCE_TYPE = "MESPAS";
                        }
                        else
                        {
                            objenqheaderorg.SOURCE_TYPE = SourceType;
                        }
                        _datacontext.TSNQ_ENQUIRY_HDR_ORGTable.Add(objenqheaderorg);
                        _datacontext.SaveChanges();
                        long PKENQHDRORGID = objEnquiry.PK_SNQENQUIRY_HDR_ID;
                        TSNQ_ENQUIRY_HDR objenqheader = new TSNQ_ENQUIRY_HDR();
                        objenqheader.FK_PROCESS_ID = 1;
                        objenqheader.FK_INSTANCE_ID = 1;
                        objenqheader.ENQUIRY_DATE = objEnquiry.enquiryDate;
                        objenqheader.OWNER = GetCustomerMapping(objEnquiry.owner);
                        objenqheader.OWNER_EMAILID = objEnquiry.ownerEmailid;
                        objenqheader.SHIP_NAME = objEnquiry.shipName;
                        objenqheader.ENQREF_NO = objEnquiry.enqrefNo;
                        if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISMESPASDocumentFolder"] + "" + Path.GetFileName(strPath);
                        }
                        else
                        {
                            objenqheader.DOC_PATH = this._Configuration.GetSection("DocumentPath")["IISSNQDocumentFolder"] + "" + Path.GetFileName(strPath);
                        }
                        objenqheader.STATUS = 5;
                        objenqheader.QUOTATION_NO = objEnquiry.quotationNo;
                        objenqheader.CREATED_BY = 1;
                        objenqheader.CREATED_DATE = DateTime.Now;
                        objenqheader.ERROR_CODE = objEnquiry.errorCode;
                        objenqheader.MAKER = objEnquiry.maker;
                        objenqheader.TYPE = objEnquiry.type;
                        objenqheader.EQUIPMENT = objEnquiry.equipment;
                        objenqheader.SERIAL_NO = objEnquiry.serialNo;
                        objenqheader.DISCOUNT_AMOUNT = objEnquiry.discountAmount;
                        objenqheader.NET_AMOUNT = objEnquiry.netAmount;
                        objenqheader.EMAIL_RECEIVED_AT = objEnquiry.emailReceivedat;
                        objenqheader.EMAIL_PROCESSED_AT = objEnquiry.emailProcessedat;
                        objenqheader.IN_ERROR_AT = "";
                        objenqheader.VERIFIED_AT = "";
                        objenqheader.UPDATED_AT = "";
                        objenqheader.QUOTATION_CREATED_AT = "";
                        objenqheader.SAVE_AS_DRAFT = "";
                        objenqheader.PORT = objEnquiry.port;
                        objenqheader.DELIVERY_DATE = objEnquiry.deliveryDate;
                        objenqheader.AS400_MAPPING_PORT = GetCustomerMapping(objEnquiry.owner);
                        objenqheader.MAIL_BODY = objEnquiry.mailBody;
                        objenqheader.MAIL_SUBJECT = objEnquiry.emailSubject;
                        objenqheader.RFQ_URL = objEnquiry.rfqUrl;
                        objenqheader.ERROR_VERIFIED_COUNTER = 0;
                        if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheader.SOURCE_TYPE = objEnquiry.sourceType;
                            objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = objEnquiry.IsUpdatedEquipmentWithML;
                            objenqheader.IS_UPDATED_MAKER_WITH_ML = objEnquiry.IsUpdatedMakerWithML;
                        }
                        else
                        {
                            objenqheader.SOURCE_TYPE = SourceType;
                            objenqheader.IS_UPDATED_EQUIPMENT_WITH_ML = 0;
                            objenqheader.IS_UPDATED_MAKER_WITH_ML = 0;
                        }
                        if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                        {
                            objenqheader.EVENT_ID = objEnquiry.eventId;
                        }
                        else
                        {
                            objenqheader.EVENT_ID = 0;
                        }
                        _datacontext.TSNQ_ENQUIRY_HDRTable.Add(objenqheader);
                        _datacontext.SaveChanges();
                        long PKENQHDRID = objenqheader.PK_SNQENQUIRY_HDR_ID;
                        //save multiple header levels document path
                        if (objEnquiry.docHdrDetails != null && PKENQHDRID != 0)
                        {
                            foreach (var item in objEnquiry.docHdrDetails)
                            {
                                // if (item.docPath != "")
                                {
                                    T_SNQ_DOCUMENT_HDR objenqdetails = new T_SNQ_DOCUMENT_HDR();
                                    {
                                        objenqdetails.ENQUIRY_HDR_ID = PKENQHDRID;
                                        objenqdetails.DOC_PATH = item.docPath;
                                        objenqdetails.ERROR_DESCRIPTION = item.errorDescription;
                                        objenqdetails.CREATEBY = 1;
                                        objenqdetails.CREATEDAT = DateTime.Now;
                                        objenqdetails.ISACTIVE = 1;
                                    };
                                    _datacontext.T_SNQ_DOCUMENT_HDRTable.Add(objenqdetails);
                                    _datacontext.SaveChanges();
                                }
                            }
                        }
                        if (objEnquiry.itemDetails != null && PKENQHDRID != 0)
                        {
                            int count = 0;
                            foreach (var item in objEnquiry.itemDetails)
                            {
                                TSNQ_ENQUIRY_DTL_ORG objenqdetailsorg = new TSNQ_ENQUIRY_DTL_ORG();
                                {
                                    objenqdetailsorg.FK_SNQENQUIRY_HDR_ID = PKENQHDRORGID;
                                    objenqdetailsorg.PART_CODE = item.partCode;
                                    objenqdetailsorg.PART_NAME = item.partName;
                                    objenqdetailsorg.QUANTITY = item.quantity;
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
                                //ML Logic For PartCode/Impa Code
                                #region ML Logic to find IMPA code
                                string partCode = item.partCode;
                                string partName = item.partName;
                                if (MLLogicSNQ == "YES")
                                {
                                    if ((GetCustomerMapping(objEnquiry.owner) == jpnOwner
                           || GetCustomerMapping(objEnquiry.owner) == europOwner
                           || GetCustomerMapping(objEnquiry.owner) == hkOwner && partCode.Length > 6))
                                    {
                                        partName = partName + " " + item.partCode;
                                        partCode = "";
                                    }
                                    else if ((partCode == null || partCode.Length != 6) && !string.IsNullOrEmpty(partName))
                                    {
                                        double percentage = 0;
                                        string matchingPercent = this._Configuration.GetSection("MatchingPercent")["MATCHINGVALUE"];
                                        for (int counter = 0; counter < lstpart.Count; counter++)
                                        {
                                            var lcs = partName.LongestCommonSubsequence(lstpart[counter].PART_NAME);
                                            if (lcs.Item2 > double.Parse(matchingPercent) &&
                                                    percentage < lcs.Item2 &&
                                                    partCode.Length <= lstpart[counter].PART_CODE.Length)
                                            {
                                                //Logger1.Activity("Part Name:" + " " + partName + "," + " Previous Part Code:" + " " + partCode + "," + " New Part Code:" + " " + lstpart[counter].PARTCODE + "," + " Percentage:" + lcs.Item2 + "," + " " + " LCS string" + lcs.Item1);
                                                percentage = lcs.Item2;
                                                partCode = lstpart[counter].PART_CODE;
                                                isUpdatedWithML = 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        partCode = item.partCode;
                                    }
                                }
                                #endregion ML Logic to find IMPA code
                                //ML Logic For PartCode/Impa Code
                                //unit mapping
                                #region unit manipulation
                                string AS400unit = "";
                                string ItmDescwithUnit = "";
                                if (!string.IsNullOrEmpty(item.unit))
                                {
                                    var UnitOM = (from hdr in _datacontext.MUOMTable
                                                  where hdr.TEMPLATE_UOM.ToLower() == item.unit.ToString().ToLower()
                                                  select hdr.AS400_UOM).ToList();
                                    if (UnitOM.Count > 0)
                                    {
                                        AS400unit = UnitOM[0];
                                        ItmDescwithUnit = partName;
                                    }
                                    else if (item.unit.ToString().Length > 3)
                                    {
                                        ItmDescwithUnit = partName + " " + item.unit;
                                        double percentage = 0;
                                        string matchingPercent = this._Configuration.GetSection("MatchingPercentForUnit")["MATCHINGVALUEFORUNIT"];
                                        for (int counter = 0; counter < lstunit.Count; counter++)
                                        {
                                            var lcs = item.unit.ToString().ToUpper().Substring(0, 4).LongestCommonSubsequence(lstunit[counter].TEMPLATE_UOM.ToUpper());
                                            if (lcs.Item2 > double.Parse(matchingPercent) && percentage < lcs.Item2)
                                            {
                                                percentage = lcs.Item2;
                                                AS400unit = lstunit[counter].AS400_UOM;
                                                isUpdatedWithML = 2; //flag for unitML
                                            }
                                        }
                                        if (AS400unit == "")
                                        {
                                            AS400unit = item.unit;
                                        }
                                    }
                                    else
                                    {
                                        AS400unit = item.unit;
                                        ItmDescwithUnit = partName;
                                    }
                                }
                                else
                                {
                                    AS400unit = item.unit;
                                    ItmDescwithUnit = partName;
                                }
                                #endregion unit manipulation
                                //end unit mapping
                                string quantity = "";
                                if (item.quantity.Contains(","))
                                {
                                    quantity = item.quantity.Replace(",", "");
                                }
                                else
                                {
                                    quantity = item.quantity.Trim();
                                }
                                int qty = quantity.Count(x => x == '.');
                                if (qty > 1)
                                {
                                    quantity = quantity.Remove(quantity.IndexOf('.'), 1);
                                }
                                else
                                {
                                    quantity = item.quantity.Trim();
                                }
                                if (!String.IsNullOrEmpty(quantity))
                                {
                                    TSNQ_ENQUIRY_DTL objenqdetails = new TSNQ_ENQUIRY_DTL();
                                    {
                                        objenqdetails.FK_SNQENQUIRY_HDR_ID = PKENQHDRID;
                                        objenqdetails.PART_CODE = partCode;
                                        objenqdetails.PART_NAME = ItmDescwithUnit;
                                        objenqdetails.QUANTITY = quantity;
                                        objenqdetails.UNIT = AS400unit;
                                        objenqdetails.PRICE = item.price;
                                        objenqdetails.COST = item.cost;
                                        objenqdetails.UPDATED_DATE = DateTime.Now;
                                        objenqdetails.SUPPLIER = item.supplier;
                                        objenqdetails.STATUS = 5;
                                        objenqdetails.SEQ_NO = Convert.ToInt32(item.seqNo);
                                        objenqdetails.ERROR_CODE = item.errorCode;
                                        objenqdetails.ACCOUNT_NO = GetAccountCodeMapping(item.accountNo, item.accountDescription, objEnquiry.owner);
                                        // objenqdetails.ACCOUNT_NO = GetAccountCodeMapping(partCode, objEnquiry.mailSubject, item.accountDescription) ;
                                        objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                        objenqdetails.IS_UPDATED_WITH_ML = isUpdatedWithML;
                                        if (objEnquiry.sourceType.ToUpper() == "MESPAS")
                                        {
                                            objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = Convert.ToInt32(item.IsUpdatedMESPASItemsWithML);
                                        }
                                        else
                                        {
                                            objenqdetails.IS_UPDATED_MESPASITEMS_WITH_ML = 0;
                                        }
                                    }
                                    _datacontext.TSNQ_ENQUIRY_DTLTable.Add(objenqdetails);
                                    _datacontext.SaveChanges();
                                    long PKDTLID = objenqdetails.PK_SNQENQUIRY_DTL_ID;
                                    if (objEnquiry.itemDetails != null)
                                    {
                                        if (item.docdtlDetails != null)
                                        {
                                            foreach (var item2 in item.docdtlDetails)
                                            {
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
                                            }
                                        }
                                    }
                                };
                            }
                        }
                        obj.result = "Enquiry Saved Successfully";
                        string ownerEmailId = GetCustomerEmailMapping(objenqheader.OWNER);
                        SendErrorRFQNotification(objEnquiry.enqrefNo, objEnquiry.owner, objEnquiry.ownerEmailid, objEnquiry.shipName, objEnquiry.quotationNo);
                        _hub.Clients.All.BroadcastMessage();
                    }
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //From portal
        public MessageSNQ UpdateEnquiry(Enquiryheader objEnquiry)
        {
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while Updating Enquiry";
            string otherAccountCode = this._Configuration.GetSection("DefaultAccountCode")["OtherAccountCode"];
            try
            {
                if (objEnquiry != null)
                {
                    TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == objEnquiry.PK_SNQENQUIRY_HDR_ID select hdr).FirstOrDefault();
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
                    headerobj.PORT = objEnquiry.port;
                    headerobj.AS400_MAPPING_PORT = objEnquiry.mappingPort;
                    headerobj.DELIVERY_DATE = objEnquiry.deliveryDate;
                    headerobj.OWNERSHIP = 0;
                    if (Convert.ToInt32(objEnquiry.status) != 8)//Rejected
                    {
                        if (headerobj.STATUS == 1)  //not started
                        {
                            headerobj.STATUS = 2; //Verified
                            headerobj.SAVE_AS_DRAFT = "";
                            headerobj.VERIFIED_AT = DateTime.Now.ToString();
                            headerobj.VERIFIED_BY = objEnquiry.verifiedBy;
                        }
                        else if (headerobj.STATUS == 5)
                        {
                            headerobj.STATUS = 6; // updated
                            headerobj.SAVE_AS_DRAFT = "";
                            headerobj.UPDATED_AT = DateTime.Now.ToString();
                            headerobj.CORRECTED_BY = objEnquiry.correctedBy;
                        }
                        else if (headerobj.STATUS == 7)
                        {
                            headerobj.STATUS = 7; //forwordtomanual
                        }
                        else if (headerobj.STATUS == 2 || headerobj.STATUS == 6)
                        {
                            headerobj.STATUS = 4; //Success
                            headerobj.UPDATED_AT = DateTime.Now.ToString();
                            headerobj.QUOTATION_CREATED_AT = objEnquiry.quotationCreatedat;
                        }
                        else
                        {
                            headerobj.STATUS = 5;//error
                            headerobj.ERROR_CODE = objEnquiry.errorCode;
                            headerobj.IN_ERROR_AT = DateTime.Now.ToString();
                            string ownerEmailId = GetCustomerEmailMapping(headerobj.OWNER);
                            SendErrorRFQNotification(objEnquiry.enqrefNo, objEnquiry.owner, objEnquiry.ownerEmailid, objEnquiry.shipName, objEnquiry.quotationNo);
                        }
                    }
                    else
                    {
                        headerobj.STATUS = 8;
                    }
                    _datacontext.SaveChanges();
                    int count = 0;
                    if (objEnquiry.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (item.accountNo == otherAccountCode)
                            {
                                count = 1;
                            }
                            TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == item.PK_SNQENQUIRY_DTL_ID select dtl).FirstOrDefault();
                            if (objdtl != null)
                            {
                                if (objdtl.UNIT.Length > 3)
                                {
                                    var UnitMlData = (from mlunit in _datacontext.MUOMTable
                                                      where (mlunit.TEMPLATE_UOM == objdtl.UNIT)
                                                      select new
                                                      {
                                                          mlunit.TEMPLATE_UOM,
                                                          mlunit.AS400_UOM
                                                      }).ToList();
                                    if (UnitMlData.Count == 0)
                                    {
                                        MUOM objmlunit = new MUOM();
                                        {
                                            objmlunit.TEMPLATE_UOM = objdtl.UNIT;
                                            objmlunit.AS400_UOM = item.unit.Substring(0, 3);
                                            objmlunit.CREATED_BY = 2;
                                            objmlunit.CREATED_DATE = DateTime.Now;
                                        };
                                        _datacontext.MUOMTable.Add(objmlunit);
                                        _datacontext.SaveChanges();
                                    }
                                }
                                else
                                {
                                    if (objdtl.UNIT != item.unit)
                                    {
                                        MUOM unitobj = (from uom in _datacontext.MUOMTable where uom.AS400_UOM.Trim().ToUpper() == objdtl.UNIT.Trim().ToUpper() select uom).FirstOrDefault();
                                        unitobj.AS400_UOM = item.unit.Substring(0, 3);
                                        unitobj.CREATED_BY = 5;
                                        _datacontext.SaveChanges();
                                    }
                                }
                                objdtl.PART_CODE = item.partCode;
                                objdtl.PART_NAME = item.partName;
                                objdtl.QUANTITY = item.quantity;
                                objdtl.UNIT = item.unit;
                                objdtl.PRICE = item.price;
                                objdtl.COST = item.cost;
                                objdtl.UPDATED_DATE = DateTime.Now;
                                objdtl.SUPPLIER = item.supplier;
                                objdtl.STATUS = Convert.ToInt32(item.status);
                                objdtl.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objdtl.ERROR_CODE = item.errorCode;
                                objdtl.ACCOUNT_NO = item.accountNo;
                                objdtl.ACCOUNT_DESCRIPTION = item.accountDescription;
                                objdtl.SEQ_NOTEXT = item.seqNoText;
                                _datacontext.SaveChanges();
                                dtlid[i] = Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID);
                            }
                            else
                            {
                                if (item.accountNo == otherAccountCode)
                                {
                                    count = 1;
                                }
                                TSNQ_ENQUIRY_DTL objenqdetails = new TSNQ_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_SNQENQUIRY_HDR_ID = objEnquiry.PK_SNQENQUIRY_HDR_ID;
                                    objenqdetails.PART_CODE = item.partCode;
                                    objenqdetails.PART_NAME = item.partName;
                                    objenqdetails.QUANTITY = item.quantity;
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
                                    objenqdetails.IS_UPDATED_WITH_ML = 0;
                                    objenqdetails.SEQ_NOTEXT = item.seqNoText;
                                };
                                _datacontext.TSNQ_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                            i = i + 1;
                        }
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
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        public MessageSNQ UpdateSaveAsEnquiry(Enquiryheader objEnquiry)
        {
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while Updating Enquiry";
            string otherAccountCode = this._Configuration.GetSection("DefaultAccountCode")["OtherAccountCode"];
            try
            {
                if (objEnquiry != null)
                {
                    TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == objEnquiry.PK_SNQENQUIRY_HDR_ID select hdr).FirstOrDefault();
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
                    headerobj.PORT = objEnquiry.port;
                    headerobj.AS400_MAPPING_PORT = objEnquiry.mappingPort;//GetPortMapping("", objEnquiry.port);
                    headerobj.DELIVERY_DATE = objEnquiry.deliveryDate;
                    // headerobj.OWNERSHIP = 0;
                    if (headerobj.STATUS == 1 && objEnquiry.saveAsDraft == "saveInVerification")  //not started
                    {
                        headerobj.STATUS = 1; //Verified
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
                    _datacontext.SaveChanges();
                    int count = 0;
                    if (objEnquiry.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (item.accountNo == otherAccountCode)
                            {
                                count = 1;
                            }
                            TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == item.PK_SNQENQUIRY_DTL_ID select dtl).FirstOrDefault();
                            if (objdtl != null)
                            {
                                if (objdtl.UNIT.Length > 3)
                                {
                                    var UnitMlData = (from mlunit in _datacontext.MUOMTable
                                                      where (mlunit.TEMPLATE_UOM == objdtl.UNIT)
                                                      select new
                                                      {
                                                          mlunit.TEMPLATE_UOM,
                                                          mlunit.AS400_UOM
                                                      }).ToList();
                                    if (UnitMlData.Count == 0)
                                    {
                                        MUOM objmlunit = new MUOM();
                                        {
                                            objmlunit.TEMPLATE_UOM = objdtl.UNIT;
                                            objmlunit.AS400_UOM = item.unit.Substring(0, 3);
                                            objmlunit.CREATED_BY = 3;
                                            objmlunit.CREATED_DATE = DateTime.Now;
                                        };
                                        _datacontext.MUOMTable.Add(objmlunit);
                                        _datacontext.SaveChanges();
                                    }
                                }
                                else
                                {
                                    if (objdtl.UNIT != item.unit)
                                    {
                                        MUOM unitobj = (from uom in _datacontext.MUOMTable where uom.AS400_UOM.Trim().ToUpper() == objdtl.UNIT.Trim().ToUpper() select uom).FirstOrDefault();
                                        unitobj.AS400_UOM = item.unit.Substring(0, 3);
                                        unitobj.CREATED_BY = 4;
                                        _datacontext.SaveChanges();
                                    }
                                }
                                objdtl.PART_CODE = item.partCode;
                                objdtl.PART_NAME = item.partName;
                                objdtl.QUANTITY = item.quantity;
                                objdtl.UNIT = item.unit;
                                objdtl.PRICE = item.price;
                                objdtl.COST = item.cost;
                                objdtl.UPDATED_DATE = DateTime.Now;
                                objdtl.SUPPLIER = item.supplier;
                                objdtl.STATUS = Convert.ToInt32(item.status);
                                objdtl.SEQ_NO = Convert.ToInt32(item.seqNo);
                                objdtl.ERROR_CODE = item.errorCode;
                                objdtl.ACCOUNT_NO = item.accountNo;
                                objdtl.ACCOUNT_DESCRIPTION = item.accountDescription;
                                objdtl.SEQ_NOTEXT = item.seqNoText;
                                _datacontext.SaveChanges();
                                dtlid[i] = Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID);
                            }
                            else
                            {
                                if (item.accountNo == otherAccountCode)
                                {
                                    count = 1;
                                }
                                TSNQ_ENQUIRY_DTL objenqdetails = new TSNQ_ENQUIRY_DTL();
                                {
                                    objenqdetails.FK_SNQENQUIRY_HDR_ID = objEnquiry.PK_SNQENQUIRY_HDR_ID;
                                    objenqdetails.PART_CODE = item.partCode;
                                    objenqdetails.PART_NAME = item.partName;
                                    objenqdetails.QUANTITY = item.quantity;
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
                                    objenqdetails.ERROR_CODE = item.errorCode;
                                    objenqdetails.ACCOUNT_NO = item.accountNo;
                                    objenqdetails.ACCOUNT_DESCRIPTION = item.accountDescription;
                                    objenqdetails.IS_UPDATED_WITH_ML = 0;
                                    objenqdetails.SEQ_NOTEXT = item.seqNoText;
                                };
                                _datacontext.TSNQ_ENQUIRY_DTLTable.Add(objenqdetails);
                                _datacontext.SaveChanges();
                            }
                            i = i + 1;
                        }
                    }
                    if (count > 0)
                    {
                        var Dtldata = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                       where dtl.FK_SNQENQUIRY_HDR_ID == headerobj.PK_SNQENQUIRY_HDR_ID
                                       select new
                                       {
                                           dtl.PK_SNQENQUIRY_DTL_ID,
                                           dtl.FK_SNQENQUIRY_HDR_ID,
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
                                           dtl.SEQ_NOTEXT
                                       }).ToList();
                        if (Dtldata.Count > 0)
                        {
                            foreach (var item in Dtldata)
                            {
                                TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == item.PK_SNQENQUIRY_DTL_ID select dtl).FirstOrDefault();
                                if (objdtl != null)
                                {
                                    objdtl.ACCOUNT_NO = otherAccountCode;
                                    _datacontext.SaveChanges();
                                }
                            }
                        }
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
                }
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //Delete dtl item From portal
        public MessageSNQ DeleteItems(List<Enquirydetailsdata> objEnquirydtls)
        {
            List<Enquirydetailsdata> EnqDetails = new List<Enquirydetailsdata>();
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while Deleting Enquiry";
            try
            {
                foreach (var item in objEnquirydtls)
                {
                    TSNQ_ENQUIRY_DTL objSnqDtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                                  where dtl.PK_SNQENQUIRY_DTL_ID == item.PK_SNQENQUIRY_DTL_ID
                                                  select dtl).ToList().SingleOrDefault();
                    if (objSnqDtl != null)
                    {
                        _datacontext.TSNQ_ENQUIRY_DTLTable.Remove(objSnqDtl);
                        _datacontext.SaveChanges();
                    }
                }
                obj.result = "Data deleted successfully";
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //From bot
        public MessageSNQ UpdateEnqStatus(Enquiryheader objEnquiry)
        {
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while Updating Enquiry from AS400";
            string MLLogicSNQ = this._Configuration.GetSection("MLLogicSNQ")["MLAllow"];
            try
            {
                TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == objEnquiry.PK_SNQENQUIRY_HDR_ID select hdr).FirstOrDefault();
                if (Convert.ToInt32(objEnquiry.status) == 5)
                {
                    headerobj.STATUS = 5; //Error
                    headerobj.ERROR_CODE = objEnquiry.errorCode;
                    headerobj.QUOTATION_NO = objEnquiry.quotationNo;
                    headerobj.IN_ERROR_AT = DateTime.Now.ToString();
                    if (objEnquiry.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (Convert.ToInt32(item.status) == 5)// if (Convert.ToInt32(item.status) != 4)
                            {
                                TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID) select dtl).FirstOrDefault();
                                if (objdtl != null)
                                {
                                    objdtl.STATUS = 5;//Convert.ToInt32(item.status);
                                    objdtl.ERROR_CODE = item.errorCode;
                                    objdtl.UPDATED_DATE = Convert.ToDateTime(item.UPDATED_DATE);
                                    _datacontext.SaveChanges();
                                    dtlid[i] = Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID);
                                }
                                i = i + 1;
                            }
                        }
                    }
                    _datacontext.SaveChanges();
                    string ownerEmailId = GetCustomerEmailMapping(headerobj.OWNER);
                    SendErrorRFQNotification(headerobj.ENQREF_NO, headerobj.OWNER, headerobj.OWNER_EMAILID, headerobj.SHIP_NAME
                        , headerobj.QUOTATION_NO);
                }
                if (Convert.ToInt32(objEnquiry.status) == 4 || Convert.ToInt32(objEnquiry.status) == 5)
                {
                    if (Convert.ToInt32(objEnquiry.status) != 5)
                    {
                        headerobj.STATUS = 4; //Success
                        headerobj.QUOTATION_CREATED_AT = objEnquiry.quotationCreatedat;
                        headerobj.QUOTATION_NO = objEnquiry.quotationNo;
                    }
                    if (objEnquiry.itemDetails != null)
                    {
                        long[] dtlid = new long[objEnquiry.itemDetails.Count];
                        int i = 0;
                        foreach (var item in objEnquiry.itemDetails)
                        {
                            if (Convert.ToInt32(item.status) == 4) //  if (Convert.ToInt32(item.status) != 5) //
                            {
                                TSNQ_ENQUIRY_DTL objdtl = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID) select dtl).FirstOrDefault();
                                if (objdtl != null)
                                {
                                    objdtl.STATUS = 4;// Convert.ToInt32(item.status);
                                    objdtl.ERROR_CODE = item.errorCode;
                                    objdtl.UPDATED_DATE = Convert.ToDateTime(item.UPDATED_DATE);
                                    _datacontext.SaveChanges();
                                    dtlid[i] = Convert.ToInt64(item.PK_SNQENQUIRY_DTL_ID);
                                }
                            }
                            if (MLLogicSNQ == "YES")
                            {
                                if (!string.IsNullOrEmpty(item.partCode))
                                {
                                    if (!string.IsNullOrEmpty(item.partCode))
                                    {
                                        if (item.partCode.Length <= 6)
                                        {
                                            var EnqMlData = (from mlitems in _datacontext.T_SNQ_ENQUIRY_ML_ITEMSTable
                                                             where (mlitems.PART_CODE == item.partCode && mlitems.PART_NAME.ToUpper() == item.partName.ToUpper() && mlitems.IS_ACTIVE == 1)
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
                                }
                            }
                            i = i + 1;
                        }
                    }
                    _datacontext.SaveChanges();
                }
                obj.result = "Enquiry Updated.";
                return obj;
            }
            catch (Exception ex)
            {
                obj.result = ex.Message;
            }
            return obj;
        }
        //For bot
        public List<Enquiryheader1> GetDetailsForAS400()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            List<Enquiryheader1> lstEnqHdrdtls = new List<Enquiryheader1>();
            string as400Port = "";
            var HdrData = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                           where (hdr.STATUS == 2 || hdr.STATUS == 6)
                           select hdr).OrderBy(c => c.STATUS).OrderByDescending(c => c.AS400_MAPPING_PORT).ToList();
            //Get all Verified and updated enquiries for  Yokohama ,kobe port .and take 1st five enquiry of same port
            if (HdrData.Count > 0)
            {
                foreach (var itemd in HdrData)
                {
                    as400Port = itemd.AS400_MAPPING_PORT;
                    break;
                }
            }
            int NoofEnq = Convert.ToInt32(this._Configuration.GetSection("GetEntriesForAs400")["Take"]);
            //   var objhdrData = HdrData;
            HdrData = HdrData.Take(NoofEnq).Where(x => x.AS400_MAPPING_PORT == as400Port).ToList();
            if (HdrData.Count > 0)
            {
                foreach (var item in HdrData)
                {
                    Enquiryheader1 objClsEnquiryhdr = new Enquiryheader1
                    {
                        PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID,
                        enquiryDate = item.ENQUIRY_DATE,
                        owner = DefaultCustomerMapping(item.OWNER),
                        ownerEmailid = item.OWNER_EMAILID,
                        enqrefNo = item.ENQREF_NO,
                        shipName = item.SHIP_NAME,
                        maker = item.MAKER,
                        type = item.TYPE,
                        equipment = item.EQUIPMENT,
                        serialNo = item.SERIAL_NO,
                        status = Convert.ToString(item.STATUS),
                        quotationNo = item.QUOTATION_NO,
                        port = item.PORT,
                        emailSubject = item.MAIL_SUBJECT,
                        rfqUrl = item.RFQ_URL,
                        mappingPort = item.AS400_MAPPING_PORT,
                        deliveryDate = (item.DELIVERY_DATE == "" || item.DELIVERY_DATE == null) ? "" : ParseStringToDate.ParseStringtoDate(item.DELIVERY_DATE).ToString("dd/MM/yyyy"), //DateConversion(item.DELIVERY_DATE),
                        totalNoOfItems = GetDetailCountforOther(item.PK_SNQENQUIRY_HDR_ID),
                    };
                    List<EnquiryHeaderDocDetails> lstHdrdoc = new List<EnquiryHeaderDocDetails>();
                    var docdata = (from doc in _datacontext.T_SNQ_DOCUMENT_HDRTable
                                   where doc.ENQUIRY_HDR_ID == item.PK_SNQENQUIRY_HDR_ID
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
                    var Dtldata = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable
                                   where dtl.FK_SNQENQUIRY_HDR_ID == objClsEnquiryhdr.PK_SNQENQUIRY_HDR_ID
                                   select new
                                   {
                                       dtl.PK_SNQENQUIRY_DTL_ID,
                                       dtl.FK_SNQENQUIRY_HDR_ID,
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
                                       dtl.SEQ_NOTEXT
                                   });
                    Dtldata = Dtldata.Where(x => x.STATUS == 1 || x.STATUS == 2 || x.STATUS == 3 || x.STATUS == 5 || x.STATUS == 6);
                    var objdtlData = Dtldata.ToList();
                    List<EnquiryDetails> lstClsEnquiryDtl = new List<EnquiryDetails>();
                    List<AccountData> lstAccountData = new List<AccountData>();
                    List<EnquiryDetailDocDetails> lstdtldoc = new List<EnquiryDetailDocDetails>();
                    if (objdtlData.Count > 0)
                    {
                        foreach (var item1 in objdtlData)
                        {
                            EnquiryDetails objClsEnquirydtl = new EnquiryDetails
                            {
                                PK_SNQENQUIRY_DTL_ID = item1.PK_SNQENQUIRY_DTL_ID,
                                fkEnquiryid = item1.FK_SNQENQUIRY_HDR_ID.ToString(),
                                partCode = item1.PART_CODE,
                                partName = item1.PART_NAME,
                                quantity = item1.QUANTITY,
                                unit = item1.UNIT,
                                price = item1.PRICE,
                                cost = item1.COST,
                                supplier = item1.SUPPLIER,
                                UPDATED_DATE = item1.UPDATED_DATE.ToString(),
                                accountDescription = item1.ACCOUNT_DESCRIPTION,
                                seqNo = Convert.ToString(item1.SEQ_NO),
                                accountNo = item1.ACCOUNT_NO,
                                seqNoText = item1.SEQ_NOTEXT,
                                status = Convert.ToString(item1.STATUS)
                            };
                            lstClsEnquiryDtl.Add(objClsEnquirydtl);
                        }
                        //newly added
                        var accountcodelist = lstClsEnquiryDtl.Select(x => x.accountNo).Distinct();
                        foreach (string acno in accountcodelist)
                        {
                            AccountData obj = new AccountData();
                            obj.ACCOUNTCODE = acno;
                            obj.itemDetails = lstClsEnquiryDtl.Where(x => x.accountNo == acno).ToList();
                            foreach (var ITEM in obj.itemDetails)
                            {
                                var docdtldata = (from doc in _datacontext.T_SNQ_DOCUMENT_DTLTable
                                                  where doc.ENQUIRY_DTL_ID == ITEM.PK_SNQENQUIRY_DTL_ID
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
                                    ITEM.docdtlDetails = lstdtldoc;
                                }
                            }
                            lstAccountData.Add(obj);
                        }
                        objClsEnquiryhdr.itemDetails = lstAccountData;
                        //end
                    }
                    lstEnqHdrdtls.Add(objClsEnquiryhdr);
                }
            }
            return lstEnqHdrdtls;
        }
        //Get Not Started Enquiry List
        public List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (objsearchdata.FromDate != "")// && objsearchdata.ToDate != "" && objsearchdata.sourceType != ""
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)notstartedStatus
                                // 1)
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
                                    enquirydt = enqheader.ENQUIRY_DATE == null ? "" : enqheader.ENQUIRY_DATE,
                                    enqheader.ENQREF_NO,
                                    enqheader.SHIP_NAME,
                                    enqheader.OWNER,
                                    statusmst.STATUS_DESCRIPTION,
                                    enqheader.CREATED_DATE,
                                    enqheader.OWNERSHIP,
                                    enqheader.SAVE_AS_DRAFT,
                                    enqheader.SOURCE_TYPE,
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
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
                    // DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.saveAsDraft = item.SAVE_AS_DRAFT;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
            else
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)notstartedStatus
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
                                    enqheader.ENQUIRY_DATE,
                                    enqheader.ENQREF_NO,
                                    enqheader.SHIP_NAME,
                                    enqheader.OWNER,
                                    statusmst.STATUS_DESCRIPTION,
                                    enqheader.CREATED_DATE,
                                    enqheader.OWNERSHIP,
                                    enqheader.SAVE_AS_DRAFT,
                                    enqheader.SOURCE_TYPE,
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.saveAsDraft = item.SAVE_AS_DRAFT;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
        }
        //Get Error Enquiry List
        public List<Enquiryheaderdata> GetErrorTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (objsearchdata.FromDate != "")// && objsearchdata.ToDate != "")
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)errorStatus
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                    enqheader.SAVE_AS_DRAFT,
                                    enqheader.SOURCE_TYPE,
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    userm.LOGIN_NAME,
                                    userm.USER_NAME
                                });
                if (objsearchdata.CustNameShipNameRefNo != "")
                {
                    Tasklist = Tasklist.Where(x => x.OWNER.Contains(objsearchdata.CustNameShipNameRefNo) || x.ENQREF_NO == objsearchdata.CustNameShipNameRefNo || x.SHIP_NAME == objsearchdata.CustNameShipNameRefNo || x.QUOTATION_NO == objsearchdata.CustNameShipNameRefNo);
                }
                if (objsearchdata.FromDate != "" && objsearchdata.ToDate == "")
                {
                    DateTime fromDt = Convert.ToDateTime(objsearchdata.FromDate);
                    // DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
                    Tasklist = Tasklist.Where(x => x.CREATED_DATE.Date == fromDt.Date);
                }
                if (objsearchdata.sourceType != "")
                {
                    Tasklist = Tasklist.Where(x => x.SOURCE_TYPE.ToUpper() == objsearchdata.sourceType.ToUpper());
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.saveAsDraft = item.SAVE_AS_DRAFT;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
            else
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)errorStatus
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.ownerEmailid = item.OWNER_EMAILID;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.saveAsDraft = item.SAVE_AS_DRAFT;
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.docPath = item.DOC_PATH;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
        }
        //Get Completed Enquiry List
        public List<Enquiryheaderdata> GetCompletedTaskList(searchdata objsearchdata)
        {
            List<Enquiryheaderdata> lstEnqheader = new List<Enquiryheaderdata>();
            if (objsearchdata.FromDate != "")// && objsearchdata.ToDate != "")
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)successStatus
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
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
                    // DateTime toDt = Convert.ToDateTime(objsearchdata.ToDate);
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.duration = ParseStringToDate.DateDifference(item.CREATED_DATE, Convert.ToDateTime(item.QUOTATION_CREATED_AT));
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
            else
            {
                var Tasklist = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                from userm in usmt.DefaultIfEmpty()
                                where enqheader.STATUS == (int)successStatus
                                select new
                                {
                                    enqheader.PK_SNQENQUIRY_HDR_ID,
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
                                    DETAIL_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
                                    DETAIL_ERROR_COUNT = _datacontext.TSNQ_ENQUIRY_DTLTable.Where(x => x.STATUS == 5).Count(t => t.FK_SNQENQUIRY_HDR_ID == enqheader.PK_SNQENQUIRY_HDR_ID),
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
                    objEnqheadr.PK_SNQENQUIRY_HDR_ID = item.PK_SNQENQUIRY_HDR_ID;
                    objEnqheadr.enquiryDate = dt[0];//data.ENQUIRY_DATE;
                    objEnqheadr.enqrefNo = item.ENQREF_NO;
                    objEnqheadr.shipName = item.SHIP_NAME;
                    objEnqheadr.owner = item.OWNER;
                    objEnqheadr.ownerEmailid = item.OWNER_EMAILID;
                    objEnqheadr.status = item.STATUS_DESCRIPTION;
                    objEnqheadr.duration = ParseStringToDate.DateDifference(item.CREATED_DATE, Convert.ToDateTime(item.QUOTATION_CREATED_AT));
                    objEnqheadr.quotationNo = item.QUOTATION_NO;
                    objEnqheadr.docPath = item.DOC_PATH;
                    objEnqheadr.TotalNoOfErrorItems = item.DETAIL_ERROR_COUNT;
                    objEnqheadr.TotalNoOfItems = item.DETAIL_COUNT;
                    objEnqheadr.sourceType = item.SOURCE_TYPE;
                    objEnqheadr.createdDate = item.CREATED_DATE.ToString();
                    if (item.OWNERSHIP != 0)
                    {
                        objEnqheadr.action = "YES";
                        // objEnqheadr.pkuserId = data.PK_USER_ID;
                        objEnqheadr.loginId = item.LOGIN_NAME;
                        objEnqheadr.userName = item.USER_NAME;
                    }
                    else
                    {
                        objEnqheadr.action = "";
                    }
                    lstEnqheader.Add(objEnqheadr);
                }
                return lstEnqheader.OrderByDescending(x => x.PK_SNQENQUIRY_HDR_ID).ToList();
            }
        }
        public MessageSNQ Updateownership(EnqOwnership objOwnershipdtls)
        {
            Enquirydetailsdata EnqDetails = new Enquirydetailsdata();
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while taking Ownership of Enquiry";
            try
            {
                TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.PK_SNQENQUIRY_HDR_ID == objOwnershipdtls.PK_SNQENQUIRY_HDR_ID select hdr).FirstOrDefault();
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
        }
        public MessageSNQ Releaseownership(EnqOwnership objEnquirydtls)
        {
            Enquirydetailsdata EnqDetails = new Enquirydetailsdata();
            MessageSNQ obj = new MessageSNQ();
            obj.result = "Error while releasing Ownership of Enquiry";
            try
            {
                var EnqHeaderdata = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                     where enqheader.OWNERSHIP == objEnquirydtls.Ownership
                                     select new
                                     {
                                         enqheader.PK_SNQENQUIRY_HDR_ID,
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
                        TSNQ_ENQUIRY_HDR objhdr = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable where hdr.OWNERSHIP == data.OWNERSHIP select hdr).FirstOrDefault();
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
        }
        //For portal 
        public EnqOwnership Getuserownershipdtl(int PKHDRID)
        {
            EnqOwnership lstEnqheader = new EnqOwnership();
            var action = "";
            var EnqHeaderdata = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                 join usrmst in _datacontext.MstUserTable on enqheader.OWNERSHIP equals usrmst.PK_USER_ID into usmt
                                 from userm in usmt.DefaultIfEmpty()
                                 where enqheader.PK_SNQENQUIRY_HDR_ID == PKHDRID
                                 select new
                                 {
                                     enqheader.PK_SNQENQUIRY_HDR_ID,
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
                    PK_SNQENQUIRY_HDR_ID = EnqHeaderdata.PK_SNQENQUIRY_HDR_ID,
                    Ownership = EnqHeaderdata.OWNERSHIP,
                    action = action,
                    loginId = EnqHeaderdata.LOGIN_NAME,
                    userName = EnqHeaderdata.USER_NAME
                };
            }
            return lstEnqheader;
        }
        public List<PortMapping> GetPortMappingData()
        {
            List<PortMapping> lstPortMapping = new List<PortMapping>();
            try
            {
                var portdata = (from p in _datacontext.MM_PORT_MAPPINGTable
                                select new
                                {
                                    p.PK_PORTMAPP_ID,
                                    p.AS400_PORT,
                                }).ToList();
                var result1 = portdata.GroupBy(i => i.AS400_PORT).Select(group => group.First());
                if (result1 != null)
                {
                    foreach (var data in result1)
                    {
                        PortMapping objdata = new PortMapping();
                        objdata.mappingPortId = data.PK_PORTMAPP_ID;
                        objdata.mappingPort = data.AS400_PORT;
                        lstPortMapping.Add(objdata);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return lstPortMapping.OrderBy(x => x.mappingPort).ToList();
        }
        public string GetPortMapping(string shipName, string port)
        {
            #region port mapping
            //port mapping
            string mappedport = "";
            string snqPort = this._Configuration.GetSection("DefaultSNQPort")["SNQPort"];
            string portEnergyy = this._Configuration.GetSection("PortName")["PortENERGY"];
            string portYokohama = this._Configuration.GetSection("PortName")["portYokohama"];
            if (shipName.ToUpper().Contains(portEnergyy))
            {
                mappedport = portYokohama;
            }
            else
            {
                if (!string.IsNullOrEmpty(port))
                {
                    var findport = (from hdr in _datacontext.MM_PORT_MAPPINGTable
                                    where hdr.TEMPLATE_PORTNAME.ToUpper() == port.ToUpper()
                                    select hdr.AS400_PORT).ToList();
                    if (findport.Count > 0)
                    {
                        mappedport = findport[0];
                    }
                    else
                    {
                        mappedport = snqPort;
                    }
                }
                else
                {
                    mappedport = snqPort;
                }
            }
            return mappedport;
            //end port mapping
            #endregion port mapping
        }
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
            return mappedCustomer;
            //end customer mapping
            #endregion customer mapping
        }
        public string DefaultCustomerMapping(string owner)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomer = "";
            string defaultCust = this._Configuration.GetSection("SNQDefaultCust")["defaultCust"];
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomer = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                    join dtl in _datacontext.MCUSTOMERTable on hdr.FK_CUST_ID equals dtl.PK_CUSTOMER_ID
                                    where hdr.AS400_CUSTOMER_NAME.ToUpper().Trim() == owner.ToUpper().Trim() && dtl.DEPT_NAME.Trim().ToUpper() != "MSD"
                                    select hdr.AS400_CUSTOMER_NAME).ToList();
                if (findCustomer.Count > 0)
                {
                    mappedCustomer = findCustomer[0];
                }
                else
                {
                    mappedCustomer = defaultCust;
                }
            }
            else
            {
                mappedCustomer = owner;
            }
            return mappedCustomer;
            //end customer mapping
            #endregion customer mapping
        }
        public string GetCustomerEmailMapping(string owner)
        {
            #region customer mapping
            //customer mapping
            string mappedCustomerEmailID = "";
            if (!string.IsNullOrEmpty(owner))
            {
                var findCustomerEmailId = (from hdr in _datacontext.MCUSTOMER_MAPPINGTable
                                           join dtl in _datacontext.MCUSTOMERTable on hdr.FK_CUST_ID equals dtl.PK_CUSTOMER_ID
                                           where hdr.AS400_CUSTOMER_NAME.ToUpper() == owner.ToUpper()
                                           select dtl.CUSTOMER_EMAILID).ToList();
                if (findCustomerEmailId.Count > 0)
                {
                    mappedCustomerEmailID = findCustomerEmailId[0];
                }
            }
            //end customer mapping
            #endregion customer mapping
            return mappedCustomerEmailID;
        }
        public string GetAccountCodeDesMapping(string partCode, string MAIL_SUBJECT, string accountDescription)
        {
            //AccountCode mapping
            #region AccountCode
            string AccountCode = "";
            string ReadAccountcode = this._Configuration.GetSection("DefaultAccountCode")["AccountCode"];
            string defaultAccountCode = "";
            int count = 0;
            if (partCode.StartsWith("00"))
            {
                AccountCode = defaultAccountCode;
                count = 1;
            }
            if (!string.IsNullOrEmpty(MAIL_SUBJECT))
            {
                //Deck
                if (MAIL_SUBJECT.Contains("DECK"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "DECK"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //STATIONERY
                if (MAIL_SUBJECT.Contains("STATIONERY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "STATIONERY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ROPE
                if (MAIL_SUBJECT.Contains("ROPE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ROPE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //SAFETY
                if (MAIL_SUBJECT.Contains("SAFETY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "SAFETY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //MEDICAL
                if (MAIL_SUBJECT.Contains("MEDICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "MEDICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ENGINE
                if (MAIL_SUBJECT.Contains("ENGINE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ENGINE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ELECTRICAL
                if (MAIL_SUBJECT.Contains("ELECTRICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ELECTRICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //CABIN
                if (MAIL_SUBJECT.Contains("CABIN"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "CABIN"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PROVISION
                if (MAIL_SUBJECT.Contains("PROVISION"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PROVISION"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //BOND
                if (MAIL_SUBJECT.Contains("BOND"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "BOND"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PRIVATE
                if (MAIL_SUBJECT.Contains("PRIVATE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PRIVATE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
            }
            if (!string.IsNullOrEmpty(accountDescription))//account code ml from acc_description
            {
                //Deck
                if (accountDescription.Contains("DECK"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "DECK"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //STATIONERY
                if (accountDescription.Contains("STATIONERY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "STATIONERY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ROPE
                if (accountDescription.Contains("ROPE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ROPE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //SAFETY
                if (accountDescription.Contains("SAFETY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "SAFETY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //MEDICAL
                if (accountDescription.Contains("MEDICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "MEDICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ENGINE
                if (accountDescription.Contains("ENGINE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ENGINE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ELECTRICAL
                if (accountDescription.Contains("ELECTRICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ELECTRICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //CABIN
                if (accountDescription.Contains("CABIN"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "CABIN"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PROVISION
                if (accountDescription.Contains("PROVISION"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PROVISION"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //BOND
                if (accountDescription.Contains("BOND"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "BOND"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PRIVATE
                if (accountDescription.Contains("PRIVATE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PRIVATE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
            }
            return AccountCode;
            #endregion AccountCode
            //end AccountCode mapping
        }
        //done by pooja
        public string GetAccountCodeMapping(string accountCode, string accountDescription, string ownerName)
        {
            //AccountCode mapping
            #region AccountCode
            string AccountCode = "";
            string ReadAccountcode = this._Configuration.GetSection("DefaultAccountCode")["AccountCode"];
            string OtherAccountCode = this._Configuration.GetSection("DefaultAccountCode")["OtherAccountCode"];
            string ownerNm = this._Configuration.GetSection("DefaultAccountCode")["ownerNm"];
            if (string.IsNullOrEmpty(accountCode) && ownerName == ownerNm)
            {
                AccountCode = OtherAccountCode;
            }
            else if (!string.IsNullOrEmpty(accountCode))
            {
                AccountCode = accountCode;
            }
            else
            {
                AccountCode = ReadAccountcode;
            }
            if (!string.IsNullOrEmpty(accountDescription))//account code ml from acc_description
            {
                //Deck
                if (accountDescription.Contains("DECK"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "DECK"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //STATIONERY
                if (accountDescription.Contains("STATIONERY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "STATIONERY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ROPE
                if (accountDescription.Contains("ROPE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ROPE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //SAFETY
                if (accountDescription.Contains("SAFETY"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "SAFETY"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //MEDICAL
                if (accountDescription.Contains("MEDICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "MEDICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ENGINE
                if (accountDescription.Contains("ENGINE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ENGINE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //ELECTRICAL
                if (accountDescription.Contains("ELECTRICAL"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "ELECTRICAL"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //CABIN
                if (accountDescription.Contains("CABIN"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "CABIN"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PROVISION
                if (accountDescription.Contains("PROVISION"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PROVISION"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //BOND
                if (accountDescription.Contains("BOND"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "BOND"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
                //PRIVATE
                if (accountDescription.Contains("PRIVATE"))
                {
                    var ACCOUNTCOD = (from a in _datacontext.MCUST_DEPT_ACC_MAPPINGTable
                                      where a.ACC_DESCRIPTION == "PRIVATE"
                                      select a.ACCOUNT_CODE).ToList();
                    if (ACCOUNTCOD.Count > 0)
                    {
                        AccountCode = ACCOUNTCOD[0];
                    }
                    else
                    {
                        AccountCode = ReadAccountcode;
                    }
                }
            }
            //if (AccountCode == "")
            //{
            //    AccountCode = ReadAccountcode;
            //}
            return AccountCode;
            #endregion AccountCode
            //end AccountCode mapping
        }
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
        //date conversion function for delivery date
        public string DateConversion(string DELIVERY_DATE)
        {
            string date = "";
            if (DELIVERY_DATE == null || DELIVERY_DATE == "")
            {
                DELIVERY_DATE = "";
            }
            else
            {
                string[] dt = DELIVERY_DATE.Split("/");
                if (dt[1].ToUpper().Trim() == "JAN")
                {
                    dt[1] = "01";
                }
                if (dt[1].ToUpper().Trim() == "FEB")
                {
                    dt[1] = "02";
                }
                if (dt[1].ToUpper().Trim() == "MAR")
                {
                    dt[1] = "03";
                }
                if (dt[1].ToUpper().Trim() == "APR")
                {
                    dt[1] = "04";
                }
                if (dt[1].ToUpper().Trim() == "MAY")
                {
                    dt[1] = "05";
                }
                if (dt[1].ToUpper().Trim() == "JUN")
                {
                    dt[1] = "06";
                }
                if (dt[1].ToUpper().Trim() == "JUL")
                {
                    dt[1] = "07";
                }
                if (dt[1].ToUpper().Trim() == "AUG")
                {
                    dt[1] = "08";
                }
                if (dt[1].ToUpper().Trim() == "SEP")
                {
                    dt[1] = "09";
                }
                if (dt[1].ToUpper().Trim() == "OCT")
                {
                    dt[1] = "10";
                }
                if (dt[1].ToUpper().Trim() == "NOV")
                {
                    dt[1] = "11";
                }
                if (dt[1].ToUpper().Trim() == "DEC")
                {
                    dt[1] = "12";
                }
                if (Convert.ToInt32(dt[0]) > 12)
                {
                    date = DateTime.Parse(dt[1] + "/" + dt[0] + "/" + dt[2]).ToString("dd/MM/yyyy");
                }
                else
                {
                    date = DateTime.Parse(DELIVERY_DATE).ToString("dd/MM/yyyy");
                }
            }
            return date;
        }
        public void SendErrorRFQNotification(string rfqNo, string customerName, string customeremailId, string shipName, string quotationNo)
        {
            try
            {
                string enviornment = this._Configuration.GetSection("SNQRFQNotification")["Name"];
                string portalenviornment = this._Configuration.GetSection("PortalURL")["Name"];
                string produrl = this._Configuration.GetSection("PortalURL")["PRODPortalURL"];
                string uaturl = this._Configuration.GetSection("PortalURL")["UATPortalURL"];
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(this._Configuration.GetSection("SNQRFQNotification")["FromMail"]);
                if (enviornment.ToUpper() == "PROD")
                {
                    if (!String.IsNullOrEmpty(customeremailId))
                    {
                        mail.To.Add(customeremailId);
                    }
                    else
                    {
                        mail.To.Add(this._Configuration.GetSection("SNQRFQNotification")["ToMail"]);
                    }
                }
                else
                {
                    mail.To.Add(this._Configuration.GetSection("SNQRFQNotification")["ToMail"]);
                }
                mail.CC.Add(this._Configuration.GetSection("SNQRFQNotification")["CCMail"]);
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
                    mail.Subject = enviornment + " " + "SNQ Enquiry - RFQ No : " + rfqNo + " In ErrorWork Basket ";
                    mail.Body = "<p>Hello Team,<br /><br /> " +
                               "<table style= 'border:1px solid black; border-collapse:collapse'>" +
                               "<tbody><tr><td style= 'border:1px solid black;'> &nbsp;&nbsp;&nbsp; RFQ No : &nbsp;&nbsp;&nbsp; </td>" + "<td style= 'border:1px solid black;'> &nbsp;&nbsp;&nbsp; " + rfqNo + " &nbsp;&nbsp;&nbsp;</td></tr>" +
                               "<tr><td style= 'border:1px solid black;'> &nbsp;&nbsp;&nbsp; Ship Name : &nbsp;&nbsp;&nbsp; </td><td style= 'border:1px solid black;'> &nbsp;&nbsp;&nbsp; " + shipName + "&nbsp;&nbsp;&nbsp; </ td ></ tr >" +
                               "<tr><td style= 'border:1px solid black;'> &nbsp;&nbsp;&nbsp; Quotation No : &nbsp;&nbsp;&nbsp; </td><td style= 'border:1px solid black;' >  &nbsp;&nbsp;&nbsp; " + quotationNo + "&nbsp;&nbsp;&nbsp; </td></tr>" +
                               "</tbody></table> <br/><br/> " +
                                "was processed by RPA Creation BOT and due to some error, it went to the error work basket. Flologic Team will check and resolve the error and will also let you know if there's anything to be done by the FTC Team.<br/></p> " +
                                "<p> Portal URL : <a href=" + url + "> AutomationPortal </a><br/><br/>" +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                else
                {
                    mail.Subject = enviornment + " " + "SNQ Enquiry - Customer Name : " + customerName + " In ErrorWork Basket ";
                    mail.Body = "<p>Hello Team,<br/><br/>Customer Name : " + customerName + " was processed by RPA Creation BOT and due to some error, it went to the error work basket. Flologic Team will check and resolve the error and will also let you know if there's anything to be done by the FTC Team.<br/></p> " +
                        "<p> Portal URL : <a href=" + url + "> AutomationPortal </a><br/><br/>" +
                    "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("SNQRFQNotification")["Username"], this._Configuration.GetSection("SNQRFQNotification")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
            }
        }
        #region verify error RFQs (only AS400 lag issue)
        public MessageSNQ VerifyRFQ(string errorRFQ)
        {
            MessageSNQ objmessage = new MessageSNQ();
            string verfiedBy = this._Configuration.GetSection("ErrorSNQRFQVerified")["VerifiedBy"];
            string verfiedAllow = this._Configuration.GetSection("ErrorSNQRFQVerified")["Allow"];
            string ErrorVerifiedCount = this._Configuration.GetSection("ErrorSNQRFQVerified")["ErrorVerifiedCount"];
            objmessage.result = "Data not available";
            try
            {
                #region
                if (errorRFQ.ToUpper() == "YES")
                {
                    if (verfiedAllow.ToUpper() == "YES")
                    {
                        #region Get msd header data
                        var snqhdrData = (from enqheader in _datacontext.TSNQ_ENQUIRY_HDRTable
                                          join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                          where (enqheader.STATUS == (int)errorStatus) && (enqheader.ERROR_CODE == "" || enqheader.ERROR_CODE == null)
                                          select new
                                          {
                                              enqheader.PK_SNQENQUIRY_HDR_ID,
                                              enqheader.ERROR_VERIFIED_COUNTER,
                                              enqheader.ENQREF_NO
                                          }).ToList();
                        #endregion
                        #region update header part
                        foreach (var item in snqhdrData)
                        {
                            TSNQ_ENQUIRY_HDR headerobj = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                                          where hdr.PK_SNQENQUIRY_HDR_ID == item.PK_SNQENQUIRY_HDR_ID
                                                          select hdr).FirstOrDefault();
                            if (item.ERROR_VERIFIED_COUNTER < 2)
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
                                var snqDtlData = (from enqheader in _datacontext.TSNQ_ENQUIRY_DTLTable
                                                  join statusmst in _datacontext.M_STATUS_CODE on enqheader.STATUS equals statusmst.STATUS_CODE
                                                  where (enqheader.STATUS == (int)errorStatus || enqheader.STATUS == (int)verifiedStatus) && enqheader.FK_SNQENQUIRY_HDR_ID == item.PK_SNQENQUIRY_HDR_ID
                                                   && (enqheader.ERROR_CODE == "" || enqheader.ERROR_CODE == null)
                                                  select new
                                                  {
                                                      enqheader.PK_SNQENQUIRY_DTL_ID,
                                                      enqheader.STATUS,
                                                  }).ToList();
                                #endregion
                                #region update dtl part
                                foreach (var item1 in snqDtlData)
                                {
                                    TSNQ_ENQUIRY_DTL dtlobj = (from dtl in _datacontext.TSNQ_ENQUIRY_DTLTable where dtl.PK_SNQENQUIRY_DTL_ID == item1.PK_SNQENQUIRY_DTL_ID select dtl).FirstOrDefault();
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
        public void SendErrorVerifedRFQNotification(string rfqNo)
        {
            try
            {
                dynamic dtlBody = "";
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                string enviornment = this._Configuration.GetSection("ErrorSNQRFQVerified")["Name"];
                mail.From = new MailAddress(this._Configuration.GetSection("ErrorSNQRFQVerified")["FromMail"]);
                mail.To.Add(this._Configuration.GetSection("ErrorSNQRFQVerified")["ToMail"]);
                mail.CC.Add(this._Configuration.GetSection("ErrorSNQRFQVerified")["CCMail"]);
                if (rfqNo != "")
                {
                    mail.Subject = enviornment + " " + "SNQ" + " " + "Enquiry - RFQ No : " + rfqNo + " In ErrorWork Basket - after two times verifying.";
                    mail.Body = "<p>Hello Team,<br /><br />RFQ No : " + rfqNo + " is processed by RPA BOT .Please correct with the appropriate data .<br/></p> " +
                        "<span style='color: red;'>This is a system generated email, do not reply to this email id.</span><br/><br/>" +
                        "Thanks & Regards,<br />" +
                        "RPA BOT</p>";
                }
                mail.IsBodyHtml = true;
                SmtpServer.Port = 587;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.Credentials = new System.Net.NetworkCredential(this._Configuration.GetSection("ErrorSNQRFQVerified")["Username"], this._Configuration.GetSection("ErrorSNQRFQVerified")["Password"]);
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}
