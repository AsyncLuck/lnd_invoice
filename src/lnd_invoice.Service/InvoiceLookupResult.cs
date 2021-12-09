using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class InvoiceLookupResult
    {
        public string? value { get; set; }
        public string? value_msat { get; set; }
        public bool settled { get; set; }
        public string? creation_date { get; set; }
        public string? settle_date { get; set; }
        public string? memo { get; set; }
        public byte[]? r_hash { get; set; }  
        public string? expiry { get; set; }
        public string? payment_request { get; set; }    
    }
}
