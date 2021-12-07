using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace lnd_invoice.Service
{
    public class CoingeckoRatesService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CoingeckoRatesService(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        /// <summary>
        /// Create a LND invoice
        /// </summary>
        public async Task<BitcoinPrice> GetBitcoinPrice(string currency, string amount)
        {
            if (_httpClientFactory != null)
            {
                var httpClient = _httpClientFactory.CreateClient("Exchange_rates");

                //Request
                var urlBuilder = new System.Text.StringBuilder();
                urlBuilder.Append("?ids=bitcoin&vs_currencies={currency}&include_market_cap=false&include_24hr_vol=false&include_24hr_change=false&include_last_updated_at=false");
                urlBuilder.Replace("{currency}", System.Uri.EscapeDataString(System.Convert.ToString(currency, System.Globalization.CultureInfo.InvariantCulture)));

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());

                //Response
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    var respObj = await JsonSerializer.DeserializeAsync
                       <BitcoinRate>(contentStream);

                    if (respObj != null)
                    {
                        try
                        {
                            decimal rate = ((decimal)(respObj.bitcoin?.GetType().GetProperty(currency)?.GetValue(respObj.bitcoin, null) ?? 0));
                            decimal amountDec = decimal.Parse(amount);

                            if (rate != 0)
                            {
                                var roundedSats = Math.Round(amountDec / rate * 100000000,MidpointRounding.AwayFromZero);
                                var bitcoinPrice = new BitcoinPrice() { BitcoinRate = rate, PriceInBtc = roundedSats / 100000000, PriceInSats = roundedSats };

                                return bitcoinPrice;
                            }
                            else
                                throw new ArgumentException("No available rate for the fiat to bictoin conversion");
                        }
                        catch
                        {
                            throw new Exception("Cannot manage the Fiat to bitcoin conversion");
                        }
                    }
                    else
                        throw new HttpRequestException("Coingecko returned an empty response");
                }
                else
                    throw new HttpRequestException("Bad response status code from Coingecko: " + httpResponseMessage.StatusCode);
            }
            else
                throw new HttpRequestException("Bad HttpClient Init");
        }

    }
}

