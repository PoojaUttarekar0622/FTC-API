using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Enum
{
  public  class Status
    {
        #region
        public enum statusCode
        {
            notStartedStatus = 1,
            verifiedStatus = 2,
            inprocessStatus = 3,
            successStatus = 4,
            errorStatus = 5,
            updatedStatus = 6,
            forwardToManualStatus = 7,
            rejectedStatus = 8,
            successManualStatus = 9
        }
        #endregion
    }
}
