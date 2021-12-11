using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PSOBBCharacterDataDecoderWeb;
using PSOBBCharacterDataDecoderWeb.Service.Implements;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<PSOBBCharacterDataFileService>();
builder.Services.AddSingleton<PSOBBCharacterSearchFileService>();

await builder.Build().RunAsync();
