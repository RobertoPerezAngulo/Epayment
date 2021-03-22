using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wsApiEPayment.Models.Affipay;
using wsApiEPayment.Classes.Affipay;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using System.Configuration;

namespace wsApiEPayment.Services
{
    public class AffipayService
    {
        #region Constantes
        //string strServicio = "http://3.231.199.162:9020/ecommerce/";
        //string strServicio = "https://e-commerce.blumonpay.net/ecommerce/";
        static string strServicio = ConfigurationManager.AppSettings["strServicio"];


        string strMoneda = "484";
        string strPais = "MX";
        string strIp = "127.0.0.0";
        #endregion

        internal Respuesta RegistraVenta(int aIdEmpresa,Venta venta)
        {
            Respuesta respuesta;
            DVADB.DB2 dbCnx = new DVADB.DB2();
            dbCnx.AbrirConexion();
            dbCnx.BeginTransaccion();

            try
            {
                respuesta = new Respuesta();

                #region Token
                AutorizacionClass autorizacion = new AutorizacionClass();
                TokenAutorizado token = autorizacion.ObtenerToken(aIdEmpresa);
                //TokenAutorizado token = new TokenAutorizado();
                //strAuthorizationToken = strAuthorizationToken + token.access_token;
                #endregion

                #region Venta
                Sale sale = new Sale()
                {
                    amount = venta.Cantidad
                             ,
                    currency = strMoneda
                             ,
                    customerInformation = new CustomerInformation()
                    {
                        firstName = venta.InformacionCliente.Nombre
                        ,
                        lastName = venta.InformacionCliente.ApellidoPaterno
                        ,
                        middleName = venta.InformacionCliente.ApellidoMaterno
                        ,
                        email = venta.InformacionCliente.Correo
                        ,
                        phone1 = venta.InformacionCliente.Telefono
                        ,
                        city = (string.IsNullOrEmpty(venta.InformacionCliente.Ciudad) ? "" : venta.InformacionCliente.Ciudad)
                        ,
                        address1 = (string.IsNullOrEmpty(venta.InformacionCliente.Direccion) ? "" : venta.InformacionCliente.Direccion)
                        ,
                        postalCode = (string.IsNullOrEmpty(venta.InformacionCliente.CodigoPostal) ? "" : venta.InformacionCliente.CodigoPostal)
                        ,
                        state = (string.IsNullOrEmpty(venta.InformacionCliente.Estado) ? "" : venta.InformacionCliente.Estado)
                        ,
                        country = strPais
                        ,
                        ip = strIp
                    }
                             ,
                    noPresentCardData = new NoPresentCardData()
                    {
                        cardNumber = venta.DatosTarjeta.NumeroTarjeta
                        ,
                        cvv = venta.DatosTarjeta.CVV
                        ,
                        cardholderName = venta.DatosTarjeta.NombreTarjeta
                        ,
                        expirationYear = venta.DatosTarjeta.AnioExpiracion
                        ,
                        expirationMonth = venta.DatosTarjeta.MesExpiracion
                    }
                };
                
                var client = new RestClient(strServicio + "charge");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", token.token_type + " " +token.access_token);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(sale), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();
                ResponseSale responseSale = deserial.Deserialize<ResponseSale>(response);

                if (Convert.ToBoolean(responseSale.status))
                {
                    string strSqlSeg = "";
                    strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                    strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                    strSqlSeg = strSqlSeg + "VALUES (" + aIdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)"  + "," + "''" + ",'" + "" + "','" + "" + "'," + "''" + "," + "1" + ",";
                    strSqlSeg = strSqlSeg + "1" + "," + "2" + "," + "'" + responseSale.dataResponse.description + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                    strSqlSeg = strSqlSeg + ")";


                    RespuestaVenta respuestaVenta = new RespuestaVenta()
                    {
                        Estado = responseSale.status
                                                         ,
                        SolicitudId = responseSale.requestId
                                                         ,
                        Id = responseSale.id
                                                         ,
                        Fecha = responseSale.date
                                                         ,
                        Hora = responseSale.time
                                                         ,
                        DatosAprobacion = new DatosAprobacion()
                        {
                            Autorizacion = responseSale.dataResponse.authorization
                                                                                ,
                            Descripcion = responseSale.dataResponse.description
                                                                                ,
                            BinInformacion = new BinInformacion()
                            {
                                bank = responseSale.dataResponse.binInformation.bank,
                                bin = responseSale.dataResponse.binInformation.bin,
                                brand = responseSale.dataResponse.binInformation.brand,
                                product = responseSale.dataResponse.binInformation.product,
                                type = responseSale.dataResponse.binInformation.type
                            },
                            IdPagoElectronico = autorizacion.SQLQuery(ref dbCnx, strSqlSeg)
                        }
                    };

                    strSqlSeg = "";
                    //strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(respuestaVenta) + "'" + ", FSCADESCRI = '" + respuestaVenta.DatosAprobacion.Descripcion + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(sale) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(responseSale) + "' WHERE FICAIDPAGE =" + respuestaVenta.DatosAprobacion.IdPagoElectronico;
                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(respuestaVenta) + "'" + ", FSCADESCRI = '" + respuestaVenta.DatosAprobacion.Descripcion + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(sale) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(responseSale) + "' WHERE FICAIDPAGE =" + respuestaVenta.DatosAprobacion.IdPagoElectronico;
                    if (!autorizacion.SQLSaveUpdate(ref dbCnx, strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                    dbCnx.CommitTransaccion();
                    dbCnx.CerrarConexion();
                    respuesta = respuestaVenta;
                 }
                else
                {
                    ResponseSaleNoAproved responseSaleNoAproved = deserial.Deserialize<ResponseSaleNoAproved>(response);

                    string strSqlSeg = "";
                    strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                    strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                    strSqlSeg = strSqlSeg + "VALUES (" + aIdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "0" + ",";
                    strSqlSeg = strSqlSeg + "1" + "," + "2" + "," + "'" + responseSaleNoAproved.error.description + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                    strSqlSeg = strSqlSeg + ")";


                    RespuestaVentaNoAprobada respuestaNoAprobada = new RespuestaVentaNoAprobada()
                    {
                        Estado = responseSaleNoAproved.status,
                        SolicitudId = responseSaleNoAproved.requestId,
                        Fecha = responseSaleNoAproved.date,
                        Hora = responseSaleNoAproved.time,
                        error = new ErrorNoAprobado() { httpStatusCode = responseSaleNoAproved.error.httpStatusCode, code = responseSaleNoAproved.error.code, description = responseSaleNoAproved.error.description , IdPagoElectronico = autorizacion.SQLQuery(ref dbCnx, strSqlSeg) }
                    };

                    strSqlSeg = "";

                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(respuestaNoAprobada) + "'" + ", FSCADESCRI = '" + respuestaNoAprobada.error.description + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(sale) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(responseSaleNoAproved) + "' WHERE FICAIDPAGE =" + respuestaNoAprobada.error.IdPagoElectronico;
                    if (!autorizacion.SQLSaveUpdate(ref dbCnx,strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                    dbCnx.CommitTransaccion();
                    dbCnx.CerrarConexion();
                    respuesta = respuestaNoAprobada;
                }
                #endregion

                //respuesta = respuestaError;
            }
            catch(Exception )
            {
                respuesta = new RespuestaVenta();
            }


            return respuesta;
        }

        internal RespuestaCancelacion CancelacionVenta(int aIdEmpresa,CancelarVenta cancelarVenta,string aIdCrago)
        {
            DVADB.DB2 dbCnx = new DVADB.DB2();
            dbCnx.AbrirConexion();
            dbCnx.BeginTransaccion();
            RespuestaCancelacion respuesta;
            ResponseCancel respuestaCancel;
            AutorizacionClass autorizacion = new AutorizacionClass();
            CancelSale sale = new CancelSale();

            try
            {
                respuestaCancel = new ResponseCancel();

                #region Token
                
                TokenAutorizado token = autorizacion.ObtenerToken(aIdEmpresa);
                //TokenAutorizado token = new TokenAutorizado();
                //strAuthorizationToken = strAuthorizationToken + token.access_token;
                #endregion

                sale = new CancelSale()
                {
                    amount = cancelarVenta.Cantidad
                             ,
                    noPresentCardData = new NoPresentCardData()
                    {
                        cardNumber = cancelarVenta.DatosTarjeta.NumeroTarjeta
                        ,
                        cvv = cancelarVenta.DatosTarjeta.CVV
                        ,
                        cardholderName = cancelarVenta.DatosTarjeta.NombreTarjeta
                        ,
                        expirationYear = cancelarVenta.DatosTarjeta.AnioExpiracion
                        ,
                        expirationMonth = cancelarVenta.DatosTarjeta.MesExpiracion
                    }
                };

                var client = new RestClient(strServicio + "cancel/" + aIdCrago);
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", token.token_type + " " + token.access_token);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(sale), ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                respuestaCancel = JsonConvert.DeserializeObject<ResponseCancel>(response.Content);

                if(!Convert.ToBoolean(respuestaCancel.status))
                    throw new System.ArgumentException();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + aIdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + ",'" + "" + "','" + "" + "'," + "''" + "," + "''" + "," + "1" + ",";
                strSqlSeg = strSqlSeg + "3" + "," + "2" + "," + "'" + $"CANCELACION DE ID CARGO {aIdCrago} AFFIPAY" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                respuesta = new RespuestaCancelacion()
                {
                    Estado = respuestaCancel.status,
                    Id = respuestaCancel.requestId,
                    Fecha = respuestaCancel.Date,
                    Hora = respuestaCancel.Time,
                    respuestaAffipay = new DataRespose
                    {
                        Autorizacion = respuestaCancel.dataResponse.Authorization,
                        Descripcion = respuestaCancel.dataResponse.Description,
                        Informacion = respuestaCancel.dataResponse.binInformation
                    },
                    IdPagoElectronico = Convert.ToString(autorizacion.SQLQuery(ref dbCnx, strSqlSeg))
                };

                strSqlSeg = "";
                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(cancelarVenta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(respuesta) + "'" + ",FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(sale) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(respuestaCancel) + "' WHERE FICAIDPAGE =" + respuesta.IdPagoElectronico;
                if (!autorizacion.SQLSaveUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException();

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
            }
            catch(Exception )
            {
                string strSqlSeg = "";
                strSqlSeg = "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + aIdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + ",'" + Newtonsoft.Json.JsonConvert.SerializeObject(cancelarVenta) + "','" + Newtonsoft.Json.JsonConvert.SerializeObject(sale) + "'," + "''" + "," + "''" + "," + "0" + ",";
                strSqlSeg = strSqlSeg + "3" + "," + "2" + "," + "'" + $"ERROR DE CANCELACION DE ID CARGO {aIdCrago} AFFIPAY" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                autorizacion.SQLSaveUpdate(ref dbCnx, strSqlSeg);
                respuesta = null;

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
            }

            return respuesta;
        }



    }
}