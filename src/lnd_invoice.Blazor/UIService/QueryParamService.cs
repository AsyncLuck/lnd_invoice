using lnd_invoice.Service;
using System.Security.Cryptography;
using System.Text;

namespace lnd_invoice.Blazor.UIService
{
    public class QueryParamService
    {
        private readonly string _blazorSecurityKey = "Test";

        public QueryParamService(LndConnectionSettings settings)
        {
            _blazorSecurityKey = settings.BlazorSecurityKey;
        }

        /// <summary>
        /// Get param for url after triple DES based on the securitykey 
        /// Format "Shopname-Currency-Amount-Description-InvoiceNumber" 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public QueryParam? DecodeQueryParam(string param)
        {
            try
            {
                var flatString = TripleDESDecrypt(System.Convert.ToString(Uri.UnescapeDataString(param), System.Globalization.CultureInfo.InvariantCulture));

                var split = flatString.Split('-');
                return new QueryParam() { ShopName = split[0], Currency = split[1], Amount = split[2], Description = split[3] + " " + split[4], InvoiceExpiryInSecond = split[5] };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Encode param string (only to test)
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetEncodedQueryParam(string param)
        {
            return Uri.EscapeDataString(System.Convert.ToString(TripleDESEncrypt(param), System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Decrypt triple DES
        /// </summary>
        /// <param name="fromQueryString"></param>
        /// <returns></returns>
        private string TripleDESDecrypt(string fromQueryString)
        {
            var decrypt = Convert.FromBase64String(fromQueryString);
            var hashMD5Provider = MD5.Create();
            var securityKey = hashMD5Provider.ComputeHash(UTF8Encoding.UTF8.GetBytes(_blazorSecurityKey));

            hashMD5Provider.Clear();

            var tripleDES = TripleDES.Create();
            tripleDES.Key = securityKey;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;

            var tranform = tripleDES.CreateDecryptor();
            var result = tranform.TransformFinalBlock(decrypt, 0, decrypt.Length);

            tripleDES.Clear();

            return UTF8Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Encrypt triple DES (to test)
        /// </summary>
        /// <param name="fromQueryString"></param>
        /// <returns></returns>
        private string TripleDESEncrypt(string myString)
        {
            var encrypt = UTF8Encoding.UTF8.GetBytes(myString);
            var hashMD5Provider = MD5.Create();
            var securityKey = hashMD5Provider.ComputeHash(UTF8Encoding.UTF8.GetBytes(_blazorSecurityKey));

            hashMD5Provider.Clear();

            var tripleDES = TripleDES.Create();
            tripleDES.Key = securityKey;
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;

            var tranform = tripleDES.CreateEncryptor();

            var result = tranform.TransformFinalBlock(encrypt, 0, encrypt.Length);

            tripleDES.Clear();

            return Convert.ToBase64String(result, 0, result.Length);
        }

    }

    public class QueryParam
    {
        public string ShopName { get; set; } = "MyShop";
        public string Amount { get; set; } = "0";

        public string Currency { get; set; } = "0";
        public string Description { get; set; } = String.Empty;
        public string InvoiceExpiryInSecond { get; set; } = "360";
    }
}

