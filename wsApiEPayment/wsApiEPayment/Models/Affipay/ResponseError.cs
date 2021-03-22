using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class ResponseError
    {
        public string status { get; set; }
        public string requestId { get; set; }
        public string date { get; set; }
        public string time { get; set; }
        public error error { get; set; }
    }
    public class error
    {
        public string httpStatusCode { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }
}