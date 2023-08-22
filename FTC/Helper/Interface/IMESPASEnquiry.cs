using Helper.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.MESPASClassDeclarations;

namespace Helper.Interface
{
  public  interface IMESPASEnquiry
    {
        #region
        public Message InsertEnquiry(Enquiryheader objEnquirydtls);
        public List<Department> GetDeptData();
        public List<Enquiryheaderdata> GetEnquiryHeader(int status);

        public List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata);
        public Enquiryheader GetEnquiryDetails(long fkenquiryid);
        public Message DeleteItems(List<EnquiryDetails> objEnquirydtls);

        public Message UpdateEnquiry(Enquiryheader objEnquirydtls);
        public Message UpdateSaveAsEnquiry(Enquiryheader objEnquiry);
        public Message Updateownership(EnqOwnership objOwnershipdtls);
        public Message Releaseownership(EnqOwnership objEnquirydtls);
        public EnqOwnership Getuserownershipdtl(int PKHDRID);
        #endregion
    }

}
