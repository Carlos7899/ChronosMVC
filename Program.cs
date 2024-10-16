var builder = WebApplication.CreateBuilder(args);

// Adicione o serviço de sessão antes de construir o aplicativo
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // O cookie não pode ser acessado pelo JavaScript
    options.Cookie.IsEssential = true; // Necessário para a sessão
});

// Adicione serviços ao contêiner
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
