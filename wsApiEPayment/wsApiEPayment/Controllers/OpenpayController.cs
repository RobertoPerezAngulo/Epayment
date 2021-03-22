using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using wsApiEPayment.Models;
using wsApiEPayment.Models.Openpay;
using wsApiEPayment.Services.Openpay;


namespace wsApiEPayment.Controllers
{
    public class OpenpayController : ApiController
    {

        [Route("api/openpay/PostCargoComercio", Name = "PostCargoComercio")]
        public Object PostCargoComercio(int IdEmpresa, [FromBody]VentaOP Venta, string DispositivoId)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.CargoComercio(IdEmpresa, Venta, DispositivoId);
        }

        #region Método de cargo a tarjeta registrada en OpenPay
        [Route("api/openpay/PostCargoId", Name = "PostCargoId")]
        public Object PostCargoId(int aIdEmpresa, [FromBody]VentaIdTarjeta aVenta, string aIdT, string aDispositivoId)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.CargoId(aIdEmpresa, aVenta, aIdT, aDispositivoId);
        }
        #endregion


        #region Cancelar pago
        [Route("api/openpay/PostCancelarPago", Name = "PostCancelarPago")]
        public Object PostCancelarPago(int IdEmpresa, string IdCargo, string Description)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.CancelarPago(IdEmpresa, IdCargo, Description);
        }
        #endregion


        #region Obtener cargo 
        [Route("api/openpay/GetConsultaCargo", Name = "GetConsultaCargo")]
        public Object GetConsultaCargo(int IdEmpresa, string IdCargo)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.ObtenerCargo(IdEmpresa, IdCargo);
        }
        #endregion

        #region Cargo sepi referencia
        [Route("api/openpay/PostSPEI", Name = "PostSPEI")]
        public Object PostSPEI(int IdEmpresa, [FromBody]ObSPEI Venta)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.SPEI(IdEmpresa, Venta);
        }
        #endregion  

        #region Cargo spei 
        [Route("api/openpay/PostCargoSPEI", Name = "PostCargoSPEI")]
        public Object PostCargoSPEI(int IdEmpresa, [FromBody]ChargeSPEI Venta)
        {
            OpenpayService servicio = new OpenpayService();
            return servicio.CargoSPEI(IdEmpresa, Venta);
        }
        #endregion  


    }
}
