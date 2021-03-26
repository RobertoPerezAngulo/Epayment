using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wsApiEPayment.Models.Affipay;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Configuration;

namespace wsApiEPayment.Classes.Affipay
{
    public class AutorizacionClass
    {
        #region Constantes
        static string strServicioToken = ConfigurationManager.AppSettings["strServicioToken"];
        //string strServicioToken = "https://tokener.blumonpay.net/oauth/token";
        //string strServicioToken = "http://3.231.199.162:9000/oauth/token";
        string strAuthorization = "Basic Ymx1bW9uX3BheV9lY29tbWVyY2VfYXBpOmJsdW1vbl9wYXlfZWNvbW1lcmNlX2FwaV9wYXNzd29yZA==";
        string strTipo = "password";
        static private bool Production = Convert.ToBoolean(ConfigurationManager.AppSettings["Production"].ToString());

        #endregion

        #region Variables
        private string strRutaArchivo = @"c:\inetpub\wwwroot\wsApiEPayment\ResourcesJson\";
        #endregion

        public AutorizacionClass()
        {
            
        }

        public TokenAutorizado ObtenerToken(int aIdEmpresa)
        {
            TokenAutorizado token = new TokenAutorizado();
            string nombreArchivo = $"TokenAffiPay{aIdEmpresa}.json";

            try
            {
                string archivoJson = Path.GetFullPath(strRutaArchivo + nombreArchivo);
                if (File.Exists(archivoJson))
                {
                    string strJSON = File.ReadAllText(strRutaArchivo + nombreArchivo);
                    token = JsonConvert.DeserializeObject<TokenAutorizado>(strJSON);
                }

                if (Convert.ToDateTime(token.Fecha).AddHours(3) >= DateTime.Now)
                {

                }
                else
                {
                    token = generarToken(aIdEmpresa);
                    CrearArchivoJson(nombreArchivo, token);
                }

            }
            catch (Exception ex)
            {

            }

            return token;
        }

                     
        public bool SQLSaveUpdate(ref DVADB.DB2 conexion ,string Sentencia)
        {
            DVADB.DB2 dbCnx = conexion;
            try
            {
                dbCnx.SetQuery(Sentencia);
                return true;
            }
            catch (Exception)
            {
                dbCnx.RollbackTransaccion();
                dbCnx.CerrarConexion();
                return false;
            }

            //try
            //{
            //    DVADB.DB2 dbCnx = new DVADB.DB2();
            //    dbCnx.AbrirConexion();
            //    dbCnx.BeginTransaccion();
            //    dbCnx.SetQuery(Sentencia);
            //    dbCnx.CommitTransaccion();
            //    dbCnx.CerrarConexion();
            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}
        }

        public long SQLQuery(ref DVADB.DB2 conexion, string sentencia)
        {
            DVADB.DB2 dbCnx = conexion;
            try
            {
                string strSql = "";
                strSql = sentencia;
                return Convert.ToInt32(dbCnx.GetDataSet(strSql).Tables[0].Rows[0]["FICAIDPAGE"].ToString());
            }
            catch (Exception)
            {
                dbCnx.RollbackTransaccion();
                dbCnx.CerrarConexion();
                return 0;
            }


            //int a = 0;
            //try
            //{
            //    DVADB.DB2 dbCnx = new DVADB.DB2();
            //    string strSql = "";
            //    strSql = "SELECT coalesce(MAX(FICAIDPAGE),0) ID FROM PRODCAJA.CAEPAELE";
            //    a = Convert.ToInt32(dbCnx.GetDataSet(strSql).Tables[0].Rows[0]["ID"].ToString());
            //    return a;
            //}
            //catch (Exception)
            //{
            //    return a;
            //}
        }


        private TokenAutorizado generarToken(int aIdEmpresa)
        {
            TokenAutorizado token = new TokenAutorizado();
            Models.LlavesServicios obj = Llave(aIdEmpresa);
            try
            {
                var client = new RestClient(strServicioToken);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", strAuthorization);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("grant_type", strTipo);
                request.AddParameter("username", obj.AffiPay.UserAffiPay);
                request.AddParameter("password", obj.AffiPay.PaswordEcommerce);
                IRestResponse response = client.Execute(request);
                RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
                token = deserial.Deserialize<TokenAutorizado>(response);
                token.Fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception)
            {

            }

            return token;
        }

        private void CrearArchivoJson<T>(string nombreArchivo, T objeto)
        {
            string archivoJson = Path.GetFullPath(strRutaArchivo + nombreArchivo);
            if (File.Exists(archivoJson))
            {
                File.Delete(archivoJson);
            }
            using (StreamWriter sw = File.CreateText(archivoJson))
            {
                sw.WriteLine(JsonConvert.SerializeObject(objeto));
            }
        }

        private string obtenerURLServidor()
        {
            HttpRequest request = HttpContext.Current.Request;
            string baseUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') + "/";

            return baseUrl;
        }

        private string leerArchivoWeb(string url)
        {
            Uri uri = new Uri(url);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string output = reader.ReadToEnd();
            response.Close();

            return output;
        }

        private Models.LlavesServicios Llave(int key)
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
            _consulta.AffiPay.PaswordEcommerce = GetSHA256(_consulta.AffiPay.PaswordEcommerce);
            return _consulta;
        }


        private string GetSHA256(string str)
        {
            System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256Managed.Create();
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] stream = null;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString().ToUpper();
        }

        

    }
}