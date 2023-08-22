using Helper.Report;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Interface
{
  public interface IQuotationSubmitReport
    {
        #region
        public List<QuotationReportSummary> GetQuotationDaily_Report(DailyReport objDailyReport);
        #endregion
    }
}
