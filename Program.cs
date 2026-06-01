using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NearGo.Configurations;
using NearGo.Data;
using NearGo.Hubs;
using NearGo.Models;
using NearGo.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/auth/access-denied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Supermarket", policy => policy.RequireRole("Supermarket"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
});

builder.Services.Configure<SEPaySettings>(builder.Configuration.GetSection("SEPay"));
builder.Services.Configure<OpenAISettings>(builder.Configuration.GetSection("OpenAI"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("Gemini"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));

builder.Services.AddHttpClient<OpenAIService>();
builder.Services.AddHttpClient<GeminiService>();
builder.Services.AddScoped<ChatbotContextService>();
builder.Services.AddScoped<SEPayService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSignalR();
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "NearGo-Antiforgery";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

async Task<IResult> HandleSepayWebhook(HttpContext context, SEPayService sePayService)
{
    try
    {
        var token = context.Request.Query["token"].ToString();
        var settingsToken = sePayService.GetWebhookToken();
        if (!string.IsNullOrEmpty(settingsToken) && token != settingsToken)
        {
            return Results.Unauthorized();
        }

        string body;
        using (var reader = new StreamReader(context.Request.Body))
        {
            body = await reader.ReadToEndAsync();
        }

        var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(body);
        if (data == null)
        {
            return Results.BadRequest(new { message = "Invalid JSON" });
        }

        var content = data.GetValueOrDefault("content")?.ToString() ?? "";
        var gateway = data.GetValueOrDefault("gateway")?.ToString() ?? "";
        var transferAmountStr = data.GetValueOrDefault("transferAmount")?.ToString() ?? "0";
        var transactionId = data.GetValueOrDefault("referenceCode")?.ToString()
            ?? data.GetValueOrDefault("id")?.ToString() ?? "";

        if (string.IsNullOrEmpty(content) || !content.Contains("SEVQR"))
        {
            return Results.Ok(new { message = "Ignored - not SEVQR transfer" });
        }

        var tkpIndex = content.IndexOf("TKP", StringComparison.OrdinalIgnoreCase);
        if (tkpIndex < 0)
        {
            return Results.Ok(new { message = "Ignored - no TKP code" });
        }

        var orderCode = content.Substring(tkpIndex + 3).Trim();
        if (string.IsNullOrEmpty(orderCode))
        {
            return Results.Ok(new { message = "Ignored - empty order code" });
        }

        using var scope = context.RequestServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);

        if (order == null)
        {
            return Results.Ok(new { message = "Order not found" });
        }

        if (order.PaymentStatus == "Paid")
        {
            return Results.Ok(new { message = "Already paid" });
        }

        decimal.TryParse(transferAmountStr, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var transferAmount);

        order.PaymentStatus = "Paid";
        order.PaymentMethod = "SEPay";
        order.TransactionId = transactionId;
        order.PaymentDate = DateTime.UtcNow;
        if (order.Status == "Pending")
        {
            order.Status = "Pending";
        }

        var paymentTransaction = new PaymentTransaction
        {
            OrderId = order.Id,
            PaymentMethod = "SEPay",
            TransactionId = transactionId,
            BankCode = gateway,
            Amount = transferAmount > 0 ? transferAmount : order.TotalAmount,
            Status = "Success",
            ResponseCode = "00",
            ResponseMessage = "Thanh toán thành công qua SEPay",
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };
        db.PaymentTransactions.Add(paymentTransaction);

        var customerNotif = new Notification
        {
            UserId = order.CustomerId,
            Title = "Thanh toán thành công",
            Message = $"Đơn hàng #{order.OrderCode} đã được thanh toán qua SEPay",
            Type = "Payment",
            RelatedUrl = $"/customer/orders/detail?id={order.Id}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Notifications.Add(customerNotif);

        await db.SaveChangesAsync();

        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();
        await hubContext.Clients.Group($"user_{order.CustomerId}")
            .SendAsync("ReceiveNotification", "Thanh toán thành công",
                $"Đơn hàng #{order.OrderCode} đã được thanh toán qua SEPay", "");

        return Results.Ok(new { message = "OK" });
    }
    catch (Exception ex)
    {
        return Results.Ok(new { message = $"Error: {ex.Message}" });
    }
}

app.MapPost("/payment/sepay-webhook", HandleSepayWebhook).WithDisplayName("SEPayWebhook");
app.MapPost("/", HandleSepayWebhook).WithDisplayName("SEPayWebhookRoot");

app.MapGet("/api/payment/status/{orderCode}", async (string orderCode, ApplicationDbContext db) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);
    if (order == null) return Results.NotFound();
    return Results.Ok(new { paid = order.PaymentStatus == "Paid" });
});

app.MapFallbackToPage("/NotFound");

try
{
    Log.Information("Starting NearGo application");
    await SeedData.Initialize(app.Services);
    Log.Information("Database seeded successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to seed database");
    throw;
}

app.Run();
