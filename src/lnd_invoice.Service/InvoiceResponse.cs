using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class InvoiceResponse
    {
        public byte[]? r_hash { get; set; }
        public string? payment_request { get; set; }
        public string? add_index { get; set; }
        public byte[]? payment_addr { get; set; }
        public string? expiry { get; set; }
    }
}

