using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class Respuesta
    {
        public string Estado { get; set; }
        public string SolicitudId { get; set; }
        public string Id { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
    }

    public class BinInformacion
    {
        public string bin { get; set; }
        public string bank { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string brand { get; set; }
    }


}