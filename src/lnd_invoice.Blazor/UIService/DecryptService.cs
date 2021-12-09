using System.Security.Cryptography;
using System.Text;

namespace lnd_invoice.Blazor.UIService
{
    public class DecryptService
    {
        private readonly string _securityKey;
        public DecryptService(ApiConnectionSettings settings)
        {
            _securityKey = settings.SecretKey;
        }

        /// <summary>
        /// Decrypt triple DES
        /// </summary>
        /// <param name="encrypted"></param>
        /// <returns></returns>
        public string TripleDESDecrypt(string encrypted)
        {
            var decrypt = Convert.FromBase64String(encrypted);
            var hashMD5Provider = MD5.Create();
            var securityKey = hashMD5Provider.ComputeHash(UTF8Encoding.UTF8.GetBytes(_securityKey));

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
        /// Encrypt triple DES
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        public string TripleDESEncrypt(string myString)
        {
            var encrypt = UTF8Encoding.UTF8.GetBytes(myString);
            var hashMD5Provider = MD5.Create();
            var securityKey = hashMD5Provider.ComputeHash(UTF8Encoding.UTF8.GetBytes(_securityKey));

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
}
