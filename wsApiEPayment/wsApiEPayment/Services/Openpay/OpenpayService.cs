using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wsApiEPayment.Models.Openpay;
using wsApiEPayment.Classes;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Openpay;
using Openpay.Entities;
using Openpay.Entities.Request;
using wsApiEPayment.Models;
using System.Configuration;

namespace wsApiEPayment.Services.Openpay
{

    public class OpenpayService : ResponseOpError 
    {
        static private string CadenaEncry = ConfigurationManager.AppSettings["MetodoEncry"];
        static private bool Production = Convert.ToBoolean(ConfigurationManager.AppSettings["Production"].ToString());
        static private SQLmetodos Obj = new SQLmetodos();
        internal Object CargoComercio(int IdEmpresa, VentaOP venta, string DispositivoId)
        {
            ChargeRequest request = new ChargeRequest();
            #region Método de cargo por via banco referencia por cuenta comercio          
            try
            {

                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                openpayAPI.Production = Production;
                Card tarjeta = new Card();
                tarjeta.HolderName = venta.DatosTarjeta.NombreTarjeta;
                tarjeta.CardNumber = venta.DatosTarjeta.NumeroTarjeta;
                tarjeta.Cvv2 = venta.DatosTarjeta.CVV;
                tarjeta.ExpirationMonth = venta.DatosTarjeta.MesExpiracion;
                tarjeta.ExpirationYear = venta.DatosTarjeta.AnioExpiracion;
                tarjeta.DeviceSessionId = DispositivoId;

                Customer customer = new Customer();
                customer.Name = venta.InformacionCliente.Nombre;
                customer.LastName = venta.InformacionCliente.ApellidoPaterno;
                customer.PhoneNumber = venta.InformacionCliente.Telefono;
                customer.Email = venta.InformacionCliente.Correo;
                request.Amount = Math.Round(Convert.ToDecimal(venta.Cantidad),2);
                request.Currency = "MXN";
                request.Description = venta.Descripction;
                request.OrderId = "";
                request.DeviceSessionId = DispositivoId;
                request.Customer = customer;
                request.Method = "card";

                Card card = openpayAPI.CardService.Create(tarjeta);
                request.SourceId = card.Id;

                Charge charge = openpayAPI.ChargeService.Create(request);


                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "1" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                RespuestaCargo cargo = new RespuestaCargo()
                {
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                    Cantidad = charge.Amount.ToString(),
                    Estatus = charge.Status,
                    Descripcion = charge.Description,
                    TipoTransacion = charge.TransactionType,
                    TipoOperacion = charge.OperationType,
                    Metodo = charge.Method,
                    MensajeError = charge.ErrorMessage,
                    Tarjeta = new RespuestaTarjeta()
                    {
                        Fecha = charge.Card.CreationDate.ToString(),
                        Banco = charge.Card.BankName,
                        PagoAprobado = charge.Card.AllowsPayouts.ToString(),
                        TitularTarjeta = charge.Card.HolderName,
                        Mes = charge.Card.ExpirationMonth,
                        Anio = charge.Card.ExpirationYear,
                        Tarjeta = charge.Card.CardNumber,
                        Marca = charge.Card.Brand,
                        CargoAprobado = charge.Card.AllowsCharges.ToString(),
                        NumeroBanco = charge.Card.BankCode,
                        TipoTarjeta = charge.Card.Type,
                        Id = charge.Card.TokenId == null ? charge.Card.Id : charge.Card.TokenId
                    },
                    Autorizacion = charge.Authorization,
                    IdOrden = charge.OrderId,
                    IdCargo = charge.Id,
                    IdPagoElectronico = Convert.ToString(Obj.SQLQuery(ref dbCnx, strSqlSeg)),
                };
                
                if (Convert.ToUInt64(cargo.IdPagoElectronico) != 0) {
                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET "+ "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," +"FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(cargo) + "'" + ", FSCADESCRI = '" + cargo.Descripcion + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" +"," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(charge) + "' WHERE FICAIDPAGE =" + cargo.IdPagoElectronico;
                    if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");
                }
                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return cargo;
            }
            catch (OpenpayException ex)
            {
                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg +"INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "0" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ErrorOpenPay error = new ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    },
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg),
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                };

