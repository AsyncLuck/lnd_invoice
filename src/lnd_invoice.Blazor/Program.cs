using lnd_invoice.Blazor.UIService;
using lnd_invoice.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

//Config
builder.Services.AddOptions();
var config = builder.Configuration.GetSection("LndConnectionSettings").Get<LndConnectionSettings>();
builder.Services.AddSingleton<LndConnectionSettings>(config);

//Httpclients
ConfigureHttpClientFactories(builder);

//Services
builder.Services.AddScoped<LndService>();
builder.Services.AddScoped<CoingeckoRatesService>();
builder.Services.AddScoped<CopyToClipBoardService>();
builder.Services.AddScoped<QueryParamService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

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