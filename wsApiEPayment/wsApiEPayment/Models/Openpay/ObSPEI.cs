using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Openpay
{
    public class ObSPEI
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Telefono { get; set; }
        public string correo { get; set; }
        public string Catidad { get; set; }
        public string Descripcion { get; set; }
    }

    public class ChargeSPEI
    {
        public string Titular { get; set; }
        public string Clave { get; set; }
        public string Cantidad { get; set; }
        public string Descripcion { get; set; }
    }

    public class ObSPEINegocio
    {
        public string IdCargo { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public decimal Catidad { get; set; }
        public string Descripcion { get; set; }
        public string Status { get; set; }
        public string Metodo { get; set; }
        public long IdPagoElectronico { get; set; }
        public MetodoPago Banco { get; set; }
    }

    public class MetodoPago
    {
        public string NombreBanco { get; set; }
        public string Nombre { get; set; }
        public string ReferenciaBancaria { get; set; }
        public string TipoTransaccion { get; set; }
        public string Convenio { get; set; }
        public string Clave { get; set; }
    }
}