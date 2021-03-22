using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class ResponseSaleNoAproved: ResponsePayment
    {
        public errorNoAproved error { get; set; }
    }

    public class errorNoAproved
    {
        public string httpStatusCode { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public binInformation binInformation { get; set; }
    }
}