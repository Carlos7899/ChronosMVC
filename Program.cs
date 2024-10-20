var builder = WebApplication.CreateBuilder(args);

// Adicione o servi�o de sess�o
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adicione servi�os ao cont�iner
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Adicione o servi�o de autentica��o
builder.Services.AddAuthentication("YourCookieScheme")
    .AddCookie("YourCookieScheme", options =>
    {
        options.LoginPath = "/Corporacao/LoginCorporacao"; // Caminho para a p�gina de login
    });

var app = builder.Build();

// Configure o pipeline de requisi��o HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilite o uso de sess�o
app.UseSession();

// Habilite a autentica��o e autoriza��o
app.UseAuthentication(); // Coloque isso antes do UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
