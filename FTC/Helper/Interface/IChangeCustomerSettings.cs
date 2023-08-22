using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.ChangeCustomerSettingsClsDeclarations;
using static Helper.Model.MLClassDeclarations;

namespace Helper.Interface
{
    public interface IChangeCustomerSettings
    {
        #region
        public List<ChangeCustomerSettingsSummary> GeCustomerData();
        public Message UpdateCustomerData(ChangeCustomerSettingsSummary objCustomerData);
        #endregion
    }
}
