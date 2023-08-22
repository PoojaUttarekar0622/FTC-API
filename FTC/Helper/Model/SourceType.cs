using Helper.Data;
using Helper.Hub_Config;
using Helper.Interface;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using static Helper.Model.SourceTypeClassDeclarations;
using System.Linq;
namespace Helper.Model
{
    public class SourceType : ISourceType
    {
        #region created object of connection and configuration class
        public DataConnection _datacontext;
        private IConfiguration _Configuration;
        private readonly IHubContext<SignalRHub, ITypedHubClient> _hub;
        #endregion
        #region  created controller of that class
        public SourceType(DataConnection datacontext, IConfiguration configuration, IHubContext<SignalRHub, ITypedHubClient> hub)
        {
            _datacontext = datacontext;
            _Configuration = configuration;
            _hub = hub;
        }
        #endregion
        #region  get source type list i.e MESPAS ,RPA etc.
        public List<SourceTypeSummary> GeSourceTypeData()
        {
            #region created object of class
            List<SourceTypeSummary> lstSourceType = new List<SourceTypeSummary>();
            #endregion
            try
            {
                #region select source type data
                var sourceDatadata = (from p in _datacontext.M_SOURCETYPETable
                                      where p.ISACTIVE == 1
                                      select new
                                      {
                                          p.PK_SOURCETYPE_ID,
                                          p.SOURCE_TYPE,
                                      }).ToList();
                #endregion
                if (sourceDatadata != null)
                {
                    #region add data in list
                    foreach (var data in sourceDatadata)
                    {
                        SourceTypeSummary objdata = new SourceTypeSummary();
                        objdata.sourceTypeId = data.PK_SOURCETYPE_ID;
                        objdata.sourceType = data.SOURCE_TYPE;
                        lstSourceType.Add(objdata);
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
            }
            return lstSourceType;
        }
        #endregion
    }
}
