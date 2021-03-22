using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class Venta
    {
        public string Cantidad { get; set; }
        //public string Moneda { get; set; }
        public InformacionCliente InformacionCliente { get; set; }
        public DatosTarjeta DatosTarjeta { get; set; }
    }

    public class InformacionCliente
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string CodigoPostal { get; set; }
        public string Estado { get; set; }
        //public string Pais { get; set; }
        //public string ip { get; set; }
    }

    public class DatosTarjeta
    {
        public string NumeroTarjeta { get; set; }
        public string CVV { get; set; }
        public string NombreTarjeta { get; set; }
        public string AnioExpiracion { get; set; }
        public string MesExpiracion { get; set; }
    }
}