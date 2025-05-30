using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// (1) Add Azure AD B2C Authentication placeholder
// builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAdB2C");

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<JsonOptions>(opts =>
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter())); // Fixed line

var functionsApiBaseUrl = builder.Configuration["FunctionsApi:BaseUrl"]!
   ?? throw new InvalidOperationException("FunctionsApi:BaseUrl missing");

// Make absolutely sure it ends with a slash
if (!functionsApiBaseUrl.EndsWith('/'))
{
    functionsApiBaseUrl += "/";
}
//
builder.Services.AddHttpClient("FunctionsApi", client =>
{
    client.BaseAddress = new Uri(functionsApiBaseUrl);
});

var app = builder.Build();

app.UseStaticFiles();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
