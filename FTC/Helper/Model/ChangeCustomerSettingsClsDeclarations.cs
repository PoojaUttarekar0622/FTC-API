using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Model
{
    public class ChangeCustomerSettingsClsDeclarations
    {
        public class ChangeCustomerSettingsSummary
        {
            public long customerId { get; set; }
            public string templateCustomerName { get; set; }
            public string AS400CustomerName { get; set; }
            public string AS400UserId { get; set; }
            public string customerEmailId { get; set; }
        }
    }
}
