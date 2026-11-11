DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ProductManager>();

var app = builder.Build();
app.UseStaticFiles();
app.UseCrudProducts();

app.Run();
