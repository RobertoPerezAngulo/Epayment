using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class CancelarVenta
    {
        public string Cantidad { get; set; }
        public DatosTarjeta DatosTarjeta { get; set; }
    }
}