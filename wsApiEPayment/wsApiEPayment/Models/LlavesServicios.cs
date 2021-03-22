using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models
{
    public class LlavesServicios
    {
        public int IdEmpresa { get; set; }
        public string Nombre { get; set; }
        public OpenPaySeguridad Openpay { get; set; }
        public AffiPaySeguridad AffiPay { get; set; }
    }

    public class OpenPaySeguridad
    {
        public string LlaveOPenPayPrivada { get; set; }
        public string LlaveOpenPayPublica { get; set; }
        public string IdComercioOpenPay { get; set; }
    }

    public class AffiPaySeguridad
    {
        public string UserAffiPay { get; set; }
        public string PaswordAffiPay { get; set; }
        public string PaswordEcommerce { get; set; }
    }

}