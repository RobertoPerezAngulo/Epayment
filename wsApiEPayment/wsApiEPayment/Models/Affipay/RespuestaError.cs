using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class RespuestaError: Respuesta
    {
        public ErrorPermiso error { get; set; }
    }

    public class ErrorPermiso
    {
        public string httpStatusCode { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }
}