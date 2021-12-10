using lnd_invoice.Service;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Config
var config = builder.Configuration.GetSection("LndConnectionSettings").Get<LndConnectionSettings>();
builder.Services.AddSingleton<LndConnectionSettings>(config);

//Httpclients
ConfigureHttpClientFactories(builder);

//Services
builder.Services.AddScoped<LndService>();
builder.Services.AddScoped<CoingeckoRatesService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.MapPost("/invoices", async (InvoiceRequest invoice, LndService lndService, CoingeckoRatesService rateService) =>
{
    //Round currency amount to 2 decimals
    invoice.Amount = Math.Round(invoice.Amount, 2); 

    //Price in btc
    var priceInBitcoin = await rateService.GetBitcoinPrice(invoice.Currency.ToLower(), invoice.Amount);

    //Generate LND invoice
    var descriptionLnd = invoice.ShopName + ":" + invoice.Description + ":" + invoice.InvoiceIdentifier + ":" + invoice.Currency + ":" + invoice.Amount;

    //PROD
    var invoiceLnd = await lndService.CreateInvoice(descriptionLnd, priceInBitcoin.PriceInSats.ToString(), invoice.ExpiryInSec.ToString());

    //TEST amount (10 sats)
    //var invoiceLnd = await lndService.CreateInvoice(descriptionLnd, "10", invoice.ExpiryInSec.ToString());

    //TEST without access to LND node
    //var invoiceLnd = new InvoiceResponse() { add_index = "test", expiry = "360", payment_addr = new byte[50], payment_request = "xxx", r_hash = new byte[50]};
    //invoiceLnd.r_hash_str = System.Convert.ToString(Convert.ToHexString(invoiceLnd.r_hash), System.Globalization.CultureInfo.InvariantCulture);

    return Results.Created($"/invoices/{invoiceLnd.r_hash}", invoiceLnd);
});

app.MapGet("/invoices/{r_hash_str}", async (string r_hash_str, LndService lndService) =>
{
    //PROD
    var invoice = await lndService.GetInvoice(System.Uri.EscapeDataString(r_hash_str),null);

    //TEST without access to LND node
    //var invoice = new InvoiceLookupResult() { creation_date = "08.12.2022", settled = true, settle_date = "now", value = "100", value_msat = "10000000000", memo="TestShop:Test payment:1:usd:1000", payment_request="TEST SHOP INVOICE" };
    
    return invoice == null ? Results.NotFound() : Results.Ok(invoice);
});


app.Run();

/// <summary>
/// Configure api requests for LND and Coingecko (rates)
/// </summary>
void ConfigureHttpClientFactories(WebApplicationBuilder builder)
{
    //Httpclient factory wiht Tor socks5 proxy
    builder.Services.AddHttpClient("Lnd_Tor", c =>
    {
        c.BaseAddress = new Uri(config.OnionAddress + ":" + config.LndRestApiPort + "/");
        c.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        c.DefaultRequestHeaders.Add("Grpc-Metadata-macaroon", config.InvoiceLndMacaroon);
        c.Timeout = TimeSpan.FromMinutes(3);

    }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    Proxy = new WebProxy("socks5://" + config.TorSocks5Proxy + ":" + config.TorSocks5ProxyPort),
                    //Insecure (maybe less with Tor)
                    ServerCertificateCustomValidationCallback =
                        (HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors) => true
                });

    //Coingecko for exchange rates (free no API key)
    builder.Services.AddHttpClient("Exchange_rates", c =>
    {
        c.BaseAddress = new Uri("https://api.coingecko.com/api/v3/simple/price");
        c.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        c.Timeout = TimeSpan.FromMinutes(3);

    });

}





