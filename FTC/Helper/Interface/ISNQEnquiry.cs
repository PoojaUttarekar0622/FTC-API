using Helper.Model;
using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.SNQClassDeclarations;

namespace Helper.Interface
{
    public interface ISNQEnquiry
    {
        #region
        List<Enquiryheaderdata> GetEnquiryHeader(int status);
        Enquiryheaderdata GetEnquiryDetails(long fkenquiryid);
        MessageSNQ InsertEnquiry(Enquiryheader objEnquirydtls);
        MessageSNQ UpdateEnquiry(Enquiryheader objEnquirydtls);
        MessageSNQ UpdateEnqStatus(Enquiryheader objEnquirydtls);
        List<Enquiryheader1> GetDetailsForAS400();
        MessageSNQ DeleteItems(List<Enquirydetailsdata> objEnquirydtls);
        List<Enquiryheaderdata> GetTaskList(searchdata objsearchdata);
        List<Enquiryheaderdata> GetErrorTaskList(searchdata objsearchdata);
        List<Enquiryheaderdata> GetCompletedTaskList(searchdata objsearchdata);
        MessageSNQ Updateownership(EnqOwnership objEnquirydtls);
        MessageSNQ Releaseownership(EnqOwnership objEnquirydtls);
        EnqOwnership Getuserownershipdtl(int PKHDRID);
        MessageSNQ UpdateSaveAsEnquiry(Enquiryheader objEnquiry);
        public List<PortMapping> GetPortMappingData();
        public MessageSNQ VerifyRFQ(string errorRFQ);
        public List<Enquiryheaderdata> GetTaskListForShipName(shipSearchdata objsearchdata);
        public MessageSNQ UpdateRFQByShipName(List<Enquiryheaderdata> lstenquhdrdata);
        #endregion
    }
}
