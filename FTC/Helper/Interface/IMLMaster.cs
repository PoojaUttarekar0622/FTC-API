using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.MLClassDeclarations;

namespace Helper.Interface
{
  public interface IMLMaster
    {
        #region
        public List<MLMasterData1> GetMLMasterData(MLData objmlMaster);

        public List<MLData2> GetIDwiseMLList(MLData objmlMaster);

        public Message DeleteMlMaster(List<MLData> objmlMaster);
        public Message SaveMLMaster(List<MLData> objmlMaster);
        public List<MLProcessType> GetMLProcessType();
        public List<MLType> GetMLType(MLType objMLType);
        public List<PagignationData> GetPagignationData(PagignationData objPagignationData);

        public Message RegisterNewImpa(MLData objmlMaster);

        public List<MLMasterData1> GetMLDataBySearch(searchdata objsearchdata);
        #endregion
    }
}
