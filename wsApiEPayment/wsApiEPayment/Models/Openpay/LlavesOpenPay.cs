using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Openpay
{
    public class LlavesOpenPay
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public string LlaveOPenPayPrivada { get; set; }
        public string LlaveOpenPayPublica { get; set; }
        public int IdMarca { get; set; }
        public string IdComercio { get; set; }
    }


    public class OpenPayLlavesSeguridad {
        public string llavePrivada { get; set; }
        public string llavePublica { get; set; }
        public string Comercio { get; set; }
    }
}