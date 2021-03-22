using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class CancelSale
    {
        public string amount { get; set; }
        public NoPresentCardData noPresentCardData { get; set; }
    }
}