                error.description = Obj.DepuracioneErrorresTarjeta(error.error_code);

                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(error) + "'" + ", FSCADESCRI = '" + error.description + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(ex) + "' WHERE FICAIDPAGE =" + error.IdPagoElectronico;
                if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return error;
            }
            catch (Exception ex)
            {
                ErrorService error = new ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Cargo id tarjeta"
                };
                return error;
            }
            #endregion
        }
        
        internal Object CargoId(int IdEmpresa, VentaIdTarjeta venta, string aIdTarjeta, string DispositivoId)
        {
            ChargeRequest request = new ChargeRequest();
            #region Método de cargo a tarjeta registrada en OpenPay
            try
            {
                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada), Obj.Decrypt(CadenaEncry, _x.Openpay.IdComercioOpenPay));
                openpayAPI.Production = Production;
                Customer customer = new Customer();
                customer.Name = venta.InformacionCliente.Nombre;
                customer.LastName = venta.InformacionCliente.ApellidoPaterno;
                customer.PhoneNumber = venta.InformacionCliente.Telefono;
                customer.Email = venta.InformacionCliente.Correo;
                request.Method = "card";
                request.SourceId = aIdTarjeta;
                request.Amount = Math.Round(Convert.ToDecimal(venta.Cantidad),2);
                request.Currency = "MXN";
                request.Description = venta.Descripction;
                request.OrderId = "";
                request.DeviceSessionId = DispositivoId;
                request.Customer = customer;

                Charge charge = openpayAPI.ChargeService.Create(request);

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "1" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                RespuestaCargo cargo = new RespuestaCargo()
                {
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                    Cantidad = charge.Amount.ToString(),
                    Estatus = charge.Status,
                    Descripcion = charge.Description,
                    TipoTransacion = charge.TransactionType,
                    TipoOperacion = charge.OperationType,
                    Metodo = charge.Method,
                    MensajeError = charge.ErrorMessage,
                    Tarjeta = new RespuestaTarjeta()
                    {
                        Fecha = charge.Card.CreationDate.ToString(),
                        Banco = charge.Card.BankName,
                        PagoAprobado = charge.Card.AllowsPayouts.ToString(),
                        TitularTarjeta = charge.Card.HolderName,
                        Mes = charge.Card.ExpirationMonth,
                        Anio = charge.Card.ExpirationYear,
                        Tarjeta = charge.Card.CardNumber,
                        Marca = charge.Card.Brand,
                        CargoAprobado = charge.Card.AllowsCharges.ToString(),
                        NumeroBanco = charge.Card.BankCode,
                        TipoTarjeta = charge.Card.Type,
                        Id = charge.Card.TokenId == null ? charge.Card.Id : charge.Card.TokenId
                    },
                    Autorizacion = charge.Authorization,
                    IdOrden = charge.OrderId,
                    IdCargo = charge.Id,
                    IdPagoElectronico = Convert.ToString(Obj.SQLQuery(ref dbCnx, strSqlSeg)),
                };

                if (Convert.ToUInt64(cargo.IdPagoElectronico) != 0)
                {
                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(cargo) + "'" + ", FSCADESCRI = '" + cargo.Descripcion + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(charge) + "' WHERE FICAIDPAGE =" + cargo.IdPagoElectronico;
                    if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");
                }
                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return cargo;
            }
            catch (OpenpayException ex)
            {
                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "0" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ErrorOpenPay error = new ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    },
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg),
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                };

                error.description = Obj.DepuracioneErrorresTarjeta(error.error_code);


                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(error) + "'" + ", FSCADESCRI = '" + error.description + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(ex) + "' WHERE FICAIDPAGE =" + error.IdPagoElectronico;
                if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return error;
            }
            catch (Exception ex)
            {
                ErrorService error = new ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Cargo id tarjeta"
                };
                return error;
            }
            #endregion
        }

        internal Object SPEI(int IdEmpresa, ObSPEI venta)
        {
            ChargeRequest request = new ChargeRequest();
            #region Cargos de SPIE Openpay
            try
            {
                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada), Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                request.Method = "bank_account";
                request.Amount = Math.Round(Convert.ToDecimal(venta.Catidad),2);
                request.Description = venta.Descripcion;
                request.OrderId = "";

                Customer customer = new Customer();
                customer.Name = venta.Nombre;
                customer.LastName = venta.Apellido;
                customer.PhoneNumber = venta.Telefono;
                customer.Email = venta.correo;
                request.Customer = customer;
                
                Charge charge = openpayAPI.ChargeService.Create(request);

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "1" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ObSPEINegocio _ventanegocio = new ObSPEINegocio()
                {
                    Fecha = DateTime.Today.ToString("dd-MM-yyyy"),
                    Hora = DateTime.Now.ToString("HH:mm:ss"),
                    Catidad = charge.Amount,
                    Descripcion = charge.Description,
                    Status = charge.Status,
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg),
                    IdCargo = charge.Id,
                    Metodo = charge.Method,
                    Banco = new MetodoPago()
                    {
                        NombreBanco = charge.PaymentMethod.BankName,
                        Nombre = charge.PaymentMethod.Name,
                        Clave = charge.PaymentMethod.CLABE,
                        Convenio = charge.PaymentMethod.Reference,
                        TipoTransaccion = charge.PaymentMethod.Type
                    }
                };

                if (_ventanegocio.IdPagoElectronico != 0)
                {
                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(_ventanegocio) + "'" + ", FSCADESCRI = '" + charge.Description + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(charge) + "' WHERE FICAIDPAGE =" + _ventanegocio.IdPagoElectronico;
                    if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");
                }
                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();

                return _ventanegocio;

            }
            catch (OpenpayException ex)
            {
                DVADB.DB2 dbCnx = new DVADB.DB2();
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + "," + "''" + "," + "''" + ",'" + "" + "'," + "''" + "," + "0" + ",";
                strSqlSeg = strSqlSeg + "1" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ErrorOpenPay error = new ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    },
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg),
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                };

                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET " + "FSCASOJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(venta) + "'," + "FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(error) + "'" + ", FSCADESCRI = '" + error.description + "'" + "," + "FSCASPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "'" + "," + "FSCARPJSON = '" + Newtonsoft.Json.JsonConvert.SerializeObject(ex) + "' WHERE FICAIDPAGE =" + error.IdPagoElectronico;
                if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return error;
            }
            catch (Exception ex)
            {
                ErrorService error = new ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Cargo SPEI"
                };
                return error;
            }
            #endregion
        }

       internal Object CargoSPEI(int IdEmpresa, ChargeSPEI venta)
       {
            #region CargoSPEI
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada), Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                PayoutRequest request = new PayoutRequest();
                request.Method = "bank_account";
                BankAccount bankAccount = new BankAccount();
                bankAccount.HolderName = venta.Titular;
                bankAccount.CLABE = venta.Clave;
                request.BankAccount = bankAccount;
                request.Amount = Math.Round(Convert.ToDecimal(venta.Cantidad),2);
                request.Description = venta.Descripcion;

                Payout _cargo = openpayAPI.PayoutService.Create(request);

                return _cargo;
            }
            catch (OpenpayException ex)
            {
                ErrorOpenPay error = new ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    },
                    IdPagoElectronico = 0,
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                };
                return error;
            }
            #endregion
       }

        internal Object CancelarPago(int IdEmpresa, string aIdCargo, string Description)
        {
            DVADB.DB2 dbCnx = new DVADB.DB2();
            #region Cancelar pago por comercio
            try
            {

                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                openpayAPI.Production = Production;
                Charge charge = openpayAPI.ChargeService.Refund(aIdCargo, Description);
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();
                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg +"INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + ",'" + "" + "','" + "" + "','" + "" +"','" + "" + "',"+ "1" + ",";
                strSqlSeg = strSqlSeg + "3" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                RespuestaCargo cargo = new RespuestaCargo()
                {
                    FechaCreacion = DateTime.Today.ToString("dd-MM-yyyy"),
                    HoraCreacion = DateTime.Now.ToString("HH:mm:ss"),
                    Cantidad = charge.Amount.ToString(),
                    Estatus = charge.Status,
                    Descripcion = charge.Description,
                    TipoTransacion = charge.TransactionType,
                    TipoOperacion = charge.OperationType,
                    Metodo = charge.Method,
                    MensajeError = charge.ErrorMessage,
                    Tarjeta = new RespuestaTarjeta()
                    {
                        Fecha = charge.Card.CreationDate.ToString(),
                        Banco = charge.Card.BankName,
                        PagoAprobado = charge.Card.AllowsPayouts.ToString(),
                        TitularTarjeta = charge.Card.HolderName,
                        Mes = charge.Card.ExpirationMonth,
                        Anio = charge.Card.ExpirationYear,
                        Tarjeta = charge.Card.CardNumber,
                        Marca = charge.Card.Brand,
                        CargoAprobado = charge.Card.AllowsCharges.ToString(),
                        NumeroBanco = charge.Card.BankCode,
                        TipoTarjeta = charge.Card.Type,
                        Id = charge.Card.TokenId == null ? charge.Card.Id : charge.Card.TokenId
                    },
                    CargoCancelado =  new CancelarPago()
                    {
                        id = charge.Refund.Id,
                        Cantidad = charge.Refund.Amount.ToString(),
                        Autorizacion = charge.Refund.Authorization,
                        Metodo = charge.Refund.Method,
                        TipoOperacion = charge.Refund.OperationType,
                        TipoTransacion = charge.Refund.TransactionType,
                        Estatus = charge.Refund.Status,
                        Descripcion = Description
                    },
                    Autorizacion = charge.Authorization,
                    IdOrden = charge.OrderId,
                    IdCargo = charge.Id,
                    IdPagoElectronico = Convert.ToString(Obj.SQLQuery(ref dbCnx, strSqlSeg))
                };


                if (Convert.ToInt64(cargo.IdPagoElectronico) != 0)
                {
                    strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(cargo) + "', FSCADESCRI = '" + cargo.Descripcion + "' WHERE FICAIDPAGE =" + cargo.IdPagoElectronico;
                    if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                        throw new System.ArgumentException("Hubo error al intentar guardar los datos");
                }

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();

                return cargo;
            }
            catch (OpenpayException ex)
            {

                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + ",'" + "" + "','" + "" + "','" + "" + "','" + "" + "'," + "0" + ",";
                strSqlSeg = strSqlSeg + "2" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ErrorOpenPay error = new ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    },
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg)
                };

                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(error) + "', FSCADESCRI = '" + error.description + "' WHERE FICAIDPAGE =" + error.IdPagoElectronico;
                if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return error;
            }
            catch (Exception ex)
            {
                dbCnx.AbrirConexion();
                dbCnx.BeginTransaccion();

                string strSqlSeg = "";
                strSqlSeg = "SELECT FICAIDPAGE FROM NEW TABLE (";
                strSqlSeg = strSqlSeg + "INSERT INTO  PRODCAJA.CAEPAELE ";
                strSqlSeg = strSqlSeg + "VALUES (" + IdEmpresa + "," + "(SELECT coalesce(MAX(FICAIDPAGE),0)+1 ID FROM PRODCAJA.CAEPAELE)" + ",'" + "" + "','" + "" + "','" + "" + "','"+ "" + "'," + "0" + ",";
                strSqlSeg = strSqlSeg + "2" + "," + "1" + "," + "'" + "" + "'" + "," + "1" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'APPS'" + "," + "''" + "," + "CURRENT_DATE" + "," + "CURRENT_TIME" + "," + "'0')";
                strSqlSeg = strSqlSeg + ")";

                ErrorService error = new ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Cargo id tarjeta",
                    IdPagoElectronico = Obj.SQLQuery(ref dbCnx, strSqlSeg)
                };

                strSqlSeg = "UPDATE PRODCAJA.CAEPAELE SET FSCAREJSON ='" + Newtonsoft.Json.JsonConvert.SerializeObject(error) + "', FSCADESCRI = '" + error.description + "' WHERE FICAIDPAGE =" + error.IdPagoElectronico;
                if (!Obj.SQLSaveOrUpdate(ref dbCnx, strSqlSeg))
                    throw new System.ArgumentException("Hubo error al intentar guardar los datos");

                dbCnx.CommitTransaccion();
                dbCnx.CerrarConexion();
                return error;
            }
            #endregion           
        }
        
        internal Object ObtenerTarjetaComercio(int IdEmpresa, string aIdCard)
        {
            #region Método de obtener tarjeta comercio 
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry, _x.Openpay.IdComercioOpenPay));
                Card card = openpayAPI.CardService.Get(aIdCard);
                return card;
            }
            catch (OpenpayException ex)
            {

                 ErrorOpenPay error = new  ErrorOpenPay()
                { Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new  fraud_rules() {
                        Fraude = ex.StatusCode.ToString()
                    }
                };
                return error;
            }
            catch (Exception ex) {

                 ErrorService error = new  ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Obtener T"
                };
                return error;
            }
            #endregion
        }

        internal Object ObtenerTarjetaCliente(int IdEmpresa, string aIdCliente, string aIdCard)
        {
            #region Método de obtener tarjeta cliente
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry, _x.Openpay.IdComercioOpenPay));
                Card card = openpayAPI.CardService.Get(aIdCliente, aIdCard);
                return card;
            }
            catch (OpenpayException ex)
            {

                 ErrorOpenPay error = new  ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new  fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    }
                };
                return error;
            }
            catch (Exception ex)
            {

                 ErrorService error = new  ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Obtener TC"
                };
                return error;
            }
            #endregion
        }

        internal Object EliminarTarjeta(int IdEmpresa, string aIdTarejetaRegistrada) {
            #region Eliminar tarjeta registrada
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                openpayAPI.CardService.Delete(aIdTarejetaRegistrada);
                 ErrorService mensaje = new  ErrorService()
                {
                    description = "Tarje eliminada con éxito",
                    Fecha = DateTime.Now,
                    Metodo = "Eliminar tarjeta"
                };
                return mensaje;
            }
            catch (OpenpayException ex)
            {

                 ErrorOpenPay error = new  ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new  fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    }
                };
                return error;
            }
            catch (Exception ex)
            {

                 ErrorService error = new  ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Eliminar T"
                };
                return error;
            }
            #endregion
        }

        internal Object EliminarTarjetaCliente(int IdEmpresa, string aIdCliente,string aIdTarejetaRegistrada)
        {
            #region Eliminar tarjeta cliente
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                openpayAPI.CardService.Delete(aIdCliente, aIdTarejetaRegistrada);
                 ErrorService mensaje = new  ErrorService()
                {
                    description = "Tarje eliminada con éxito",
                    Fecha = DateTime.Now,
                    Metodo = "Eliminar tarjeta"
                };
                return mensaje;
            }
            catch (OpenpayException ex)
            {

                 ErrorOpenPay error = new  ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new  fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    }
                };
                return error;
            }
            catch (Exception ex)
            {

                 ErrorService error = new  ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "Eliminar TC"
                };
                return error;
            }
            #endregion
        }

        internal Object ObtenerCargo(int IdEmpresa, string aIdCargo)
        {
            #region Obtener cargo
            try
            {
                Models.LlavesServicios _x = Llave(IdEmpresa);
                OpenpayAPI openpayAPI = new OpenpayAPI(Obj.Decrypt(CadenaEncry, _x.Openpay.LlaveOPenPayPrivada),Obj.Decrypt(CadenaEncry,_x.Openpay.IdComercioOpenPay));
                Charge charge = openpayAPI.ChargeService.Get(aIdCargo);
                return charge;
            }
            catch (OpenpayException ex)
            {

                 ErrorOpenPay error = new  ErrorOpenPay()
                {
                    Categoria = ex.Category,
                    description = ex.Description,
                    http_code = ex.StatusCode.ToString(),
                    error_code = ex.ErrorCode,
                    request_id = ex.RequestId,
                    fraude = new  fraud_rules()
                    {
                        Fraude = ex.StatusCode.ToString()
                    }
                };
                return error;
            }
            catch (Exception ex)
            {

                 ErrorService error = new  ErrorService()
                {
                    description = ex.Message,
                    Fecha = DateTime.Now,
                    Metodo = "ObtenerCargo"
                 };
                return error;
            }
            #endregion
        }


    }
}