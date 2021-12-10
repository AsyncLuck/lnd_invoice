using lnd_invoice.Blazor;
using lnd_invoice.Blazor.UIService;
using lnd_invoice.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.HttpOverrides;
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
var config = builder.Configuration.GetSection("ApiConnectionSettings").Get<ApiConnectionSettings>();
string apiUrl = config.LndInvoiceApiAdress;

builder.Services.AddSingleton<ApiConnectionSettings>(config);

//Lnd invoice api
builder.Services.AddHttpClient("invoice_api", c =>
{
    c.BaseAddress = new Uri(apiUrl);
    c.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

    c.Timeout = TimeSpan.FromMinutes(3);

}).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    //Insecure
                    ServerCertificateCustomValidationCallback =
                        (HttpRequestMessage requestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslErrors) => true
                });

//Services
builder.Services.AddScoped<CopyToClipBoardService>();
builder.Services.AddScoped<DecryptService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
