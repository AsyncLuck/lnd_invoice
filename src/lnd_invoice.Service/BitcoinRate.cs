using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class BitcoinRate
    {
        public BitcoinCurrency? bitcoin { get; set; }
    }

    public class BitcoinCurrency
    {
        public decimal usd { get; set; }
        public decimal cad { get; set; }
        public decimal gbp { get; set; }
        public decimal chf { get; set; }
    }

    

}


