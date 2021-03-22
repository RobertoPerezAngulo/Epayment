using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class ResponseSale: ResponsePayment
    {
        public dataResponse dataResponse { get; set; }
    }

    public class dataResponse
    {
        public string authorization { get; set; }
        public string description { get; set; }
        public binInformation binInformation { get; set; }
    }
}