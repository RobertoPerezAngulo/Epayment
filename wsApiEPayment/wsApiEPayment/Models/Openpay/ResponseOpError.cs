using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Openpay
{
    public class StatusResponse
    {
        public string OK { get; set; }
        public string Mensaje { get; set; }
        public ObjectPerson Customer { get; set; }
    }
    class ErrorService
    {
        public string description { get; set; }
        public DateTime Fecha { get; set; }
        public string Metodo { get; set; }
        public long IdPagoElectronico { get; set; }
    }

    class ErrorOpenPay
    {
        public string Categoria { get; set; }
        public string description { get; set; }
        public string http_code { get; set; }
        public int error_code { get; set; }
        public string request_id { get; set; }
        public long IdPagoElectronico { get; set; }
        public fraud_rules fraude { get; set; }
        public string FechaCreacion { get; set; }
        public string HoraCreacion { get; set; }
    }

    class fraud_rules
    {
        public string Fraude { get; set; }
    }

    public class ObjectPerson
    {
        public decimal Balance { get; set; }
        public string CLABE { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Email { get; set; }
        public string ExternalId { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public bool RequiresAccount { get; set; }
        public string Status { get; set; }
        public string Store { get; set; }
    }

    public class VentaIdTarjeta
    {
        public string Cantidad { get; set; }

        public string Descripction { get; set; }

        public string OrderId { get; set; }
        public InformacionClienteSession InformacionCliente { get; set; }
    }

    public class InformacionClienteSession
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
    }

    public class ResponseOpError
    {
        static private bool Production = Convert.ToBoolean(ConfigurationManager.AppSettings["Production"].ToString());
        #region Key oranitation
        public Models.LlavesServicios Llave(int key)
        {
            string NombreArchivoJson = string.Empty;
            if (Production == true)
            {
               NombreArchivoJson = "PSConfig.json";
            }
            else
            {
                NombreArchivoJson = "PSConfig_PruebasCargo.json";
            }
            string RutaArchivoJson = @"C:\inetpub\wwwroot\wsApiEPayment\ResourcesJson\";
            string jsonString = System.IO.File.ReadAllText(RutaArchivoJson + NombreArchivoJson);
            List<Models.LlavesServicios> model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.LlavesServicios>>(jsonString);
            Models.LlavesServicios _consulta = model.Find(x => x.IdEmpresa == key);

            return _consulta;
        }
        #endregion




    }





}