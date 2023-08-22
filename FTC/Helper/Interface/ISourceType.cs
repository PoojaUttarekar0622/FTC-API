using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.SourceTypeClassDeclarations;

namespace Helper.Interface
{
   public interface ISourceType
    {
        #region
        public List<SourceTypeSummary> GeSourceTypeData();
        #endregion

    }
}
