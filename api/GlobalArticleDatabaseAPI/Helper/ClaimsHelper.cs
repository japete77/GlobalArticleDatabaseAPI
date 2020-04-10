using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GlobalArticleDatabaseAPI.Helpers
{
    public class ClaimsHelper
    {
        public static string TENANT_KEY = "tenant";
        public static string TENANT_DB_NAME = "dbname";
        public static string USERNAME_KEY = "username";
        public static string ROLE_KEY = "role";
        public static string ID = "id";

        // Standard JWT fields. See https://es.wikipedia.org/wiki/JSON_Web_Token for a full definition of JWT fields
        public static string ISSUED_AT = "iat";
        public static string NOT_BEFORE = "nbf";
        public static string EXPIRATION_TIME = "exp";

        IEnumerable<Claim> _claims { get; }

        public ClaimsHelper(IEnumerable<Claim> claims)
        {
            _claims = claims;
        }

        public string Tenant {
            get { return GetSingle(TENANT_KEY); }
        }

        public string DbName
        {
            get { return GetSingle(TENANT_DB_NAME); }
        }

        public string UserName
        {
            get { return GetSingle(USERNAME_KEY); }
        }

        public List<string> Roles
        {
            get { return GetMulti(ROLE_KEY); }
        }

        public DateTime IssuedAt
        {
            get { return FromUnixTimeStamp(Convert.ToDouble(GetSingle(ISSUED_AT))); }
        }

        public DateTime NotBefore
        {
            get { return FromUnixTimeStamp(Convert.ToDouble(GetSingle(NOT_BEFORE))); }
        }

        public DateTime ExpirationTime
        {
            get { return FromUnixTimeStamp(Convert.ToDouble(GetSingle(EXPIRATION_TIME))); }
        }

        private string GetSingle(string key)
        {
            return _claims.Where(w => w.Type == key)
                            .Select(s => s.Value)
                            .SingleOrDefault();
        }

        private List<string> GetMulti(string key)
        {
            return _claims.Where(w => w.Type == key)
                            .Select(s => s.Value)
                            .ToList();
        }

        private DateTime FromUnixTimeStamp(double timestamp)
        {
            var utcTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return utcTime.AddSeconds(timestamp);
        }
    }
}
