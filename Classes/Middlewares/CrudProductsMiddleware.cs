public class CrudProductsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CrudProductsMiddleware> _logger;

    private string adminUsername { get; } = Environment.GetEnvironmentVariable("APP_USERNAME");
    private string adminPassword { get; } = Environment.GetEnvironmentVariable("APP_PASSWORD");

    public CrudProductsMiddleware(RequestDelegate next, ILogger<CrudProductsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync (HttpContext context, ProductManager manager)
    {
        var path = context.Request.Path.Value ?? "";
        var segments = path.Split("/", StringSplitOptions.RemoveEmptyEntries);

        if(path == "/")
        {
            await context.Response.WriteAsync($@"
                <!DOCTYPE html>
                <html>
                <head><title>Main</title></head>
                <body>
                    <h1>Welcome to the main page</h1>
                </body>
                </html>
            ");
            return;
        }

        if(path == "/products/login")
        {
            var username = context.Request.Query["username"].ToString();
            var password = context.Request.Query["password"].ToString();

            var isValid = username == adminUsername && password == adminPassword;
            if (isValid)
            {
                context.Response.Cookies.Append("IsAuthenticated", "true");
                await context.Response.WriteAsync($@"
                    <!DOCTYPE html>
                    <html>
                    <head><title>Login</title></head>
                    <body>
                        <h1>Admin welcome!</h1>
                    </body>
                    </html>
                ");
            }
            else
            {
                await context.Response.WriteAsync($@"
                    <!DOCTYPE html>
                    <html>
                    <head><title>Login</title></head>
                    <body>
                        <h1>Regular user welcome</h1>
                    </body>
                    </html>
                ");
            }
            return;
        }

        if(path.StartsWith("/products"))
        {
            if(context.Request.Cookies["IsAuthenticated"] != "true")
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            if(segments.Length == 1 && segments[0] == "products")
            {
                var products = string.Join("", manager.Products.Select(p => $"<tr><td>{p.Id}</td><td>{p.Name}</td><td>{p.Description}</td><td>{p.Price}</td><td><img src='{p.ImagePath}' width='100' /></td></tr>"));

                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync($@"
                    <!DOCTYPE html>
                    <html>
                    <head><title>Products</title></head>
                    <body>
                        <table border='1'>
                            <tr><th>ID</th><th>Product name</th><th>Description</th><th>Price</th><th>Image</th></tr>
                            {products}
                        </table>
                    </body>
                    </html>
                ");
                return;
            }

            
            if(segments.Length == 2 && segments[0] == "products" && segments[1] == "create")
            {
                var name = context.Request.Query["name"].ToString();
                if(name.Length < 2) return;
                var description = context.Request.Query["description"].ToString();
                if(description.Length < 2) return;
                
                if (!decimal.TryParse(context.Request.Query["price"], out decimal price)) return;
                var image = context.Request.Query["image"].ToString();

                var newProduct = new Product
                {
                    Id = manager.GenerateId(),
                    Name = name,
                    Description = description,
                    Price = price,
                    ImagePath = image,
                };
                manager.Products.Add(newProduct);
                await context.Response.WriteAsync("Product created successfully!");
                return;
            }

            if(segments.Length == 3 && segments[0] == "products" && segments[1] == "delete")
            {
                if(int.TryParse(segments[2], out int id))
                {
                    manager.Products.RemoveAll(p => p.Id == id);
                    await context.Response.WriteAsync($"Product {id} deleted.");
                }
                return;
            }
            if(segments.Length == 3 && segments[0] == "products" && segments[1] == "update")
            {
                if(int.TryParse(segments[2], out int id))
                {
                    var product = manager.Products.Find(p => p.Id == id);
                    if (product == null) return;

                    var newName = context.Request.Query["name"].ToString();
                    if(newName.Length >= 2) product.Name = newName;

                    var newDescription = context.Request.Query["description"].ToString();
                    if(newDescription.Length >= 2) product.Description = newDescription;

                    if(decimal.TryParse(context.Request.Query["price"], out decimal newPrice) && newPrice > 0)
                    {
                        product.Price = newPrice;
                    }

                    var newImage = context.Request.Query["image"].ToString();
                    if(!string.IsNullOrEmpty(newImage))
                    {
                        product.ImagePath = newImage;
                    }

                    await context.Response.WriteAsync($"Product {id} updated.");
                }
                return;
            }

            if(segments.Length == 2 && segments[0] == "products")
            {
                if(int.TryParse(segments[1], out int id))
                {
                    var product = manager.Products.Find(p => p.Id == id);
                    if (product == null) return;

                    context.Response.ContentType = "text/html; charset=utf-8";
                    var row = $"<tr><td>{product.Id}</td><td>{product.Name}</td><td>{product.Description}</td><td>{product.Price}</td><td><img src='{product.ImagePath}' width='100' /></td></tr>";

                    await context.Response.WriteAsync($@"
                        <!DOCTYPE html>
                        <html>
                        <head><title>Product</title></head>
                        <body>
                            <table border='1'>
                                <tr><th>ID</th><th>Product name</th><th>Description</th><th>Price</th><th>Image</th></tr>
                                {row}
                            </table>
                        </body>
                        </html>
                    ");
                }
                return;
            }
        }

        await _next(context);
    }
}

public static class CrudProductMiddlewareExtensions
{
    public static IApplicationBuilder UseCrudProducts(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CrudProductsMiddleware>();
    }
}