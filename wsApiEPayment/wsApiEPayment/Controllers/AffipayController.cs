using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

using wsApiEPayment.Models.Affipay;
using wsApiEPayment.Services;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace wsApiEPayment.Controllers
{
    public class AffipayController : ApiController
    {
        private AffipayService servicio;

        public AffipayController()
        {
            servicio = new AffipayService();
        }

        // POST api/affipay/PostRegistraCuenta
        [Route("api/affipay/PostRegistraVenta", Name = "PostRegistraVenta")]
        public Respuesta PostRegistraVenta(int aIdEmpresa,[FromBody]Venta Venta)
        {
            return servicio.RegistraVenta(aIdEmpresa,Venta);
        }

        // POST api/affipay/PostCancelacionVenta
        [Route("api/affipay/PostCancelacionVenta", Name = "PostCancelacionVenta")]
        public Object PostCancelacionVenta(int aIdEmpresa,[FromBody]CancelarVenta CancelarVenta, string aIdCargo)
        {
            return servicio.CancelacionVenta(aIdEmpresa, CancelarVenta, aIdCargo);
        }

    }
}
