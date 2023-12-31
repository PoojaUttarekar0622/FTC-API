﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Model
{
   public  class MESPASClassDeclarations
    {
        public class Department
        {
            public int deptId { get; set; }
            public string deptName { get; set; }
        }
        public class Message
        {
            public string result { get; set; }
        }
        public class Enquiryheader
        {

            public long PK_MESPASENQUIRY_HDR_ID { get; set; }

            public long eventId { get; set; }
            public string enquiryDate { get; set; }
            public string deptName { get; set; }
          
            public string owner { get; set; }
            public string ownerEmailid { get; set; }
            public string shipName { get; set; }
            public string enqrefNo { get; set; }
            public string docPath { get; set; }
            public string status { get; set; }
            public string quotationNo { get; set; }
            public string errorCode { get; set; }
            public string maker { get; set; }
            public string type { get; set; }
            public string equipment { get; set; }
            public string serialNo { get; set; }
            public string discountAmount { get; set; }
            public string netAmount { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public long FK_INSTANCE_ID { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string raisedBy { get; set; }
            public string mailBody { get; set; }
            public string emailReceivedat { get; set; }
            public string emailProcessedat { get; set; }
            public string documentProcessedat { get; set; }
            public string inErrorat { get; set; }
            public string verifiedAt { get; set; }
            public string updatedAt { get; set; }
            public string quotationCreatedat { get; set; }

            public string port { get; set; }

            public string mappingPort { get; set; }
            public string supplierCode { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string leadTimeforitem { get; set; }
            public string rfqUrl { get; set; }
            public string emailFrom { get; set; }
            public string verifiedBy { get; set; }
            public string correctedBy { get; set; }
            public string rejectedBy { get; set; }
            public string makerInfo { get; set; }
            public string saveAsDraft { get; set; }

            public string sourceType { get; set; }

            public List<EnquiryHeaderDocDetails> docHdrDetails { get; set; }

            public List<EnquiryDetails> itemDetails { get; set; }

        }

        public class EnquiryDetails
        {

            public long PK_MESPASENQUIRY_DTL_ID { get; set; }
            public long fkEnquiryid { get; set; }
            public string partCode { get; set; }
            public string partName { get; set; }
            public string quantity { get; set; }
            public string unit { get; set; }
            public string price { get; set; }
            public string cost { get; set; }
            public string discountAmount { get; set; }
            public string UPDATED_DATE { get; set; }
            public string supplier { get; set; }
            public string status { get; set; }
            public string seqNo { get; set; }
            public string errorCode { get; set; }
            public string accountNo { get; set; }
            public string accountDescription { get; set; }
            public int IS_UPDATED_WITH_ML { get; set; }
            public string enqrefNo { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public int Qty { get; set; }
            public string seqNoText { get; set; }

            public bool isSelected { get; set; }
            public List<EnquiryDetailDocDetails> docdtlDetails { get; set; }
        }

        public class EnquiryHeaderDocDetails
        {

            public long docId { get; set; }

            public long enquiryHdrId { get; set; }
            public string docPath { get; set; }
            public string errorDescription { get; set; }


            public int createdBy { get; set; }
            public DateTime? createdAt { get; set; }

            public int isActive { get; set; }
        }

        public class EnquiryDetailDocDetails
        {

            public long docId { get; set; }

            public long enquiryDtlId { get; set; }
            public string docPath { get; set; }
            public string errorDescription { get; set; }


            public int createdBy { get; set; }
            public DateTime? createdAt { get; set; }

            public int isActive { get; set; }
        }


        public class Enquiryheaderdata
        {
            public long PK_MESPASENQUIRY_HDR_ID { get; set; }

            public string createdDate { get; set; }
            public string enquiryDate { get; set; }
            public string owner { get; set; }
            public string ownerEmailid { get; set; }
            public string shipName { get; set; }
            public string enqrefNo { get; set; }
            public string docPath { get; set; }
            public string duration { get; set; }
            public string status { get; set; }
            public string quotationNo { get; set; }
            public int TotalNoOfItems { get; set; }
            public int TotalNoOfErrorItems { get; set; }

            public string errorCode { get; set; }
            public string maker { get; set; }
            public string type { get; set; }
            public string port { get; set; }
            public string equipment { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string serialno { get; set; }
            public string leadTimeforitem { get; set; }
            public string emailReceiveddate { get; set; }
            public string emailFrom { get; set; }
            public string action { get; set; }
            public long pkuserId { get; set; }
            public string loginId { get; set; }
            public string userName { get; set; }

            public string saveAsDraft { get; set; }

            public string sourceType { get; set; }
           
        }

        public class searchdata
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string token { get; set; }
            public string CustNameShipNameRefNo { get; set; }

            public string sourceType { get; set; }

        }


        public class EnqOwnership
        {
            public long PK_MESPASENQUIRY_HDR_ID { get; set; }
            public int Ownership { get; set; }
            public string action { get; set; }
            public string loginId { get; set; }
            public string userName { get; set; }

        }
    }
}
