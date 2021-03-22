using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace wsApiEPayment.Models
{
    public class SQLmetodos
    {
      
        public bool SQLSaveOrUpdate(ref DVADB.DB2 conexion, string Sentencia)
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
        }


        public string DepuracioneErrorresTarjeta(int error)
        {
            string MensajeError = "";
            try
            {
                switch (error)
                {
                    case 2004:
                        MensajeError = "El número de tarjeta es invalido.";
                        break;
                    case 2005:
                        MensajeError = "La fecha de expiración de la tarjeta es anterior a la fecha actual.";
                        break;
                    case 2006:
                        MensajeError = "El código de seguridad de la tarjeta (CVV2) no fue proporcionado.";
                        break;
                    case 2011:
                        MensajeError = "Tipo de tarjeta no soportada.";
                        break;
                    case 3002:
                        MensajeError = "La tarjeta ha expirado.";
                        break;
                    case 3006:
                        MensajeError = "La operación no esta permitida para este cliente o esta transacción.";
                        break;
                    default:
                        MensajeError = "La tarjeta fue declinada.";
                        break;
                }
                return MensajeError;
            }
            catch (Exception)
            {
                return "La tarjeta fue declinada.";
            }
        }


        public  string Encrypt(string password, string plainText)
        {
            if (plainText == null)
            {
                return null;
            }
            var bytesToBeEncrypted = Encoding.UTF8.GetBytes(plainText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA512.Create().ComputeHash(passwordBytes);
            var bytesEncrypted = Encrypt(bytesToBeEncrypted, passwordBytes);
            return Convert.ToBase64String(bytesEncrypted);
        }

        private static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] passwordBytes)
        {
            byte[] encryptedBytes = null;
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);
                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;
                    using (var cs = new CryptoStream(ms, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                        cs.Close();
                    }
                    encryptedBytes = ms.ToArray();
                }
            }
            return encryptedBytes;
        }

        public string Decrypt(string password, string encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }
            // Get the bytes of the string
            var bytesToBeDecrypted = Convert.FromBase64String(encryptedText);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            passwordBytes = SHA512.Create().ComputeHash(passwordBytes);

            var bytesDecrypted = Decrypt(bytesToBeDecrypted, passwordBytes);

            return Encoding.UTF8.GetString(bytesDecrypted);
        }
        private static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] passwordBytes)
        {
            byte[] decryptedBytes = null;
            var saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            using (MemoryStream ms = new MemoryStream())
            {
                using (RijndaelManaged AES = new RijndaelManaged())
                {
                    var key = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 1000);

                    AES.KeySize = 256;
                    AES.BlockSize = 128;
                    AES.Key = key.GetBytes(AES.KeySize / 8);
                    AES.IV = key.GetBytes(AES.BlockSize / 8);
                    AES.Mode = CipherMode.CBC;

                    using (var cs = new CryptoStream(ms, AES.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                        cs.Close();
                    }

                    decryptedBytes = ms.ToArray();
                }
            }

            return decryptedBytes;
        }
    }
}