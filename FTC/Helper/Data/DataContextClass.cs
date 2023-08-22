using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Helper.Data
{
    #region
    public class DataContextClass
    {
        [Table("M_USER")]
        public class MstUser
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_USER_ID { get; set; }
            public long FK_ROLE_ID { get; set; }
            public string USER_NAME { get; set; }
            public string LOGIN_NAME { get; set; }
            public string PASSWORD { get; set; }
            public string? EMP_CODE { get; set; }
            public string EMAIL_ID { get; set; }
            public string PHONE_NO { get; set; }
            public string MOBILE_NO { get; set; }
            public string? REPORTING_ID { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_DATE { get; set; }
            public int IS_ACTIVE { get; set; }

        }

        [Table("M_ROLE")]
        public class MstRole
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_ROLE_ID { get; set; }
            public string ROLE_NAME { get; set; }
            public int IS_ACTIVE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_DATE { get; set; }


        }


        [Table("T_LOGIN")]
        public class TrnLogin
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_LOGIN_ID { get; set; }
            public string LOGIN_ID { get; set; }
            public DateTime LOGIN_TIME { get; set; }
            public DateTime? LOGOUT_TIME { get; set; }
            public string TOKEN_ID { get; set; }
            public string DOMAIN { get; set; }
            public string APP_SESSION_ID { get; set; }
            public string DOMAIN_HOST_NAME { get; set; }
            public int? CREATED_BY { get; set; }
            public DateTime? CREATED_DATE { get; set; }

            public int ISACTIVE { get; set; }

        }

        [Table("M_STATUSCODE")]
        public class MSTATUS_CODE
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_STATUSCODE_ID { get; set; }
            public int STATUS_CODE { get; set; }
            public string STATUS_DESCRIPTION { get; set; }
            public int IS_ACTIVE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }

        }

        [Table("M_ERROR_CODE")]
        public class MERROR_CODE
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_ERROR_ID { get; set; }
            public string ERROR_CODE { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int IS_ACTIVE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }

        }

        [Table("M_CUSTOMER_MAPPING")]
        public class MCUSTOMER_MAPPING
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_CUSTOMER_MAPPING_ID { get; set; }
            public long FK_CUST_ID { get; set; }
            public string TEMPLATE_CUSTOMER_NAME { get; set; }
            public string AS400_CUSTOMER_NAME { get; set; }

            public string CUST_GROUP { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }

        }

        [Table("CUST_DEPT_ACC_MAPPING")]
        public class MCUST_DEPT_ACC_MAPPING
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_ACCOUNT_ID { get; set; }
            public long FK_CUST_ID { get; set; }
            public string ACC_DESCRIPTION { get; set; }
            public string ACCOUNT_CODE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public int IS_ACTIVE { get; set; }

        }

        [Table("M_MANUFACTURER")]
        public class MMANUFACTURER
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_MANUFACTURER_ID { get; set; }
            public string MANUFACTURER_NAME { get; set; }
            public string MANUFACTURER_CODE { get; set; }

        }

        [Table("M_MANUFACTURER_MAPPING")]
        public class MMANUFACTURER_MAPPING
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_MANUFACTURER_MAPPING_ID { get; set; }
            public long FK_MANUFACTURER_ID { get; set; }
            public string TEMPLATE_MANUFACTURER_NAME { get; set; }
            public string AS400_MANUFACTURER_NAME { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }

        }

        [Table("M_MANUFACTURER_SUPPLIER_MAPPING")]
        public class MMANUFACTURER_SUPPLIER_MAPPING
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_MANUF_SUPP_ID { get; set; }
            public long FK_MANUFACTURER_ID { get; set; }
            public string SUPPLIER_CODE { get; set; }

        }

        [Table("M_PORT_MAPPING")]
        public class MM_PORT_MAPPING
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_PORTMAPP_ID { get; set; }
            public string TEMPLATE_PORTNAME { get; set; }
            public string AS400_PORT { get; set; }

        }

        [Table("M_UOM")]
        public class MUOM
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_UOM_ID { get; set; }
            public string TEMPLATE_UOM { get; set; }
            public string AS400_UOM { get; set; }
            public int? CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }

        }

        [Table("T_MSD_ENQUIRY_HDR")]
        public class TMSD_ENQUIRY_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MSDENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public long FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public int STATUS { get; set; }
            public string QUOTATION_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string EMAIL_RECEIVED_AT { get; set; }
            public string EMAIL_PROCESSED_AT { get; set; }
            public string IN_ERROR_AT { get; set; }
            public string VERIFIED_AT { get; set; }
            public string UPDATED_AT { get; set; }
            public string QUOTATION_CREATED_AT { get; set; }
            public string SUPPLIER_CODE { get; set; }
            public int LEAD_TIME { get; set; }
            public string LEAD_TIME_PERIOD { get; set; }
            public string LEAD_TIME_FOR_ITEM { get; set; }
            public int OWNERSHIP { get; set; }
            public string RFQ_URL { get; set; }
            public string VERIFIED_BY { get; set; }
            public string CORRECTED_BY { get; set; }
            public string QUATATION_SUBMIT_DATE { get; set; }
            public string EMAIL_FROM { get; set; }

            public string MAKER_INFO { get; set; }

            public string SAVE_AS_DRAFT { get; set; }

            public string SOURCE_TYPE { get; set; }

            public string QUOTATION_RECIVED_AT { get; set; }

          
            public long EVENT_ID { get; set; }

            public string ISADDITIONL_ROUND { get; set; }

            public int IS_UPDATED_MAKER_WITH_ML { get; set; }

            public int IS_UPDATED_EQUIPMENT_WITH_ML { get; set; }

            public int ERROR_VERIFIED_COUNTER { get; set; }

            public string EMAIL_SUBJECT { get; set; }

            public string RFQ_PASSWORD { get; set; }

            public string REMARK { get; set; }

            public string AUTH_CODE { get; set; }
        }

        [Table("T_MSD_ENQUIRY_DTL")]
        public class TMSD_ENQUIRY_DTL
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MSDENQUIRY_DTL_ID { get; set; }
            public long FK_MSDENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public DateTime UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int STATUS { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public int IS_UPDATED_WITH_ML { get; set; }
            public int LEAD_TIME { get; set; }
            public string LEAD_TIME_PERIOD { get; set; }
            public string SEQ_NOTEXT { get; set; }
            public string AS400_DESCRIPTION { get; set; }
            public int IS_UPDATED_MESPASITEMS_WITH_ML { get; set; }


        }

        [Table("T_MSD_ENQUIRY_HDR_ORG")]
        public class TMSD_ENQUIRY_HDR_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MSDENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public long FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string SUPPLIER_CODE { get; set; }
            public string RFQ_URL { get; set; }

            public string MAKER_INFO { get; set; }

            public string SOURCE_TYPE { get; set; }
        }

        [Table("T_MSD_ENQUIRY_DTL_ORG")]
        public class TMSD_ENQUIRY_DTL_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MSDENQUIRY_DTL_ID { get; set; }
            public long FK_MSDENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public DateTime UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }


        }

        [Table("T_SNQ_ENQUIRY_HDR")]
        public class TSNQ_ENQUIRY_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_SNQENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public int FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public int STATUS { get; set; }
            public string QUOTATION_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string EMAIL_RECEIVED_AT { get; set; }
            public string EMAIL_PROCESSED_AT { get; set; }
            public string IN_ERROR_AT { get; set; }
            public string VERIFIED_AT { get; set; }
            public string UPDATED_AT { get; set; }
            public string QUOTATION_CREATED_AT { get; set; }
            public string PORT { get; set; }
            public string DELIVERY_DATE { get; set; }
            public string MAIL_SUBJECT { get; set; }
            public int OWNERSHIP { get; set; }
            public string VERIFIED_BY { get; set; }
            public string CORRECTED_BY { get; set; }

            public string AS400_MAPPING_PORT { get; set; }

            public string SAVE_AS_DRAFT { get; set; }

            public string SOURCE_TYPE { get; set; }

            public long EVENT_ID { get; set; }

            public int IS_UPDATED_MAKER_WITH_ML { get; set; }

            public int IS_UPDATED_EQUIPMENT_WITH_ML { get; set; }

            public string RFQ_URL { get; set; }
            public int ERROR_VERIFIED_COUNTER { get; set; }

        }

        [Table("T_SNQ_ENQUIRY_DTL")]
        public class TSNQ_ENQUIRY_DTL
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_SNQENQUIRY_DTL_ID { get; set; }
            public long FK_SNQENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public DateTime UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int STATUS { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public int IS_UPDATED_WITH_ML { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string SEQ_NOTEXT { get; set; }

            public int IS_UPDATED_MESPASITEMS_WITH_ML { get; set; }


        }

        [Table("T_SNQ_ENQUIRY_HDR_ORG")]
        public class TSNQ_ENQUIRY_HDR_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_SNQENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public int FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string PORT { get; set; }
            public string DELIVERY_DATE { get; set; }
            public string AS400_MAPPING_PORT { get; set; }
            public string SOURCE_TYPE { get; set; }
        }

        [Table("T_SNQ_ENQUIRY_DTL_ORG")]
        public class TSNQ_ENQUIRY_DTL_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_SNQENQUIRY_DTL_ID { get; set; }
            public long FK_SNQENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public DateTime UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }



        }

        [Table("T_SNQ_ENQUIRY_ML_ITEMS")]
        public class T_SNQ_ENQUIRY_ML_ITEMS
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_ITEM_ID { get; set; }
            public string PART_NAME { get; set; }
            public string PART_CODE { get; set; }
            public DateTime CREATEDAT { get; set; }
            public int? IS_ACTIVE { get; set; }

        }


        [Table("M_CUSTOMER")]
        public class MCUSTOMER
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long PK_CUSTOMER_ID { get; set; }
            public string CUSTOMERNAME { get; set; }
            public int IS_ACTIVE { get; set; }
            public string AUTOVERIFICATION { get; set; }
            public string CUSTOMER_EMAILID { get; set; }
            public string AS400USER_ID { get; set; }
            public string DEPT_NAME { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }



        }

        [Table("M_PROCESS")]
        public class M_PROCESS
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_PROCESS_ID { get; set; }
            public string PROCESS_TYPE { get; set; }
            public int IS_ACTIVE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_DATE { get; set; }

        }

        [Table("M_MLTYPE")]
        public class M_MLTYPE
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_MLTYPE { get; set; }
            public int FK_PROCESSTYPE_ID { get; set; }
            public string MLTYPE { get; set; }
            public int IS_ACTIVE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_DATE { get; set; }

        }
        [Table("M_EVENT")]
        public class M_EVENT
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_EVENTID { get; set; }
            public long EVENT_ID { get; set; }

            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int ISACTIVE { get; set; }


        }
        [Table("T_DOCUMENT_HDR")]
        public class T_DOCUMENT_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_HDR_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }


        [Table("T_DOCUMENT_DTL")]
        public class T_DOCUMENT_DTL
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_DTL_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }


        [Table("T_SNQ_DOCUMENT_HDR")]
        public class T_SNQ_DOCUMENT_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_HDR_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }


        [Table("T_SNQ_DOCUMENT_DTL")]
        public class T_SNQ_DOCUMENT_DTL
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_DTL_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }

        [Table("M_SOURCETYPE")]
        public class M_SOURCETYPE
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_SOURCETYPE_ID { get; set; }
            public string SOURCE_TYPE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int ISACTIVE { get; set; }
        }


     

        [Table("T_MESPAS_ENQUIRY_HDR")]
        public class T_MESPAS_ENQUIRY_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MESPASENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public long FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public int STATUS { get; set; }
            public string QUOTATION_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string EMAIL_RECEIVED_AT { get; set; }
            public string EMAIL_PROCESSED_AT { get; set; }
            public string IN_ERROR_AT { get; set; }
            public string VERIFIED_AT { get; set; }
            public string UPDATED_AT { get; set; }
            public string QUOTATION_CREATED_AT { get; set; }

            public string PORT { get; set; }

            public string MAPPING_PORT { get; set; }
            public string SUPPLIER_CODE { get; set; }
            public int LEAD_TIME { get; set; }
            public string LEAD_TIME_PERIOD { get; set; }
            public string LEAD_TIME_FOR_ITEM { get; set; }
            public int OWNERSHIP { get; set; }
            public string RFQ_URL { get; set; }
            public string VERIFIED_BY { get; set; }
            public string CORRECTED_BY { get; set; }
            public string QUATATION_SUBMIT_DATE { get; set; }
            public string EMAIL_FROM { get; set; }

            public string MAKER_INFO { get; set; }

            public string SAVE_AS_DRAFT { get; set; }

            public string SOURCE_TYPE { get; set; }

            public long EVENT_ID { get; set; }

            public string REJECTED_BY { get; set; }
            // pub 

        }
        [Table("T_MESPAS_ENQUIRY_HDR_ORG")]
        public class T_MESPAS_ENQUIRY_HDR_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MESPASENQUIRY_HDR_ID { get; set; }
            public string ENQUIRY_DATE { get; set; }
            public string OWNER { get; set; }
            public string OWNER_EMAILID { get; set; }
            public string SHIP_NAME { get; set; }
            public int FK_PROCESS_ID { get; set; }
            public long FK_INSTANCE_ID { get; set; }
            public string DOC_PATH { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime CREATED_DATE { get; set; }
            public string RAISED_BY { get; set; }
            public string ERROR_CODE { get; set; }
            public string MAIL_BODY { get; set; }
            public string ENQREF_NO { get; set; }
            public string MAKER { get; set; }
            public string TYPE { get; set; }
            public string EQUIPMENT { get; set; }
            public string SERIAL_NO { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }
            public string NET_AMOUNT { get; set; }
            public string SUPPLIER_CODE { get; set; }
            public string RFQ_URL { get; set; }

            public string MAKER_INFO { get; set; }

            public string SOURCE_TYPE { get; set; }
        }

        [Table("T_MESPAS_ENQUIRY_DTL_ORG")]
        public class T_MESPAS_ENQUIRY_DTL_ORG
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MESPASENQUIRY_DTL_ID { get; set; }
            public long FK_MESPASENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public string UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public string DISCOUNT_AMOUNT { get; set; }


        }

        [Table("T_MESPAS_DOCUMENT_HDR")]
        public class T_MESPAS_DOCUMENT_HDR
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_HDR_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }


        [Table("T_MESPAS_DOCUMENT_DTL")]
        public class T_MESPAS_DOCUMENT_DTL
    {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long DOC_ID { get; set; }

            public long ENQUIRY_DTL_ID { get; set; }
            public string DOC_PATH { get; set; }
            public string ERROR_DESCRIPTION { get; set; }
            public int CREATEBY { get; set; }
            public DateTime? CREATEDAT { get; set; }

            public int ISACTIVE { get; set; }

        }


        [Table("T_MESPAS_ENQUIRY_DTL")]
        public class T_MESPAS_ENQUIRY_DTL
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public long PK_MESPASENQUIRY_DTL_ID { get; set; }
            public long FK_MESPASENQUIRY_HDR_ID { get; set; }
            public string PART_CODE { get; set; }
            public string PART_NAME { get; set; }
            public string QUANTITY { get; set; }
            public string UNIT { get; set; }
            public string PRICE { get; set; }
            public string COST { get; set; }
            public string UPDATED_DATE { get; set; }
            public string SUPPLIER { get; set; }
            public int STATUS { get; set; }
            public int SEQ_NO { get; set; }
            public string ERROR_CODE { get; set; }
            public string ACCOUNT_NO { get; set; }
            public string ACCOUNT_DESCRIPTION { get; set; }
            public int IS_UPDATED_WITH_ML { get; set; }
            public int LEAD_TIME { get; set; }
            public string LEAD_TIME_PERIOD { get; set; }
            public string SEQ_NOTEXT { get; set; }
            public string AS400_DESCRIPTION { get; set; }


        }

        [Table("M_DEPT")]
        public class M_DEPT
        {

            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_DEPT_ID { get; set; }

            public string DEPT_NAME { get; set; }
            public int CRETAEDBY { get; set; }

            public DateTime? CREATED_AT { get; set; }
            public int ISACTIVE { get; set; }
        }

        [Table("M_MSD_ITEMS")]
        public class M_MSD_ITEMS
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_ITEM_ID { get; set; }
            public string ITEM_NAME { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int IS_ACTIVE { get; set; }
        }


        [Table("M_SNQ_ITEMS")]
        public class M_SNQ_ITEMS
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_ITEM_ID { get; set; }
            public string ITEM_NAME { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int IS_ACTIVE { get; set; }
        }


        [Table("M_MAKER")]
        public class M_MAKER
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_MAKER_ID { get; set; }
            public string MAKER_NAME { get; set; }
            public string MAKER_CODE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int IS_ACTIVE { get; set; }
        }

        [Table("M_EQUIPMENT")]
        public class M_EQUIPMENT
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

            public int PK_EQUIPMENT_ID { get; set; }
            public string EQUIPMENT_NAME { get; set; }
            public string EQUIPMENT_CODE { get; set; }
            public int CREATED_BY { get; set; }
            public DateTime? CREATED_AT { get; set; }
            public int IS_ACTIVE { get; set; }
        }

    }

    #endregion
}
