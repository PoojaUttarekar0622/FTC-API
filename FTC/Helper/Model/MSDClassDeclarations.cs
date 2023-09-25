using System;
using System.Collections.Generic;


namespace Helper.Model
{
    public class MSDClassDeclarations
    {
       
        public class Enquiryheader
        {

            public long PK_MSDENQUIRY_HDR_ID { get; set; }

            public long eventId { get; set; }
            public string enquiryDate { get; set; }
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
            public string supplierCode { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string leadTimeforitem { get; set; }
            public string rfqUrl { get; set; }
            public string emailFrom { get; set; }
            public string verifiedBy { get; set; }
            public string correctedBy { get; set; }

            public string makerInfo { get; set; }
            public string saveAsDraft { get; set; }

            public string sourceType { get; set; }

             public string IsAdditionalRound { get; set; }

             public int IsUpdatedMakerWithML { get; set; }
             public int IsUpdatedEquipmentWithML { get; set; }
 
            public  string emailSubject { get; set; }
            public string rfqPassword{ get; set; }

            public string hdrRemark { get; set; }

            public string authCode { get; set; }
            public List<EnquiryHeaderDocDetails> docHdrDetails { get; set; }

            public List<EnquiryDetails> itemDetails { get; set; }

        }

        public class EnquiryDetails
        {

            public long PK_MSDENQUIRY_DTL_ID { get; set; }
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

            public string  IsUpdatedMESPASItemsWithML { get; set; }
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
            public long PK_MSDENQUIRY_HDR_ID { get; set; }

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

           
            public List<Enquirydetailsdata> itemDetails { get; set; }
        }

        public class Enquirydetailsdata
        {
            public long PK_MSDENQUIRY_DTL_ID { get; set; }

         
            public long fkEnquiryid { get; set; }
            public string partCode { get; set; }
            public string partName { get; set; }
            public string quantity { get; set; }
            public string unit { get; set; }
            public string price { get; set; }
            public string cost { get; set; }
            public string status { get; set; }
            public string seqNo { get; set; }
            public string seqNoText { get; set; }
            public string errorCode { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string accountNo { get; set; }
            public string accountDescription { get; set; }
            public bool isSelected { get; set; }
        }

        public class EnquiryheaderdataForas400
        {
            public long PK_MSDENQUIRY_HDR_ID { get; set; }
            public string enquiryDate { get; set; }
            public string owner { get; set; }
            public string ownerEmailid { get; set; }

            public string IsAttachmentHasErrors { get; set; }
            public string enqrefNo { get; set; }
            public string shipName { get; set; }
            public string maker { get; set; }

            public string mfgCode { get; set; }
            public string type { get; set; }
            public string equipment { get; set; }
            public string serialNo { get; set; }
            public string quotationNo { get; set; }
            public string status { get; set; }
            public string supplierCode { get; set; }
            public string errorCode { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string leadTimeforitem { get; set; }
       
            public string quotationCreatedat { get; set; }
            public string makerInfo { get; set; }
            public string rfqUrl { get; set; }
            public string docPath { get; set; }

            public string emailSubject { get; set; }

            public string hdrRemark { get; set; }

            public string authCode { get; set; }

            public string as400UserId { get; set; }

            public int totalNoOfItems { get; set; }
            public List<EnquiryHeaderDocDetails> docHdrDetails { get; set; }

            public List<EnquirydetailsdataForas400> itemDetails { get; set; }

           }
        
        public class EnquirydetailsdataForas400
        {
            public long PK_MSDENQUIRY_DTL_ID { get; set; }
            public long fkEnquiryid { get; set; }
            public string partCode { get; set; }
            public string partName { get; set; }
            public int quantity { get; set; }
            public string unit { get; set; }
            public string price { get; set; }
            public string cost { get; set; }
            public string supplier { get; set; }
            public DateTime UPDATED_DATE { get; set; }
            public string accountNo { get; set; }
            public string accountDescription { get; set; }
            public string seqNo { get; set; }
            public int leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string status { get; set; }
            public string errorCode { get; set; }
            public string seqNoText { get; set; }

            public List<EnquiryDetailDocDetails> docdtlDetails { get; set; }

        }

        public class EnquiryheaderForMSDBOT
        {
          
            public long msdEnquiryID { get; set; }
            public string enqrefNo { get; set; }
            public string quotationNo { get; set; }
            public string owner { get; set; }

            public string ownerEmailid { get; set; }
            public string shipName { get; set; }
            public string leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string leadTimeforitem { get; set; }
            public string rfqUrl { get; set; }

            public string quotationReceiveddate { get; set; }
            public string quotationSubmitdate { get; set; }

            public string rfqPassword { get; set; }

            public string authCode { get; set; }

            public string as400UserId { get; set; }

            public string emailProcessedat { get; set; }

            public string emailSubject { get; set; }
            public List<EnquiryDetailsForMSDBOT> itemDetails { get; set; }

        }

        public class EnquiryDetailsForMSDBOT
        {
            public string seqNo { get; set; }
            public string quantity { get; set; }
            public string unit { get; set; }
            public string cost { get; set; }
            public string leadTime { get; set; }
            public string leadTimeperiod { get; set; }
            public string as400Description { get; set; }

        }

        public class MessageMSD
        {
            public string result { get; set; }
        }

        public class MessageWithRFQData
        {
            public string result { get; set; }
            public string ownerEmailId { get; set; }
        }
        public class MSDRFQData
        {
            public string errorRFQ { get; set; }
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
            public long PK_MSDENQUIRY_HDR_ID { get; set; }
            public int Ownership { get; set; }
            public string action { get; set; }
            public string loginId { get; set; }
            public string userName { get; set; }

        }

       

      
    }
}
