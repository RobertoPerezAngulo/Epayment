using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class ResponseCancel
    {
        public string status { get; set; }
        public string requestId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public DataResposeAffipay dataResponse { get; set; }
    }
    public class DataResposeAffipay
    {
        public string Authorization { get; set; }
        public string Description { get; set; }
        public binInformation binInformation { get; set; }
    }
}