# NearGo

Nền tảng thương mại điện tử bán sản phẩm cận date dành cho siêu thị tại Việt Nam.

## 🚀 Công nghệ

- **Backend**: ASP.NET Core Razor Pages .NET 8, Entity Framework Core, SignalR
- **Database**: SQL Server
- **Authentication**: ASP.NET Identity
- **Frontend**: TailwindCSS, Flowbite, Alpine.js, Chart.js, Swiper.js
- **Payment**: VNPay Sandbox, MoMo Sandbox
- **AI**: OpenAI GPT-4o-mini (Chatbot)
- **Analytics**: Google Analytics

## 📋 Yêu cầu

- .NET 8 SDK
- SQL Server (LocalDB hoặc SQL Server Express/Developer)
- Visual Studio 2022 hoặc VS Code

## 🔧 Cài đặt

### 1. Clone và restore packages

```bash
cd NearGo
dotnet restore
```

### 2. Cấu hình database

Sửa file `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=NearGo;User Id=sa;Password=123;TrustServerCertificate=True"
}
```

### 3. Chạy migrations và seed data

Database sẽ tự động được migrate và seed khi chạy ứng dụng lần đầu tiên.

```bash
dotnet run
```

### 4. Truy cập ứng dụng

- **URL**: https://localhost:5001
- **HTTP**: http://localhost:5000

## 🔑 Tài khoản demo

### Admin
- Email: admin@neargo.vn
- Mật khẩu: Admin@123

### Supermarket
- Email: market1@neargo.vn
- Mật khẩu: Market@123
- (Có 20 tài khoản supermarket từ market1 đến market20)

### Customer
- Email: customer1@neargo.vn
- Mật khẩu: Customer@123
- (Có 10 tài khoản customer từ customer1 đến customer10)

## 💳 Cấu hình thanh toán

### VNPay (Sandbox)

```json
"VNPay": {
  "TmnCode": "YOUR_TMN_CODE",
  "HashSecret": "YOUR_HASH_SECRET",
  "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
  "ReturnUrl": "https://localhost:5001/payment/vnpay-return"
}
```

Đăng ký VNPay Sandbox tại: https://sandbox.vnpayment.vn/

### MoMo (Sandbox)

```json
"Momo": {
  "PartnerCode": "YOUR_PARTNER_CODE",
  "AccessKey": "YOUR_ACCESS_KEY",
  "SecretKey": "YOUR_SECRET_KEY",
  "ReturnUrl": "https://localhost:5001/checkout/momo-return",
  "NotifyUrl": "https://localhost:5001/checkout/momo-notify",
  "ApiEndpoint": "https://test-payment.momo.vn/v2/gateway/api/create"
}
```

Đăng ký MoMo Sandbox tại: https://developers.momo.vn/

## 🤖 Cấu hình AI Chatbot

```json
"OpenAI": {
  "ApiKey": "YOUR_OPENAI_API_KEY",
  "Model": "gpt-4o-mini"
}
```

Lấy API Key tại: https://platform.openai.com/

## 📊 Cấu hình Google Analytics

```json
"GoogleAnalytics": {
  "MeasurementId": "G-XXXXXXXXXX"
}
```

## 📂 Cấu trúc project

```
NearGo/
├── Pages/           # Razor Pages
│   ├── Admin/       # Admin dashboard
│   ├── Auth/        # Login, Register, ChangePassword
│   ├── Cart/        # Giỏ hàng
│   ├── Checkout/    # Thanh toán
│   ├── Customer/    # Customer dashboard
│   ├── Products/    # Sản phẩm
│   ├── Supermarket/ # Supermarket dashboard
│   ├── FlashSales/  # Flash Sale
│   ├── Chatbot/     # AI Chatbot API
│   └── Shared/      # Layouts, partial views
├── Models/          # Domain models
├── Data/            # DbContext, SeedData
├── Services/        # Business logic services
├── Hubs/            # SignalR hubs
├── Configurations/  # Settings classes
└── wwwroot/         # Static files
```

## 🚢 Deploy

### IIS

1. Publish: `dotnet publish -c Release -o ./publish`
2. Tạo Application Pool mới với .NET CLR Version: "No Managed Code"
3. Cấu hình IIS Site trỏ đến thư mục publish

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY . .
ENTRYPOINT ["dotnet", "NearGo.dll"]
```

## ✨ Tính năng

- ✅ Đăng ký, đăng nhập, phân quyền (Admin/Supermarket/Customer)
- ✅ Quản lý sản phẩm cận date
- ✅ Giỏ hàng, thanh toán (COD/VNPay/MoMo)
- ✅ Flash Sale, Voucher giảm giá
- ✅ Hộp bất ngờ (Surprise Box)
- ✅ Dashboard Admin với biểu đồ
- ✅ Dashboard Supermarket với doanh thu
- ✅ Dashboard Customer với lịch sử mua hàng
- ✅ AI Chatbot hỗ trợ khách hàng
- ✅ SignalR thông báo realtime
- ✅ Dark/Light mode
- ✅ Responsive mobile
- ✅ Sản phẩm tự động ẩn khi hết hàng/hết hạn

## 📝 License

MIT
