using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class RespuestaVenta: Respuesta
    {
        public DatosAprobacion DatosAprobacion { get; set; }
    }

    public class DatosAprobacion
    {
        public long IdPagoElectronico { get; set; }
        public string Autorizacion { get; set; }
        public string Descripcion { get; set; }
        public BinInformacion BinInformacion { get; set; }
    }
}