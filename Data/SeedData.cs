using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NearGo.Models;

namespace NearGo.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            if (await roleManager.RoleExistsAsync("Admin")) return;

            await roleManager.CreateAsync(new IdentityRole("Admin"));
            await roleManager.CreateAsync(new IdentityRole("Supermarket"));
            await roleManager.CreateAsync(new IdentityRole("Customer"));

            var admin = new AppUser
            {
                UserName = "admin@neargo.vn",
                Email = "admin@neargo.vn",
                FullName = "Quản trị viên NearGo",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");

            var categories = new List<Category>
            {
                new Category { Name = "Sữa & Chế phẩm từ sữa", Slug = "sua-che-pham-tu-sua", Description = "Sữa tươi, sữa chua, phô mai và các chế phẩm từ sữa", ImageUrl = "https://images.unsplash.com/photo-1550583724-b2692b85b150?w=400", IconClass = "fas fa-glass-whiskey", SortOrder = 1 },
                new Category { Name = "Thịt & Hải sản", Slug = "thit-hai-san", Description = "Thịt heo, thịt bò, thịt gà và hải sản tươi sống", ImageUrl = "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?w=400", IconClass = "fas fa-drumstick-bite", SortOrder = 2 },
                new Category { Name = "Rau củ quả", Slug = "rau-cu-qua", Description = "Rau xanh, củ quả tươi nhập khẩu", ImageUrl = "https://images.unsplash.com/photo-1566385101042-1a0aa0c1268c?w=400", IconClass = "fas fa-carrot", SortOrder = 3 },
                new Category { Name = "Bánh kẹo & Snack", Slug = "banh-keo-snack", Description = "Bánh quy, kẹo, snack các loại", ImageUrl = "https://images.unsplash.com/photo-1576618148400-af8e4f30e04b?w=400", IconClass = "fas fa-cookie-bite", SortOrder = 4 },
                new Category { Name = "Nước giải khát", Slug = "nuoc-giai-khat", Description = "Nước ngọt, nước khoáng, trà, cà phê", ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400", IconClass = "fas fa-wine-bottle", SortOrder = 5 },
                new Category { Name = "Gia vị & Nguyên liệu", Slug = "gia-vi-nguyen-lieu", Description = "Gia vị nấu ăn, nguyên liệu chế biến", ImageUrl = "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=400", IconClass = "fas fa-mortar-pestle", SortOrder = 6 },
                new Category { Name = "Mì & Đồ khô", Slug = "mi-do-kho", Description = "Mì gói, miến, bún, nui và đồ khô", ImageUrl = "https://images.unsplash.com/photo-1612927601601-2e9c5a78e3c1?w=400", IconClass = "fas fa-egg", SortOrder = 7 },
                new Category { Name = "Đồ đông lạnh", Slug = "do-dong-lanh", Description = "Thực phẩm đông lạnh tiện lợi", ImageUrl = "https://images.unsplash.com/photo-1625943553852-781c6dd46faa?w=400", IconClass = "fas fa-snowflake", SortOrder = 8 },
                new Category { Name = "Đồ uống có cồn", Slug = "do-uong-co-con", Description = "Bia, rượu vang và đồ uống có cồn", ImageUrl = "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=400", IconClass = "fas fa-wine-glass-alt", SortOrder = 9 },
                new Category { Name = "Chăm sóc nhà cửa", Slug = "cham-soc-nha-cua", Description = "Nước rửa chén, nước lau sàn, chất tẩy rửa", ImageUrl = "https://images.unsplash.com/photo-1585421514284-efb74c2b69ba?w=400", IconClass = "fas fa-soap", SortOrder = 10 },
                new Category { Name = "Chăm sóc cá nhân", Slug = "cham-soc-ca-nhan", Description = "Sữa tắm, dầu gội, kem đánh răng", ImageUrl = "https://images.unsplash.com/photo-1556228578-0d85b1a4d571?w=400", IconClass = "fas fa-hand-sparkles", SortOrder = 11 },
                new Category { Name = "Đồ ăn vặt", Slug = "do-an-vat", Description = "Đồ ăn vặt nhập khẩu, local brand", ImageUrl = "https://images.unsplash.com/photo-1586998001936-b3076d4d0cb9?w=400", IconClass = "fas fa-candy-cane", SortOrder = 12 },
                new Category { Name = "Bia & Đồ nhậu", Slug = "bia-do-nhau", Description = "Bia tươi, bia chai và đồ nhậu", ImageUrl = "https://images.unsplash.com/photo-1566633806327-68e152aaf26d?w=400", IconClass = "fas fa-beer", SortOrder = 13 },
                new Category { Name = "Sản phẩm Organic", Slug = "san-pham-organic", Description = "Sản phẩm hữu cơ, sạch, an toàn", ImageUrl = "https://images.unsplash.com/photo-1550989460-0adf9ea622e2?w=400", IconClass = "fas fa-leaf", SortOrder = 14 },
                new Category { Name = "Quà tặng & Set quà", Slug = "qua-tang-set-qua", Description = "Hộp quà tặng, set quà Tết", ImageUrl = "https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400", IconClass = "fas fa-gift", SortOrder = 15 }
            };
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            var supermarkets = new List<Supermarket>();
            var supermarketUsers = new List<(string name, string email, string phone, string address, string slug, string desc, string logo, string cover, bool verified, string tier)>()
            {
                ("VinMart+ Thủ Đức", "market1@neargo.vn", "0901122334", "123 Lê Văn Việt, Thủ Đức, TP.HCM", "vinmart-thu-duc", "VinMart+ cung cấp thực phẩm sạch, an toàn với giá tốt nhất thị trường.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", true, "Premium"),
                ("Co.op Food Bình Thạnh", "market2@neargo.vn", "0902233445", "456 Nguyễn Xí, Bình Thạnh, TP.HCM", "coop-food-binh-thanh", "Hệ thống siêu thị thực phẩm Co.op Food với hàng ngàn sản phẩm chất lượng.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", true, "Premium"),
                ("AEON Food Market", "market3@neargo.vn", "0903344556", "789 Tân Phú, Quận 7, TP.HCM", "aeon-food-market", "AEON Food Market - Siêu thị Nhật Bản với thực phẩm nhập khẩu chất lượng cao.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1583071299210-c6c113f4e8d6?w=1200", true, "Premium"),
                ("Bách Hóa Xanh Quận 1", "market4@neargo.vn", "0904455667", "12 Nguyễn Huệ, Quận 1, TP.HCM", "bach-hoa-xanh-q1", "Bách Hóa Xanh - thực phẩm tươi sống giá gốc mỗi ngày.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", true, "Standard"),
                ("Mart Vạn Phúc", "market5@neargo.vn", "0905566778", "345 Vạn Phúc, Hà Đông, Hà Nội", "mart-van-phuc", "Mart Vạn Phúc - siêu thị gia đình với hơn 10 năm kinh nghiệm.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", false, "Standard"),
                ("Green Food Đà Nẵng", "market6@neargo.vn", "0906677889", "567 Nguyễn Văn Linh, Đà Nẵng", "green-food-da-nang", "Thực phẩm xanh, sạch, an toàn cho mọi gia đình.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", true, "Premium"),
                ("Fresh Market Cần Thơ", "market7@neargo.vn", "0907788990", "89 Mậu Thân, Ninh Kiều, Cần Thơ", "fresh-market-can-tho", "Fresh Market - Nông sản sạch từ đồng bằng sông Cửu Long.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", false, "Free"),
                ("Satra Food Biên Hòa", "market8@neargo.vn", "0908899001", "234 Phạm Văn Thuật, Biên Hòa, Đồng Nai", "satra-food-bien-hoa", "Satra Food - Hệ thống thực phẩm an toàn số 1 Đồng Nai.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", true, "Standard"),
                ("Market Hải Phòng", "market9@neargo.vn", "0909900112", "456 Lạch Tray, Ngô Quyền, Hải Phòng", "market-hai-phong", "Siêu thị thực phẩm Hải Phòng với hải sản tươi ngon mỗi ngày.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", false, "Free"),
                ("Eco Food Huế", "market10@neargo.vn", "0910011223", "78 Trần Hưng Đạo, Huế", "eco-food-hue", "Thực phẩm Organic và đặc sản Huế.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1583071299210-c6c113f4e8d6?w=1200", false, "Standard"),
                ("Market Nha Trang", "market11@neargo.vn", "0911122334", "123 Nguyễn Trãi, Nha Trang, Khánh Hòa", "market-nha-trang", "Hải sản tươi sống Nha Trang chất lượng cao.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", true, "Standard"),
                ("Foody Market Vũng Tàu", "market12@neargo.vn", "0912233445", "567 Thùy Vân, Vũng Tàu", "foody-market-vung-tau", "Thực phẩm nhập khẩu và hải sản Vũng Tàu.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", false, "Free"),
                ("Organic Mart Đà Lạt", "market13@neargo.vn", "0913344556", "89 Nguyễn Văn Cừ, Đà Lạt", "organic-mart-da-lat", "Rau củ Organic Đà Lạt - Nông sản sạch cao nguyên.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", true, "Premium"),
                ("Mart Quy Nhơn", "market14@neargo.vn", "0914455667", "234 Trần Hưng Đạo, Quy Nhơn", "mart-quy-nhon", "Hải sản và thực phẩm tươi Quy Nhơn.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", false, "Free"),
                ("City Food Cần Thơ", "market15@neargo.vn", "0915566778", "456 Nguyễn Văn Cừ, Ninh Kiều, Cần Thơ", "city-food-can-tho", "City Food - Thực phẩm thành phố miền Tây.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", false, "Standard"),
                ("Sài Gòn Food", "market16@neargo.vn", "0916677889", "789 Lý Thường Kiệt, Quận 10, TP.HCM", "sai-gon-food", "Sài Gòn Food - Thực phẩm Sài Gòn với đa dạng sản phẩm.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", true, "Premium"),
                ("Metro An Phú", "market17@neargo.vn", "0917788990", "12 Cao Thắng, Quận 3, TP.HCM", "metro-an-phu", "Metro An Phú - Hàng nhập khẩu và bán sỉ.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", true, "Standard"),
                ("Happy Food Bình Dương", "market18@neargo.vn", "0918899001", "567 Đại lộ Bình Dương, Thủ Dầu Một", "happy-food-binh-duong", "Happy Food - Giao hàng nhanh thực phẩm sạch.", "https://images.unsplash.com/photo-1578916171728-46686eac8d58?w=200", "https://images.unsplash.com/photo-1604712409871-3f04eecb64c1?w=1200", false, "Free"),
                ("Food Center Đà Nẵng", "market19@neargo.vn", "0919900112", "234 Hùng Vương, Đà Nẵng", "food-center-da-nang", "Food Center - Thực phẩm Đà Nẵng giá gốc.", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=200", "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", true, "Standard"),
                ("Best Food HCM", "market20@neargo.vn", "0920011223", "345 Nguyễn Đình Chiểu, Quận 1, TP.HCM", "best-food-hcm", "Best Food - Thực phẩm chất lượng nhất cho bạn.", "https://images.unsplash.com/photo-1558618666-fcd25c85f82e?w=200", "https://images.unsplash.com/photo-1588964895597-cfccd6e2dbf9?w=1200", true, "Premium")
            };

            var supermarketIds = new List<int>();
            foreach (var (name, email, phone, address, slug, desc, logo, cover, verified, tier) in supermarketUsers)
            {
                var sm = new Supermarket
                {
                    Name = name,
                    Slug = slug,
                    Description = desc,
                    LogoUrl = logo,
                    CoverImageUrl = cover,
                    Address = address,
                    Phone = phone,
                    Email = email,
                    IsActive = true,
                    IsVerified = verified,
                    SubscriptionTier = tier,
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                    Rating = Random.Shared.NextDouble() * 2 + 3,
                    ReviewCount = Random.Shared.Next(10, 500)
                };
                context.Supermarkets.Add(sm);
                await context.SaveChangesAsync();
                supermarketIds.Add(sm.Id);

                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    PhoneNumber = phone,
                    EmailConfirmed = true,
                    IsActive = true,
                    SupermarketId = sm.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, "Market@123");
                await userManager.AddToRoleAsync(user, "Supermarket");
            }

            var customers = new List<(string name, string email, string phone, string address)>
            {
                ("Nguyễn Văn An", "customer1@neargo.vn", "0901010101", "123 Nguyễn Trãi, Quận 1, TP.HCM"),
                ("Trần Thị Bình", "customer2@neargo.vn", "0902020202", "456 Lê Lợi, Quận 3, TP.HCM"),
                ("Lê Hoàng Cường", "customer3@neargo.vn", "0903030303", "789 Hai Bà Trưng, Quận 1, TP.HCM"),
                ("Phạm Thị Dung", "customer4@neargo.vn", "0904040404", "12 Nguyễn Huệ, Quận Bình Thạnh, TP.HCM"),
                ("Hoàng Văn Em", "customer5@neargo.vn", "0905050505", "34 Trần Phú, Quận 5, TP.HCM"),
                ("Võ Thị Phượng", "customer6@neargo.vn", "0906060606", "56 Lý Tự Trọng, Quận 1, TP.HCM"),
                ("Đặng Minh Giàu", "customer7@neargo.vn", "0907070707", "78 Nguyễn Đình Chiểu, Quận 3, TP.HCM"),
                ("Bùi Thị Hạnh", "customer8@neargo.vn", "0908080808", "90 Cách Mạng Tháng 8, Quận 10, TP.HCM"),
                ("Ngô Văn Ý", "customer9@neargo.vn", "0909090909", "123 Võ Văn Tần, Quận 3, TP.HCM"),
                ("Dương Thị Kim", "customer10@neargo.vn", "0910101010", "456 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM")
            };

            foreach (var (name, email, phone, address) in customers)
            {
                var user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    FullName = name,
                    PhoneNumber = phone,
                    Address = address,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(user, "Customer@123");
                await userManager.AddToRoleAsync(user, "Customer");
            }

            var productData = new List<(string name, decimal price, double disc, int stock, DateTime expiry, string img, int catIdx, string unit, string origin, string desc, string tags)>
            {
                ("TH True Milk 1L", 45000, 20, 50, DateTime.UtcNow.AddDays(5), "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=400", 0, "hộp", "Việt Nam", "Sữa tươi TH True Milk tiệt trùng 1L, hạn sử dụng còn 5 ngày", "sữa,th true milk,tiệt trùng"),
                ("Sữa chua Vinamilk 4 hộp", 35000, 30, 100, DateTime.UtcNow.AddDays(3), "https://images.unsplash.com/photo-1571212515416-f18001d9b4c7?w=400", 0, "lốc", "Việt Nam", "Sữa chua Vinamilk hương dâu, 4 hộp/lốc", "sữa chua,vinamilk,đường"),
                ("Phô mai Con Bò Cười 48g", 25000, 25, 80, DateTime.UtcNow.AddDays(7), "https://images.unsplash.com/photo-1585822545290-8db9e5c2c12a?w=400", 0, "gói", "Việt Nam", "Phô mai Con Bò Cười vị nguyên bản", "phô mai,con bò cười"),
                ("Ba rọi heo Mỹ 500g", 85000, 35, 30, DateTime.UtcNow.AddDays(2), "https://images.unsplash.com/photo-1607623814075-e51df1bdc82f?w=400", 1, "kg", "Mỹ", "Ba rọi heo Mỹ nhập khẩu, thịt tươi ngon", "thịt heo,ba rọi,mỹ"),
                ("Đùi gà tươi 1kg", 65000, 25, 40, DateTime.UtcNow.AddDays(4), "https://images.unsplash.com/photo-1587593810167-a84920ea0781?w=400", 1, "kg", "Việt Nam", "Đùi gà tươi, thịt chắc, ngọt", "đùi gà,thịt gà"),
                ("Tôm sú size lớn 500g", 120000, 40, 20, DateTime.UtcNow.AddDays(1), "https://images.unsplash.com/photo-1565680018434-b513d5e47fd5?w=400", 1, "kg", "Việt Nam", "Tôm sú tươi size lớn, đánh bắt tự nhiên", "tôm sú,hải sản"),
                ("Cá hồi Nauy phi lê 200g", 150000, 20, 15, DateTime.UtcNow.AddDays(6), "https://images.unsplash.com/photo-1579705745899-e93e89e9ed06?w=400", 1, "kg", "Nauy", "Cá hồi Nauy phi lê nguyên vỉ, giàu Omega-3", "cá hồi,nauy,phi lê"),
                ("Bông cải xanh Đà Lạt 500g", 25000, 30, 60, DateTime.UtcNow.AddDays(3), "https://images.unsplash.com/photo-1582341951677-80c5aed9e241?w=400", 2, "bông", "Việt Nam", "Bông cải xanh tươi Đà Lạt, không thuốc trừ sâu", "bông cải,đà lạt,rau"),
                ("Cà rốt hữu cơ 1kg", 30000, 25, 70, DateTime.UtcNow.AddDays(5), "https://images.unsplash.com/photo-1447175008436-054170c2e979?w=400", 2, "kg", "Việt Nam", "Cà rốt hữu cơ, ngọt, giàu vitamin A", "cà rốt,hữu cơ"),
                ("Cải thảo 1kg", 20000, 20, 80, DateTime.UtcNow.AddDays(4), "https://images.unsplash.com/photo-1551908129-e7c92879ebdb?w=400", 2, "kg", "Việt Nam", "Cải thảo tươi, giòn, ngọt", "cải thảo,rau xanh"),
                ("Oreo vị sữa 97g", 15000, 30, 120, DateTime.UtcNow.AddDays(10), "https://images.unsplash.com/photo-1558961363-fa8fdf82db35?w=400", 3, "gói", "Mỹ", "Bánh Oreo vị sữa, nhân kem vani", "oreo,bánh,quy"),
                ("Snack Poca 60g", 10000, 35, 150, DateTime.UtcNow.AddDays(8), "https://images.unsplash.com/photo-1621447504864-d8689a3e6c8d?w=400", 3, "gói", "Việt Nam", "Snack Poca khoai tây vị BBQ", "snack,poca,khoai tây"),
                ("Kẹo Socola Ferrero 8 viên", 85000, 25, 40, DateTime.UtcNow.AddDays(15), "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=400", 3, "hộp", "Ý", "Kẹo Socola Ferrero Rocher nhập khẩu Ý", "socola,ferrero,kẹo"),
                ("Coca Cola Zero 1.5L", 15000, 20, 200, DateTime.UtcNow.AddDays(20), "https://images.unsplash.com/photo-1562686241-3d8e2b2f7e8b?w=400", 4, "chai", "Mỹ", "Coca Cola Zero không đường, 1.5L", "coca cola,nước ngọt,zero"),
                ("Trà xanh Ocha 0.5L", 12000, 25, 180, DateTime.UtcNow.AddDays(12), "https://images.unsplash.com/photo-1556679343-c7306c1976bc?w=400", 4, "chai", "Việt Nam", "Trà xanh Ocha ít ngọt, thanh mát", "trà xanh,ocha"),
                ("Nước suối Aquafina 1L", 8000, 15, 300, DateTime.UtcNow.AddDays(30), "https://images.unsplash.com/photo-1560053608-13721e0d69e9?w=400", 4, "chai", "Việt Nam", "Nước suối Aquafina tinh khiết", "nước suối,aquafina"),
                ("Dầu ăn Tường An 1L", 40000, 20, 60, DateTime.UtcNow.AddDays(14), "https://images.unsplash.com/photo-1515989876540-6d4f32e3ad18?w=400", 5, "chai", "Việt Nam", "Dầu ăn Tường An tinh luyện từ dầu thực vật", "dầu ăn,tường an"),
                ("Nước mắm Nam Ngư 500ml", 35000, 25, 50, DateTime.UtcNow.AddDays(20), "https://images.unsplash.com/photo-1596040033229-a9821ebd058d?w=400", 5, "chai", "Việt Nam", "Nước mắm cốt Nam Ngư đậm đà", "nước mắm,nam ngư"),
                ("Hạt nêm Knorr 200g", 20000, 30, 90, DateTime.UtcNow.AddDays(18), "https://images.unsplash.com/photo-1612927601601-2e9c5a78e3c1?w=400", 5, "gói", "Việt Nam", "Hạt nêm Knorr từ thịt heo thượng hạng", "hạt nêm,knorr"),
                ("Mì Hảo Hảo 30 gói", 85000, 20, 100, DateTime.UtcNow.AddDays(25), "https://images.unsplash.com/photo-1612927601601-2e9c5a78e3c1?w=400", 6, "thùng", "Việt Nam", "Mì gói Hảo Hảo tôm chua cay, thùng 30 gói", "mì,hảo hảo,gói"),
                ("Miến dong Việt 500g", 25000, 25, 70, DateTime.UtcNow.AddDays(22), "https://images.unsplash.com/photo-1612927601601-2e9c5a78e3c1?w=400", 6, "gói", "Việt Nam", "Miến dong Việt sạch, dai ngon", "miến dong,miến"),
                ("Cá basa đông lạnh phi lê 500g", 45000, 35, 35, DateTime.UtcNow.AddDays(7), "https://images.unsplash.com/photo-1625943553852-781c6dd46faa?w=400", 7, "gói", "Việt Nam", "Cá basa phi lê đông lạnh sạch", "cá basa,đông lạnh"),
                ("Đậu hũ non Đại Phát 400g", 15000, 25, 80, DateTime.UtcNow.AddDays(4), "https://images.unsplash.com/photo-1625943553852-781c6dd46faa?w=400", 7, "cây", "Việt Nam", "Đậu hũ non Đại Phát, mềm mịn", "đậu hũ,đậu phụ"),
                ("Bia Tiger 330ml", 12000, 30, 200, DateTime.UtcNow.AddDays(45), "https://images.unsplash.com/photo-1566633806327-68e152aaf26d?w=400", 8, "lon", "Việt Nam", "Bia Tiger nâu, hương vị mạnh mẽ", "bia tiger,bia"),
                ("Rượu Vang Chilano 750ml", 250000, 25, 20, DateTime.UtcNow.AddDays(60), "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=400", 8, "chai", "Chile", "Rượu vang Chilano Cabernet Sauvignon", "rượu vang,chilano"),
                ("Sáp thơm Febreze 300ml", 45000, 30, 60, DateTime.UtcNow.AddDays(30), "https://images.unsplash.com/photo-1585421514284-efb74c2b69ba?w=400", 9, "chai", "Mỹ", "Sáp thơm phòng Febreze hương oải hương", "febreze,sáp thơm"),
                ("Dầu gội Sunsilk 650ml", 85000, 20, 45, DateTime.UtcNow.AddDays(40), "https://images.unsplash.com/photo-1556228578-0d85b1a4d571?w=400", 10, "chai", "Thái Lan", "Dầu gội Sunsilk mượt mà tổ ong", "dầu gội,sunsilk"),
                ("Kem đánh răng P/S 225g", 35000, 25, 80, DateTime.UtcNow.AddDays(50), "https://images.unsplash.com/photo-1556228578-0d85b1a4d571?w=400", 10, "tuýp", "Việt Nam", "Kem đánh răng P/S trắng sáng", "kem đánh răng,p/s"),
                ("Bánh tráng trộn 500g", 25000, 20, 100, DateTime.UtcNow.AddDays(15), "https://images.unsplash.com/photo-1586998001936-b3076d4d0cb9?w=400", 11, "gói", "Việt Nam", "Bánh tráng trộn sẵn gia vị", "bánh tráng,đồ ăn vặt"),
                ("Hạt dẻ cười rang muối 200g", 65000, 30, 40, DateTime.UtcNow.AddDays(20), "https://images.unsplash.com/photo-1586998001936-b3076d4d0cb9?w=400", 11, "gói", "Mỹ", "Hạt dẻ cười rang muối nhập khẩu", "hạt dẻ,hạt khô"),
                ("Bia Heineken 330ml", 15000, 25, 250, DateTime.UtcNow.AddDays(35), "https://images.unsplash.com/photo-1566633806327-68e152aaf26d?w=400", 12, "lon", "Hà Lan", "Bia Heineken xanh, nhập khẩu Hà Lan", "heineken,bia"),
                ("Rau hữu cơ Mix 500g", 35000, 30, 50, DateTime.UtcNow.AddDays(3), "https://images.unsplash.com/photo-1550989460-0adf9ea622e2?w=400", 13, "túi", "Việt Nam", "Bộ rau hữu cơ mix các loại", "rau hữu cơ,organic"),
                ("Set quà Tết Luxury 2025", 500000, 20, 10, DateTime.UtcNow.AddDays(60), "https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400", 14, "set", "Việt Nam", "Hộp quà Tết cao cấp với rượu vang, socola, trà", "set quà,tết,quà tặng"),
                ("Snack Lays 60g", 12000, 25, 180, DateTime.UtcNow.AddDays(14), "https://images.unsplash.com/photo-1621447504864-d8689a3e6c8d?w=400", 3, "gói", "Mỹ", "Snack Lays khoai tây vị tự nhiên", "snack,lays,khoai tây"),
                ("Bánh Cosy 192g", 25000, 20, 90, DateTime.UtcNow.AddDays(20), "https://images.unsplash.com/photo-1576618148400-af8e4f30e04b?w=400", 3, "gói", "Việt Nam", "Bánh Cosy mềm bơ", "bánh cosy,bơ"),
            };

            var products = new List<Product>();
            var rand = new Random();
            int productCount = 0;
            foreach (var (name, price, disc, stock, expiry, img, catIdx, unit, origin, desc, tags) in productData)
            {
                int smId = supermarketIds[productCount % supermarketIds.Count];
                var discountedPrice = price - (price * (decimal)(disc / 100));
                var prod = new Product
                {
                    Name = name,
                    Slug = name.ToLower().Replace(" ", "-").Replace(",", "").Replace(".", "").Replace("&", "va").Replace("/", "-") + "-" + Guid.NewGuid().ToString().Substring(0, 6),
                    Description = desc,
                    OriginalPrice = price,
                    DiscountedPrice = Math.Round(discountedPrice, 0),
                    DiscountPercentage = disc,
                    StockQuantity = stock,
                    ExpiryDate = expiry,
                    ImageUrl = img,
                    Unit = unit,
                    Origin = origin,
                    IsActive = true,
                    IsBoosted = productCount < 10,
                    BoostExpiry = productCount < 10 ? DateTime.UtcNow.AddDays(30) : null,
                    ViewCount = rand.Next(50, 5000),
                    SoldCount = rand.Next(10, 500),
                    SmartExpiryScore = Math.Round(100 - ((DateTime.UtcNow.AddDays(60) - expiry).TotalDays / 60 * 100), 1),
                    Tags = tags,
                    CategoryId = categories[catIdx].Id,
                    SupermarketId = smId,
                    CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 30))
                };
                products.Add(prod);
                productCount++;
            }
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            var banners = new List<Banner>
            {
                new Banner { Title = "Flash Sale Siêu Hời", Subtitle = "Giảm đến 50% sản phẩm cận date", ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1200", LinkUrl = "/flashsales", ButtonText = "Mua ngay", SortOrder = 1, IsActive = true, Status = "Approved", PaymentStatus = "Paid", PackageDays = 30, PackagePrice = 200000 },
                new Banner { Title = "Sản phẩm Organic", Subtitle = "Thực phẩm sạch, an toàn cho gia đình", ImageUrl = "https://images.unsplash.com/photo-1550989460-0adf9ea622e2?w=1200", LinkUrl = "/products?category=san-pham-organic", ButtonText = "Khám phá", SortOrder = 2, IsActive = true, Status = "Approved", PaymentStatus = "Paid", PackageDays = 30, PackagePrice = 200000 },
                new Banner { Title = "Tiết kiệm cùng NearGo", Subtitle = "Mua thực phẩm cận date - Chất lượng tốt, giá tốt", ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=1200", LinkUrl = "/products", ButtonText = "Xem ngay", SortOrder = 3, IsActive = true, Status = "Approved", PaymentStatus = "Paid", PackageDays = 30, PackagePrice = 200000 }
            };
            context.Banners.AddRange(banners);

            var testBanner = new Banner
            {
                Title = "Khuyến Mãi Tháng 6",
                Subtitle = "Giảm sốc 30% thực phẩm cận date - Có giá trị đến hết tháng",
                ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=1200",
                LinkUrl = "/flashsales",
                ButtonText = "Xem ngay",
                SupermarketId = 1,
                Status = "Pending",
                PaymentStatus = "Paid",
                PackageDays = 7,
                PackagePrice = 50000,
                IsActive = false,
                SortOrder = 0,
                CreatedAt = DateTime.UtcNow
            };
            context.Banners.Add(testBanner);

            var vouchers = new List<Voucher>();
            for (int i = 0; i < 30; i++)
            {
                vouchers.Add(new Voucher
                {
                    Code = $"NEARGO{rand.Next(1000, 9999)}",
                    Title = $"Khuyến mãi {rand.Next(5, 30)}%",
                    Description = "Giảm giá cho đơn hàng từ 100,000đ",
                    DiscountType = rand.Next(2) == 0 ? "Percentage" : "Fixed",
                    DiscountValue = rand.Next(2) == 0 ? rand.Next(5, 30) : rand.Next(10000, 50000),
                    MaxDiscountAmount = 100000,
                    MinOrderAmount = 100000,
                    MaxUsage = 100,
                    CurrentUsage = rand.Next(0, 50),
                    SupermarketId = i < 20 ? supermarketIds[i % supermarketIds.Count] : null,
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    ExpiryDate = DateTime.UtcNow.AddDays(rand.Next(5, 30)),
                    IsActive = true
                });
            }
            context.Vouchers.AddRange(vouchers);

            var flashSales = new List<FlashSale>
            {
                new FlashSale { Title = "Flash Sale Cuối Tuần", Description = "Giảm sốc cuối tuần", ImageUrl = "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?w=800", StartTime = DateTime.UtcNow.AddHours(-2), EndTime = DateTime.UtcNow.AddHours(22), IsActive = true },
                new FlashSale { Title = "Flash Sale Thực Phẩm", Description = "Giảm giá thực phẩm cận date", ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=800", StartTime = DateTime.UtcNow.AddDays(-1), EndTime = DateTime.UtcNow.AddDays(1), IsActive = true },
                new FlashSale { Title = "Sale Đồ Uống", Description = "Giảm đến 40% đồ uống", ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=800", StartTime = DateTime.UtcNow.AddHours(2), EndTime = DateTime.UtcNow.AddDays(2), IsActive = true }
            };
            context.FlashSales.AddRange(flashSales);
            await context.SaveChangesAsync();

            for (int i = 0; i < 20 && i < products.Count; i++)
            {
                var fp = new FlashSaleProduct
                {
                    FlashSaleId = flashSales[i % 3].Id,
                    ProductId = products[i].Id,
                    SalePrice = products[i].DiscountedPrice * 0.7m,
                    MaxQuantity = products[i].StockQuantity / 2,
                    SoldQuantity = rand.Next(0, products[i].StockQuantity / 3)
                };
                context.FlashSaleProducts.Add(fp);
            }

            var surpriseBoxes = new List<SurpriseBox>
            {
                new SurpriseBox { Name = "Hộp bất ngờ Thực phẩm", Description = "5 sản phẩm thực phẩm cận date ngẫu nhiên - giá siêu rẻ!", Price = 100000, OriginalValue = 250000, ImageUrl = "https://images.unsplash.com/photo-1549465220-1a8b9238cd48?w=400", StockQuantity = 50, IsActive = true },
                new SurpriseBox { Name = "Hộp bất ngờ Đồ uống", Description = "6 lon/chai đồ uống ngẫu nhiên - tiết kiệm 50%!", Price = 80000, OriginalValue = 180000, ImageUrl = "https://images.unsplash.com/photo-1544145945-f90425340c7e?w=400", StockQuantity = 30, IsActive = true },
                new SurpriseBox { Name = "Hộp bất ngờ Snack", Description = "10 gói snack ngẫu nhiên - cho ngày xem phim!", Price = 60000, OriginalValue = 150000, ImageUrl = "https://images.unsplash.com/photo-1576618148400-af8e4f30e04b?w=400", StockQuantity = 40, IsActive = true }
            };
            context.SurpriseBoxes.AddRange(surpriseBoxes);
            await context.SaveChangesAsync();

            await context.SaveChangesAsync();
        }
    }
}
