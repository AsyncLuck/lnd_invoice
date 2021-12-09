using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace lnd_invoice.Service
{
    public class LndService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public LndService(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        /// <summary>
        /// Create a LND invoice
        /// </summary>
        public async Task<InvoiceResponse> CreateInvoice(string description, string amt, string expiryInSecond)
        {
            if (_httpClientFactory != null)
            {
                var httpClient = _httpClientFactory.CreateClient("Lnd_Tor");

                //Request 
                var invoice = new LndInvoiceRequest() { memo = description, value = amt, expiry = expiryInSecond };
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/invoices")
                {
                    Content = new StringContent(JsonSerializer.Serialize(invoice), Encoding.UTF8, "application/json")
                };

                //Response
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    InvoiceResponse? respObj = await JsonSerializer.DeserializeAsync
                       <InvoiceResponse>(contentStream);

                    if (respObj != null)
                    {

                        respObj.r_hash_str = respObj.r_hash == null ? "" : System.Convert.ToString(Convert.ToHexString(respObj.r_hash), System.Globalization.CultureInfo.InvariantCulture);
                        return respObj;
                    }
                    else
                        throw new HttpRequestException("Lnd returned an empty response");
                }
                else
                    throw new HttpRequestException("Bad response status code from Lnd api: " + httpResponseMessage.StatusCode);
            }
            else
                throw new HttpRequestException("Bad HttpClient Init");
        }

        /// <summary>
        /// Check if LND invoice is paid (you can choose to pass str or byte[]
        /// </summary>
        public async Task<InvoiceLookupResult> GetInvoice(string? invoiceRHash_str, byte[]? invoiceRHash)
        {
            if (_httpClientFactory != null)
            {
                var httpClient = _httpClientFactory.CreateClient("Lnd_Tor");

                //Request
                var urlBuilder = new System.Text.StringBuilder();
                urlBuilder.Append("v1/invoice/{r_hash_str}?");

                if (invoiceRHash_str != null)
                    urlBuilder.Replace("{r_hash_str}", invoiceRHash_str);
                else
                {
                    if (invoiceRHash == null)
                        throw new ArgumentNullException(nameof(invoiceRHash));
                    else
                        urlBuilder.Replace("{r_hash_str}", System.Uri.EscapeDataString(System.Convert.ToString(Convert.ToHexString(invoiceRHash), System.Globalization.CultureInfo.InvariantCulture)));
                }

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, urlBuilder.ToString());

                //Response
                var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream =
                        await httpResponseMessage.Content.ReadAsStreamAsync();

                    InvoiceLookupResult? respObj = await JsonSerializer.DeserializeAsync
                       <InvoiceLookupResult>(contentStream);

                    return respObj ?? throw new HttpRequestException("Lnd returned an empty response");
                }
                else
                    throw new HttpRequestException("Bad response status code from Lnd api: " + httpResponseMessage.StatusCode);
            }
            else
                throw new HttpRequestException("Bad HttpClient Init");
        }

    }
}

