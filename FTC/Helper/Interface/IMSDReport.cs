using Helper.Model;
using Helper.Report;
using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Interface
{
  public interface IMSDReport
    {
        #region
        List<MSDReportSummary> GetMSDDaily_Report(DailyReport objDailyReport);
        #endregion
    }
}