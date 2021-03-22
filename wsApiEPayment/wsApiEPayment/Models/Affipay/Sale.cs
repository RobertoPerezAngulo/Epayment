using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class Sale
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public CustomerInformation customerInformation { get; set; }
        public NoPresentCardData noPresentCardData { get; set; }
    }

    public class CustomerInformation
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string middleName { get; set; }
        public string email { get; set; }
        public string phone1 { get; set; }
        public string city { get; set; }
        public string address1 { get; set; }
        public string postalCode { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string ip { get; set; }
    }

    public class NoPresentCardData
    {
        public string cardNumber { get; set; }
        public string cvv { get; set; }
        public string cardholderName { get; set; }
        public string expirationYear { get; set; }
        public string expirationMonth { get; set; }
    }
}