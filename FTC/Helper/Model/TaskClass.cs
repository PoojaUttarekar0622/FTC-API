using System;
using Helper.Data;
using System.Linq;
using static Helper.Data.DataContextClass;
using Helper.Interface;
namespace Helper.Model
{
    public class TaskClass : ITaskDtl
    {
        #region created object of connection and configuration class
        public DataConnection _datacontext;
        #endregion
        #region  created controller of that class
        public TaskClass(DataConnection datacontext)
        {
            _datacontext = datacontext;
        }
        #endregion
        #region get count of MSD RFQ's i.e pedning ,error , completed etc
        public StatusMSD GetStatusCountForMSD(string fromdate, string todate)
        {
            #region created object of class
            StatusMSD objStatus = new StatusMSD();
            #endregion
            #region  get RFQ's count by particular date
            if (fromdate != "" && todate != "")
            {
                DateTime fromDt = Convert.ToDateTime(fromdate);
                DateTime toDt = Convert.ToDateTime(todate);
                #region calculate pending rfq's count
                objStatus.MSDVerification = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                             where hdr.STATUS == 1 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                             select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate completed rfq's count
                objStatus.MSDCompleted = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                          where (hdr.STATUS == 4 || hdr.STATUS == 9) && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                          select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate error rfq's count
                objStatus.MSDError = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                      where hdr.STATUS == 5 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                      select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate inprocess or updated rfq's count
                objStatus.MSDProcessing = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                           where (hdr.STATUS == 2 || hdr.STATUS == 6) && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                           select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                objStatus.MSDInProcess = objStatus.MSDVerification + objStatus.MSDProcessing;
                #region calculate total rfq's count
                objStatus.MSDTotal = objStatus.MSDCompleted + objStatus.MSDInProcess + objStatus.MSDError;
                #endregion
            }
            #endregion
            #region  get all RFQ's count 
            else
            {
                #region calculate completed rfq's count
                objStatus.MSDCompleted = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                          where (hdr.STATUS == 4 || hdr.STATUS == 9)
                                          select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate pending rfq's count
                objStatus.MSDVerification = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                             where hdr.STATUS == 1
                                             select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate error rfq's count
                objStatus.MSDError = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                      where hdr.STATUS == 5
                                      select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate manual rfq's count
                objStatus.MSDManual = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                       where hdr.STATUS == 7
                                       select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate inprocess or updated rfq's count
                objStatus.MSDProcessing = (from hdr in _datacontext.TMSD_ENQUIRY_HDRTable
                                           where (hdr.STATUS == 2 || hdr.STATUS == 6)
                                           select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                objStatus.MSDInProcess = objStatus.MSDVerification + objStatus.MSDProcessing;
                #region calculate total rfq's count
                objStatus.MSDTotal = objStatus.MSDCompleted + objStatus.MSDInProcess + objStatus.MSDError;
                #endregion
            }
            #endregion
            return objStatus;
        }
        #endregion
         #region get count of SNQ RFQ's i.e pedning ,error , completed etc
        public StatusSNQ GetStatusCountForSNQ(string fromdate, string todate)
        {
            StatusSNQ objStatus = new StatusSNQ();
            #region get RFQ's count by particular date
            if (fromdate != "" && todate != "")
            {
                DateTime fromDt = Convert.ToDateTime(fromdate);
                DateTime toDt = Convert.ToDateTime(todate);
                #region calculate pending rfq's count
                objStatus.SNQVerification = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                             where hdr.STATUS == 1 && hdr.IS_ACTIVE == 1 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                             select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate completed rfq's count
                objStatus.SNQCompleted = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                          where hdr.STATUS == 4 && hdr.IS_ACTIVE == 1 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                          select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate error rfq's count
                objStatus.SNQError = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                      where hdr.STATUS == 5 && hdr.IS_ACTIVE==1 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                      select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate inprocess or updated rfq's count
                objStatus.SNQProcessing = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                           where (hdr.STATUS == 2 || hdr.STATUS == 6) && hdr.IS_ACTIVE == 1 && hdr.CREATED_DATE.Date >= fromDt && hdr.CREATED_DATE.Date <= toDt
                                           select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                objStatus.SNQInProcess = objStatus.SNQVerification + objStatus.SNQProcessing;
                #region calculate total rfq's count
                objStatus.SNQTotal = objStatus.SNQCompleted + objStatus.SNQInProcess + objStatus.SNQError;
                #endregion
            }
            #endregion
            #region  get  all RFQ's count
            else
            {
                #region calculate completed rfq's count
                objStatus.SNQCompleted = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                          where hdr.STATUS == 4 && hdr.IS_ACTIVE == 1
                                          select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate pending rfq's count
                objStatus.SNQVerification = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                             where hdr.STATUS == 1 && hdr.IS_ACTIVE == 1
                                             select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate error rfq's count
                objStatus.SNQError = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                      where hdr.STATUS == 5 && hdr.IS_ACTIVE == 1
                                      select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate manual rfq's count
                objStatus.SNQManual = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                       where hdr.STATUS == 7 && hdr.IS_ACTIVE == 1
                                       select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                #region calculate inprocess or updated rfq's count
                objStatus.SNQProcessing = (from hdr in _datacontext.TSNQ_ENQUIRY_HDRTable
                                           where (hdr.STATUS == 2 || hdr.STATUS == 6) && hdr.IS_ACTIVE == 1
                                           select hdr).OrderByDescending(x => x.CREATED_DATE).Count();
                #endregion
                objStatus.SNQInProcess = objStatus.SNQVerification + objStatus.SNQProcessing;
                #region calculate total rfq's count
                objStatus.SNQTotal = objStatus.SNQCompleted + objStatus.SNQInProcess + objStatus.SNQError;
                #endregion
            }
            #endregion
            return objStatus;
        }
        #endregion
        public class StatusSNQ
        {
            public int SNQCompleted { get; set; }
            public int SNQInProcess { get; set; }
            public int SNQProcessing { get; set; }
            public int SNQVerification { get; set; }
            public int SNQError { get; set; }
            public int SNQManual { get; set; }
            public int SNQTotal { get; set; }
        }
        public class StatusMSD
        {
            //MSD
            public int MSDCompleted { get; set; }
            public int MSDInProcess { get; set; }
            public int MSDProcessing { get; set; }
            public int MSDVerification { get; set; }
            public int MSDError { get; set; }
            public int MSDManual { get; set; }
            public int MSDTotal { get; set; }
        }
        public class searchdata
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string token { get; set; }
        }
    }
}
