
using Helper.Report;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Interface
{
    public interface ISNQReport
    {
        #region
        List<SNQReportSummary> GetSNQDaily_Report(DailyReport objDailyReport);
        #endregion
    }
}
