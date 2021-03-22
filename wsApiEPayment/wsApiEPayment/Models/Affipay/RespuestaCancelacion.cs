using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class RespuestaCancelacion
    {
        public string Estado { get; set; }
        public string Id { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public DataRespose respuestaAffipay { get; set; }
        public string IdPagoElectronico { get; set; }
    }

    public class DataRespose
    {
        public string Autorizacion { get; set; }
        public string Descripcion { get; set; }
        public binInformation Informacion { get; set; }
    }

    
}