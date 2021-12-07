# LND Invoice with Blazor
Very minimal implementation of a LND invoice generator with Net 6 Blazor with a Tor connection to your personnal LND node.

Go in the Blazor project to modify the appsettings.json, put your connection info and your are set to test.

On your LND node, you need to declare the REST APi as a Tor Hidden Service

From your remote computer or VPS, you need to have Tor or Tor Browser up and running and specify your socks5 proxy port in the config.

Don't forget your macaroon too. You can extract it from your LND node like this :

```xxd -ps -u -c 1000 /path-to-lnd/data/chain/bitcoin/mainnet/invoice.macaroon```

Config
``` 
  "LndConnectionSettings": {
    "OnionAddress": "https://youronionhere.onion",
    "LndRestApiPort": "8080",
    "TorSocks5Proxy": "127.0.0.1",
    "TorSocks5ProxyPort": "9150",
    "InvoiceLndMacaroon": "your Lnd invoice macaroon str here",
    "BlazorSecurityKey": "yourblazorsecuritykeyforparam"
  }
```

The BlazorSecurity key is the Triple DES password you will use to encrypt your query string
```https://yourblazorinvoiceurl?param=yourencryptedparamforinvoicegeneration```

```
Url param decode like this
var flatString = TripleDESDecrypt(System.Convert.ToString(Uri.UnescapeDataString(param), System.Globalization.CultureInfo.InvariantCulture));
var split = flatString.Split('-');
return new QueryParam() { ShopName = split[0], Currency = split[1], Amount = split[2], Description = split[3] + " " + split[4], InvoiceExpiryInSecond = split[5] };
```


