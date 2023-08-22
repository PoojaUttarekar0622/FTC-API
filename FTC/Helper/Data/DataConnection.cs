using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Helper.Data
{
    public class DataConnection : DbContext
    {
        public DataConnection(DbContextOptions<DataConnection> options) : base(options)
        {

        }

       
        public DbSet<DataContextClass.MstUser> MstUserTable { get; set; }
        public DbSet<DataContextClass.MstRole> MstRoleTable { get; set; }
        public DbSet<DataContextClass.TrnLogin> TrnLoginTable { get; set; }
        public DbSet<DataContextClass.TMSD_ENQUIRY_HDR> TMSD_ENQUIRY_HDRTable { get; set; }
        public DbSet<DataContextClass.TMSD_ENQUIRY_DTL> TMSD_ENQUIRY_DTLTable { get; set; }
        public DbSet<DataContextClass.TMSD_ENQUIRY_HDR_ORG> TMSD_ENQUIRY_HDR_ORGTable { get; set; }
        public DbSet<DataContextClass.TMSD_ENQUIRY_DTL_ORG> TMSD_ENQUIRY_DTL_ORGTable { get; set; }
        public DbSet<DataContextClass.TSNQ_ENQUIRY_HDR> TSNQ_ENQUIRY_HDRTable { get; set; }
        public DbSet<DataContextClass.TSNQ_ENQUIRY_DTL> TSNQ_ENQUIRY_DTLTable { get; set; }
        public DbSet<DataContextClass.T_SNQ_ENQUIRY_ML_ITEMS> T_SNQ_ENQUIRY_ML_ITEMSTable { get; set; }
        public DbSet<DataContextClass.TSNQ_ENQUIRY_HDR_ORG> TSNQ_ENQUIRY_HDR_ORGTable { get; set; }
        public DbSet<DataContextClass.TSNQ_ENQUIRY_DTL_ORG> TSNQ_ENQUIRY_DTL_ORGTable { get; set; }
        public DbSet<DataContextClass.MSTATUS_CODE> M_STATUS_CODE { get; set; }
        public DbSet<DataContextClass.MERROR_CODE> M_ERROR_CODE { get; set; }
        public DbSet<DataContextClass.MMANUFACTURER> MMANUFACTURERTable { get; set; }
        public DbSet<DataContextClass.MCUSTOMER_MAPPING> MCUSTOMER_MAPPINGTable { get; set; }
        public DbSet<DataContextClass.MMANUFACTURER_SUPPLIER_MAPPING> MMANUFACTURER_SUPPLIER_MAPPINGTable { get; set; }
        public DbSet<DataContextClass.MM_PORT_MAPPING> MM_PORT_MAPPINGTable { get; set; }
        public DbSet<DataContextClass.MCUST_DEPT_ACC_MAPPING> MCUST_DEPT_ACC_MAPPINGTable { get; set; }
        public DbSet<DataContextClass.MMANUFACTURER_MAPPING> MMANUFACTURER_MAPPINGTable { get; set; }

        public DbSet<DataContextClass.MCUSTOMER> MCUSTOMERTable { get; set; }
        public DbSet<DataContextClass.MUOM> MUOMTable { get; set; }
        public DbSet<DataContextClass.M_MLTYPE> M_MLTYPETable { get; set; }
        public DbSet<DataContextClass.M_PROCESS> M_PROCESSTable { get; set; }

        public DbSet<DataContextClass.M_EVENT> M_EVENTTable { get; set; }


        public DbSet<DataContextClass.T_DOCUMENT_HDR> T_DOCUMENT_HDRTable { get; set; }

        public DbSet<DataContextClass.T_DOCUMENT_DTL> T_DOCUMENT_DTLTable { get; set; }


        public DbSet<DataContextClass.T_SNQ_DOCUMENT_HDR> T_SNQ_DOCUMENT_HDRTable { get; set; }

        public DbSet<DataContextClass.T_SNQ_DOCUMENT_DTL> T_SNQ_DOCUMENT_DTLTable { get; set; }

        public DbSet<DataContextClass.M_SOURCETYPE> M_SOURCETYPETable { get; set; }

        public DbSet<DataContextClass.M_MSD_ITEMS> M_MSD_ITEMSTable { get; set; }

        public DbSet<DataContextClass.M_SNQ_ITEMS> M_SNQ_ITEMSTable { get; set; }

        public DbSet<DataContextClass.T_MESPAS_ENQUIRY_HDR> T_MESPAS_ENQUIRY_HDRTable { get; set; }
        public DbSet<DataContextClass.T_MESPAS_ENQUIRY_HDR_ORG> T_MESPAS_ENQUIRY_HDR_ORGTable { get; set; }
        public DbSet<DataContextClass.T_MESPAS_ENQUIRY_DTL> T_MESPAS_ENQUIRY_DTLTable { get; set; }

        public DbSet<DataContextClass.T_MESPAS_ENQUIRY_DTL_ORG> T_MESPAS_ENQUIRY_DTL_ORGTable { get; set; }

        public DbSet<DataContextClass.T_MESPAS_DOCUMENT_HDR> T_MESPAS_DOCUMENT_HDRTable { get; set; }

        public DbSet<DataContextClass.T_MESPAS_DOCUMENT_DTL> T_MESPAS_DOCUMENT_DTLTable { get; set; }

        public DbSet<DataContextClass.M_DEPT> M_DEPTTable { get; set; }
        public DbSet<DataContextClass.M_MAKER> M_MAKERTable { get; set; }
        public DbSet<DataContextClass.M_EQUIPMENT> M_EQUIPMENTTable { get; set; }

    }

}
