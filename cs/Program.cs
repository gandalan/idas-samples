using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.Client.Settings;

// ------------------ CONFIGURATION ------------------
// You need to provide appGuid, user and password for the API. We use a .env file for this to simulate 
// environment variables, but you can also set the values directly in the code 🤨, get them from a configuration 
// file like appsettings.json, a database or whatever you like. Just keep it safe and secure.
//
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());
var user = Environment.GetEnvironmentVariable("IDAS_USER");
var password = Environment.GetEnvironmentVariable("IDAS_PASSWORD");

// ------------------ INITIALIZATION ------------------
// Initialize API endpoints configuration and set credentials
//
await WebApiConfigurations.Initialize(appGuid);
var settings = WebApiConfigurations.ByName("staging");
settings.UserName = user;
settings.Passwort = password;

// ------------------ LOGIN AND DO WORK ------------------
// Login and use the API
// 
var client = new WebRoutinenBase(settings);
if (await client.LoginAsync())
{
    // LoginAsync now has updated the settings WITHIN client with the auth token, user data etc.
    // Now store the auth token in our settings for later use:
    settings.AuthToken = client.AuthToken; 
    Console.WriteLine($"Login successful: User={settings.UserName} Mandant={client.AuthToken.Mandant.Name}, Environment={settings.FriendlyName}");

    // Now we can use the API
    GesamtMaterialbedarfWebRoutinen gmb = new GesamtMaterialbedarfWebRoutinen(settings);
    var bedarf = await gmb.GetAsync(DateTime.Today.AddDays(10));
    Console.WriteLine($"GesamtMaterialbedarfWebRoutinen: {bedarf.Bedarfe.Count()} items");

    string dump = Path.GetTempFileName();
    JsonSerializerOptions options = new() 
    { 
        WriteIndented = true, 
        IncludeFields = true
    };
    await File.WriteAllTextAsync(dump, JsonSerializer.Serialize(bedarf, options));
    Process.Start("notepad.exe", dump);
} else {
    Console.WriteLine("Login failed");
}