using Azure.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Winn_BOA_Cash_Pro.Data;
using Winn_BOA_Cash_Pro.Models;

var builder = WebApplication.CreateBuilder(args);

#region Azure Key Vault
if (builder.Environment.IsProduction())
{
    //TODO: change Uri for AzureKeyVault
    builder.Configuration.AddAzureKeyVault(
        new Uri("https://winnboacashpro.vault.azure.net/"),
        new DefaultAzureCredential());
}
#endregion

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

#region Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connectionString));
#endregion
#region DefaultIdentity
builder.Services.AddDefaultIdentity<AppUser>(
                options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
#endregion
#region Azure Authentication&Authorization

builder.Services.TryAddSingleton<IConfiguration, ConfigurationRoot>();

builder.Services.AddAuthentication()
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = "https://login.microsoftonline.com/" + builder.Configuration.GetValue<string>("AzureAd:TenantId") + "/v2.0/";
    options.ClientId = builder.Configuration.GetValue<string>("AzureAd:ClientId");
    options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
    options.CallbackPath = builder.Configuration.GetValue<string>("AzureAd:CallbackPath");
    options.ClientSecret = builder.Configuration.GetValue<string>("AzureAd:ClientSecret");
    options.RequireHttpsMetadata = false;
    options.SaveTokens = false;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.SignInScheme = IdentityConstants.ExternalScheme;
});
#endregion

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddSession();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});
builder.Services.AddRazorPages()
                .AddMicrosoftIdentityUI();

var app = builder.Build();

//Configure the HTTP request pipeline.k
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();