using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class LndInvoiceRequest
    {
        public string memo { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public string expiry { get; set; } = string.Empty;
    }
}

