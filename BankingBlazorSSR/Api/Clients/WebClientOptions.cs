using System.Text.Json;

namespace BankingBlazorSsr.Api.Clients;
public class WebClientOptions<TClient> where TClient : class {
   
   public required IHttpClientFactory HttpClientFactory { get; init; }
   public required IConfiguration Configuration { get; init; }
   public required JsonSerializerOptions JsonOptions { get; init; }
   public required CancellationTokenSource CancellationTokenSource { get; init; }
   public required ILogger<TClient> Logger { get; init; }

   //--- Factory to create the WebClientOptions ----------------------------------------
   public static WebClientOptions<TClient> Create(IServiceProvider provider) {
      return new WebClientOptions<TClient> {
         HttpClientFactory = provider.GetRequiredService<IHttpClientFactory>(),
         Configuration = provider.GetRequiredService<IConfiguration>(),
         JsonOptions = provider.GetRequiredService<JsonSerializerOptions>(),
         CancellationTokenSource = new CancellationTokenSource(),
         Logger = provider.GetRequiredService<ILogger<TClient>>()
      };
   }
}