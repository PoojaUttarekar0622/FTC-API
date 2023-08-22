using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.TaskClass;

namespace Helper.Interface
{
    public interface ITaskDtl
    {
        #region
        StatusMSD GetStatusCountForMSD(string fromdate, string todate);
        StatusSNQ GetStatusCountForSNQ(string fromdate, string todate);
        #endregion
    }
}
