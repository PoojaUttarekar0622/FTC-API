using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.MSDClassDeclarations;

namespace Helper.Interface
{
    public interface IMSDEnquiry
    {
        #region
        List<Enquiryheaderdata> GetEnquiryHeader(int status);
        Enquiryheaderdata GetEnquiryDetails(long fkenquiryid);
        MessageMSD InsertEnquiry(Enquiryheader objEnquirydtls);
        MessageMSD UpdateEnquiry(Enquiryheader objEnquirydtls);
        MessageMSD UpdateEnqStatus(EnquiryheaderdataForas400 objEnquirydtls);
        List<EnquiryheaderdataForas400> GetDetailsForAs400();
        MessageMSD DeleteItems(List<Enquirydetailsdata> objEnquirydtls);
        List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata);
        List<Enquiryheaderdata> GetErrorTaskList(searchdata objsearchdata);
        List<Enquiryheaderdata> GetCompletedTaskList(searchdata objsearchdata);
        MessageMSD Updateownership(EnqOwnership objEnquirydtls);
        MessageMSD Releaseownership(EnqOwnership objEnquirydtls);
        EnquiryheaderForMSDBOT GetRFQURL(string RFQNO);
        MessageMSD UpdatePriceandLeadtime(EnquiryheaderForMSDBOT objEnquirydtls);
        EnqOwnership Getuserownershipdtl(int PKHDRID);

        MessageMSD UpdateSaveAsEnquiry(Enquiryheader objEnquiry);

        public MessageMSD VerifyRFQ(string errorRFQ);
        #endregion
    }
}
