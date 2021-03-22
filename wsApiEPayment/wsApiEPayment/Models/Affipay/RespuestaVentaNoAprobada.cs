using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class RespuestaVentaNoAprobada: Respuesta
    {
        public ErrorNoAprobado error { get; set; }
    }

    public class ErrorNoAprobado
    {
        public string httpStatusCode { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public binInformation binInformation { get; set; }
        public long IdPagoElectronico { get; set; }
    }
}