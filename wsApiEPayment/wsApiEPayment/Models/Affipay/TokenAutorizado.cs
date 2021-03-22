using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsApiEPayment.Models.Affipay
{
    public class TokenAutorizado
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public string expires_in { get; set; }
        public string scopeuser_entity { get; set; }
        public string user_id { get; set; }
        public string user_system { get; set; }
        public string business_id { get; set; }
        public string jti { get; set; }

        public string Fecha { get; set; }
    }
}