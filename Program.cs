var builder = WebApplication.CreateBuilder(args);

// Adicione o serviço de sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Adicione serviços ao contêiner
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// Adicione o serviço de autenticação
builder.Services.AddAuthentication("YourCookieScheme")
    .AddCookie("YourCookieScheme", options =>
    {
        options.LoginPath = "/Corporacao/LoginCorporacao"; // Caminho para a página de login
    });

var app = builder.Build();

// Configure o pipeline de requisição HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilite o uso de sessão
app.UseSession();

// Habilite a autenticação e autorização
app.UseAuthentication(); // Coloque isso antes do UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
