using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class InvoiceRequest
    {
        public string ShopName { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public string InvoiceIdentifier { get; set; } = string.Empty ;
        public int ExpiryInSec { get; set; } = 0;
    };
}
