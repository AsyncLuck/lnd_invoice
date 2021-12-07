using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    /// <summary>
    /// Encapsulate lnd connection seetings with Tor => 
    /// !!!!!!!!!! need to be defined in appsettings.json !!!!!!!!!!!!
    /// </summary>
    public class LndConnectionSettings
    {
        public string OnionAddress { get; set; } = "https://youronionhere.onion";
        public string LndRestApiPort { get; set; } = "8080";
        public string TorSocks5Proxy { get; set; } = "127.0.0.1";
        public string TorSocks5ProxyPort { get; set; } = "9150";
        public string InvoiceLndMacaroon { get; set; } = "your Lnd invoice macaroon";
        public string BlazorSecurityKey { get; set; } = "test";
    }
}
