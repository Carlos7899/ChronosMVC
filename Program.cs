var builder = WebApplication.CreateBuilder(args);

// Adicione o servi�o de sess�o antes de construir o aplicativo
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true; // O cookie n�o pode ser acessado pelo JavaScript
    options.Cookie.IsEssential = true; // Necess�rio para a sess�o
});

// Adicione servi�os ao cont�iner
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
