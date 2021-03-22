using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Openpay
{
    public class VentaOP
    {
        public string Cantidad { get; set; }

        public string Descripction { get; set; }

        public string OrderId { get; set; }
        //public string Moneda { get; set; }
        public InformacionClienteOP InformacionCliente { get; set; }
        public DatosTarjetaOP DatosTarjeta { get; set; }

    }

    public class InformacionClienteOP
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
    }

    public class DatosTarjetaOP
    {
        public string NumeroTarjeta { get; set; }
        public string CVV { get; set; }
        public string NombreTarjeta { get; set; }
        public string AnioExpiracion { get; set; }
        public string MesExpiracion { get; set; }
    }
}