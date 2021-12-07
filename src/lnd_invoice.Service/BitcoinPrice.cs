using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class BitcoinPrice
    {
        public decimal BitcoinRate { get; set; } = 0;
        public decimal PriceInBtc { get; set; } = 0;
        public decimal PriceInSats { get; set; } = 0;
    }
}
