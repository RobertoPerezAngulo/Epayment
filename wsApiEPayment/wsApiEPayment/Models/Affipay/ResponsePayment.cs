using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class ResponsePayment
    {
        public string status { get; set; }
        public string requestId { get; set; }
        public string id { get; set; }
        public string date { get; set; }
        public string time { get; set; }
    }

    public class binInformation
    {
        public string bin { get; set; }
        public string bank { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string brand { get; set; }
    }
}