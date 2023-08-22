using System;
using System.Collections.Generic;
using System.Text;

namespace Helper.Model
{
  public class MLClassDeclarations
    {
        public class MLMasterData
        {
            public string itemIdPK { get; set; }
            public string description { get; set; }
            public string tempDescription { get; set; }

            public bool isSelected { get; set; }

        }
        public class MLMasterData1
        {
            public string itemIdPK { get; set; }
            public string description { get; set; }
            public string tempDescription { get; set; }

       

        }
        public class MLData
        {
            public long itemIdPK { get; set; }
            public string mlMasterType { get; set; }
            public string processType { get; set; }
            public string description { get; set; }

            public string tempDescription { get; set; }

        }
        public class MLData2
        {
            public string itemIdPK { get; set; }
            public string mlMasterType { get; set; }
            public string processType { get; set; }
            public string description { get; set; }

            public string tempDescription { get; set; }

            public bool isSelected { get; set; }

        }
        public class Message
        {
            public string result { get; set; }
        }

        public class MLProcessType
        {
            public int processTypeId { get; set; }
            public string processType { get; set; }
        }

        public class MLType
        {
            public int mlTypeId { get; set; }
            public int processTypeId { get; set; }
            public string mlType { get; set; }
            public string processType { get; set; }

        }

        public class PagignationData
        {
            public string itemIdPK { get; set; }
            public string description { get; set; }
            public string tempDescription { get; set; }
            public string mlMasterType { get; set; }
            public string processType { get; set; }
            public int pageNumber { get; set; }

            public int pageSize { get; set; }
            public string search { get; set; }
        }

        public class searchdata
        {
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string token { get; set; }

            public string mlMasterType { get; set; }
            public string processType { get; set; }


        }
    }
}
