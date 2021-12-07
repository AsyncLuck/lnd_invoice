using lnd_invoice.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

var serviceCollection = new ServiceCollection();
ConfigureServices(serviceCollection);
var serviceProvider = serviceCollection.BuildServiceProvider();
var lndService = serviceProvider.GetRequiredService<LndService>();

/// <summary>
/// Request an invoice from your Lnd node behind Tor and PAY IT ;)
/// Cannot pay to yourself, pay from another wallet
/// </summary>
try
{
    Console.Write("Enter the invoice amount in sats: ");
    var satsAmt = Console.ReadLine();

    Console.Write("Enter the invoice description: ");
    var description = Console.ReadLine();

    Console.Write("Enter the invoice expiry in second from now: ");
    var expiry = Console.ReadLine();

    var invoice = await lndService.CreateInvoice(description ?? "", satsAmt ?? "0", expiry ?? "0");
    Console.WriteLine("Pay me NOW !, Payment Request: " + invoice.payment_request + "\n");

    if (invoice.r_hash != null)
    {
        for (int i = 0; i < 300; i++)
        {
            var invoiceLookup = await lndService.IsPaid(invoice.r_hash);
            Console.WriteLine("Is paid : " + invoiceLookup.settled.ToString() + "\n");
            if (invoiceLookup.settled)
            {
                Console.WriteLine("TANKS for the payment");
                Thread.Sleep(5000);
                break;
            }
            Thread.Sleep(2000);
        }
    }

}
catch (Exception ex)
{
    Console.WriteLine("Lnd api error: " + ex.Message);
}


/// <summary>
/// Configure console services, config and httpclient factory
/// </summary>
/// <param name="services"></param>
static void ConfigureServices(IServiceCollection services)
{
    //Get config
    var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
    .Build();

    //General services and configurations
    var config = configuration.GetSection("LndConnectionSettings").Get<LndConnectionSettings>();
    var macaroon = config.InvoiceLndMacaroon;

    //Httpclient factory wiht Tor socks5 proxy
    services.AddHttpClient("Lnd_Tor", c =>
    {
        c.BaseAddress = new Uri(config.OnionAddress + ":" + config.LndRestApiPort + "/");
        c.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        c.DefaultRequestHeaders.Add("Grpc-Metadata-macaroon", macaroon);
        c.Timeout = TimeSpan.FromMinutes(2); //maybe too much (to test)

    }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    Proxy = new WebProxy("socks5://"+ config.TorSocks5Proxy + ":"+config.TorSocks5ProxyPort),
                    //Insecure (maybe less with Tor)
                    ServerCertificateCustomValidationCallback = (HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors) => true
                });

    services.AddTransient<LndService>();

}


