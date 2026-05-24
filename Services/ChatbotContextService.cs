using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;
using System.Security.Claims;
using System.Text;

namespace NearGo.Services
{
    public class ChatbotContextService
    {
        private readonly ApplicationDbContext _db;

        private static readonly string[] StopWords = ["có", "không", "và", "của", "là", "các", "những",
            "để", "mà", "cho", "với", "tôi", "bạn", "anh", "chị", "em", "nào", "gì", "thế",
            "vậy", "được", "rồi", "ạ", "nhé", "nha", "à", "ừ", "vâng", "dạ", "ơi", "thì",
            "đang", "sẽ", "đã", "nên", "hay", "vừa", "mới", "cũng", "lắm", "quá", "như",
            "khi", "vì", "nếu", "tại", "trên", "dưới", "trong", "ngoài", "ra", "vào"];

        public ChatbotContextService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ChatbotContextResult> BuildContext(string userMessage, ClaimsPrincipal? user)
        {
            var result = new ChatbotContextResult();
            var sb = new StringBuilder();
            var msg = userMessage.ToLowerInvariant();

            sb.AppendLine($"Hôm nay: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine();

            var now = DateTime.UtcNow;
            var searchWords = ExtractSearchWords(msg);
            var matchedProducts = new List<Product>();

            var matchedSupermarket = await FindSupermarketInMessage(msg);

            if (matchedSupermarket != null)
            {
                matchedProducts = await _db.Products
                    .Include(p => p.Supermarket)
                    .Where(p => p.IsActive && p.StockQuantity > 0 && p.ExpiryDate > now && p.SupermarketId == matchedSupermarket.Id)
                    .OrderByDescending(p => p.DiscountPercentage)
                    .Take(8)
                    .ToListAsync();

                if (matchedProducts.Count != 0)
                {
                    sb.AppendLine($"[DỮ LIỆU SẢN PHẨM CỦA '{matchedSupermarket.Name}' - Dùng để tham khảo, KHÔNG in ra]");
                    foreach (var p in matchedProducts)
                    {
                        sb.AppendLine($"  - Tên: {p.Name}, Giá: {p.DiscountedPrice:N0}đ (giảm {p.DiscountPercentage}%), HSD: {p.ExpiryDate:dd/MM/yyyy}");
                    }
                    sb.AppendLine();

                    result.Products = matchedProducts.Select(p => new ProductSuggestion
                    {
                        Id = p.Id, Name = p.Name, Slug = p.Slug,
                        Price = p.DiscountedPrice, OriginalPrice = p.OriginalPrice,
                        Discount = p.DiscountPercentage, Image = p.ImageUrl ?? "",
                        Supermarket = p.Supermarket?.Name ?? ""
                    }).ToList();
                }
                else
                {
                    sb.AppendLine($"[Không tìm thấy sản phẩm nào ở '{matchedSupermarket.Name}']");
                    sb.AppendLine();
                }

                result.Supermarkets =
                [
                    new SupermarketSuggestion
                    {
                        Id = matchedSupermarket.Id,
                        Name = matchedSupermarket.Name,
                        Slug = matchedSupermarket.Slug,
                        Logo = matchedSupermarket.LogoUrl,
                        Address = matchedSupermarket.Address,
                        Rating = matchedSupermarket.Rating
                    }
                ];

                result.ContextText = sb.ToString();
                return result;
            }

            if (!IsAskingAboutSupermarkets(msg) && searchWords.Count != 0)
            {
                matchedProducts = await _db.Products
                    .Include(p => p.Supermarket)
                    .Where(p => p.IsActive && p.StockQuantity > 0 && p.ExpiryDate > now)
                    .Where(p => searchWords.Any(w => p.Name.Contains(w)))
                    .OrderByDescending(p => p.DiscountPercentage)
                    .Take(8)
                    .ToListAsync();

                if (matchedProducts.Count != 0)
                {
                    sb.AppendLine("[DỮ LIỆU SẢN PHẨM - Dùng để tham khảo, KHÔNG in ra]");
                    foreach (var p in matchedProducts)
                    {
                        sb.AppendLine($"  - Tên: {p.Name}, Giá: {p.DiscountedPrice:N0}đ (giảm {p.DiscountPercentage}%), HSD: {p.ExpiryDate:dd/MM/yyyy}, Siêu thị: {p.Supermarket?.Name}");
                    }
                    sb.AppendLine();

                    result.Products = matchedProducts.Select(p => new ProductSuggestion
                    {
                        Id = p.Id, Name = p.Name, Slug = p.Slug,
                        Price = p.DiscountedPrice, OriginalPrice = p.OriginalPrice,
                        Discount = p.DiscountPercentage, Image = p.ImageUrl ?? "",
                        Supermarket = p.Supermarket?.Name ?? ""
                    }).ToList();
                }
                else
                {
                    var hotProducts = await _db.Products
                        .Include(p => p.Supermarket)
                        .Where(p => p.IsActive && p.StockQuantity > 0 && p.ExpiryDate > now)
                        .OrderByDescending(p => p.DiscountPercentage)
                        .Take(6)
                        .ToListAsync();

                    if (hotProducts.Count != 0)
                    {
                        sb.AppendLine("[DỮ LIỆU SẢN PHẨM HOT - Dùng để tham khảo, KHÔNG in ra]");
                        foreach (var p in hotProducts)
                        {
                            sb.AppendLine($"  - Tên: {p.Name}, Giá: {p.DiscountedPrice:N0}đ (giảm {p.DiscountPercentage}%), Siêu thị: {p.Supermarket?.Name}");
                        }
                        sb.AppendLine();

                        result.Products = hotProducts.Select(p => new ProductSuggestion
                        {
                            Id = p.Id, Name = p.Name, Slug = p.Slug,
                            Price = p.DiscountedPrice, OriginalPrice = p.OriginalPrice,
                            Discount = p.DiscountPercentage, Image = p.ImageUrl ?? "",
                            Supermarket = p.Supermarket?.Name ?? ""
                        }).ToList();
                    }
                }
            }

            if (IsAskingAboutSupermarkets(msg) || searchWords.Count == 0)
            {
                var supermarkets = await _db.Supermarkets
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.Rating)
                    .Take(20)
                    .ToListAsync();

                if (supermarkets.Count != 0)
                {
                    sb.AppendLine("[DỮ LIỆU SIÊU THỊ - Dùng để tham khảo, KHÔNG in ra]");
                    foreach (var s in supermarkets)
                    {
                        sb.AppendLine($"  - Tên: {s.Name}, Địa chỉ: {s.Address}, Đánh giá: {s.Rating}/5");
                    }
                    sb.AppendLine();

                    result.Supermarkets = supermarkets.Select(s => new SupermarketSuggestion
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Slug = s.Slug,
                        Logo = s.LogoUrl,
                        Address = s.Address,
                        Rating = s.Rating
                    }).ToList();
                }
            }

            if (IsAskingAboutNearby(msg))
            {
                var location = ExtractLocation(msg);

                if (user?.Identity?.IsAuthenticated == true && string.IsNullOrEmpty(location))
                {
                    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    var appUser = await _db.Users.FindAsync(userId);
                    if (appUser != null && !string.IsNullOrEmpty(appUser.Address))
                    {
                        location = appUser.Address;
                        sb.AppendLine($"Địa chỉ khách hàng: {appUser.Address}");
                    }
                }

                if (!string.IsNullOrEmpty(location))
                {
                    var nearby = await _db.Supermarkets
                        .Where(s => s.IsActive &&
                            ((s.Address != null && s.Address.Contains(location)) ||
                             (s.Name != null && s.Name.Contains(location))))
                        .Take(10)
                        .ToListAsync();

                    if (nearby.Count == 0)
                    {
                        var shortLoc = ExtractShortLocation(location);
                        nearby = await _db.Supermarkets
                            .Where(s => s.IsActive &&
                                ((s.Address != null && (s.Address.Contains(shortLoc) || s.Address.Contains(location))) ||
                                 (s.Name != null && (s.Name.Contains(shortLoc) || s.Name.Contains(location)))))
                            .Take(10)
                            .ToListAsync();
                    }

                    if (nearby.Count != 0)
                    {
                        sb.AppendLine("[DỮ LIỆU SIÊU THỊ - Dùng để tham khảo, KHÔNG in ra]");
                        foreach (var s in nearby)
                        {
                            sb.AppendLine($"  - Tên: {s.Name}, Địa chỉ: {s.Address}, Đánh giá: {s.Rating}/5");
                        }
                        sb.AppendLine();

                        result.Supermarkets = nearby.Select(s => new SupermarketSuggestion
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Slug = s.Slug,
                            Logo = s.LogoUrl,
                            Address = s.Address,
                            Rating = s.Rating
                        }).ToList();
                    }
                    else
                    {
                        var allSms = await _db.Supermarkets
                            .Where(s => s.IsActive)
                            .Take(20)
                            .ToListAsync();
                        if (allSms.Count != 0)
                        {
                            sb.AppendLine($"[DỮ LIỆU SIÊU THỊ - không tìm thấy ở '{location}', toàn bộ siêu thị]");
                            foreach (var s in allSms)
                            {
                                sb.AppendLine($"  - Tên: {s.Name}, Địa chỉ: {s.Address}, Đánh giá: {s.Rating}/5");
                            }
                            sb.AppendLine();

                            result.Supermarkets = allSms.Select(s => new SupermarketSuggestion
                            {
                                Id = s.Id,
                                Name = s.Name,
                                Slug = s.Slug,
                                Logo = s.LogoUrl,
                                Address = s.Address,
                                Rating = s.Rating
                            }).ToList();
                        }
                    }
                }
            }

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var appUser = await _db.Users.FindAsync(userId);
                if (appUser != null)
                {
                    sb.AppendLine($"THÔNG TIN KHÁCH HÀNG: Họ tên: {appUser.FullName}, Email: {appUser.Email}, Địa chỉ: {appUser.Address ?? "chưa cập nhật"}");
                }
            }

            result.ContextText = sb.ToString();
            return result;
        }

        private static List<string> ExtractSearchWords(string msg)
        {
            var words = msg.Split([' ', ',', '.', '?', '!', ':', ';', '-', '_', '/', '(', ')', '[', ']', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return words
                .Where(w => w.Length >= 3 && !StopWords.Contains(w))
                .Distinct()
                .Take(5)
                .ToList();
        }

        private async Task<Supermarket?> FindSupermarketInMessage(string msg)
        {
            var allSupermarkets = await _db.Supermarkets.Where(s => s.IsActive).ToListAsync();

            Supermarket? best = null;
            var bestScore = 0;

            foreach (var s in allSupermarkets)
            {
                var nameWords = s.Name.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var score = nameWords.Count(w => w.Length >= 3 && msg.Contains(w));
                if (score > bestScore)
                {
                    bestScore = score;
                    best = s;
                }
            }

            return bestScore >= 2 ? best : null;
        }

        private static bool IsAskingAboutSupermarkets(string msg)
        {
            return msg.Contains("siêu thị") || msg.Contains("cửa hàng") || msg.Contains("neargo")
                || msg.Contains("ở đâu") || msg.Contains("mua ở") || msg.Contains("market");
        }

        private static bool IsAskingAboutNearby(string msg)
        {
            return msg.Contains("gần") || msg.Contains("khu vực") || msg.Contains("quận")
                || msg.Contains("huyện") || msg.Contains("phường") || msg.Contains("xã")
                || msg.Contains("thành phố") || msg.Contains("tỉnh") || msg.Contains("địa chỉ")
                || msg.Contains("chỗ") || msg.Contains("giao hàng") || msg.Contains(" ở ")
                || msg.Contains("tại ");
        }

        private static string? ExtractLocation(string msg)
        {
            var keywords = new[] { "gần ", "khu vực ", "quận ", "huyện ", "phường ", "xã ",
                "thành phố ", "tỉnh ", "đường ", "tp. ", "ở ", "tại " };
            foreach (var kw in keywords)
            {
                var idx = msg.IndexOf(kw, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0)
                {
                    var after = msg[(idx + kw.Length)..].Trim();
                    var end = after.IndexOfAny(['?', '!', '.']);
                    var loc = end > 0 ? after[..end] : after;
                    loc = loc.Trim();
                    var stopWords = new[] { "không", "nhỉ", "ạ", "vậy", "thế", "có", "và", "hoặc" };
                    foreach (var sw in stopWords)
                    {
                        if (loc.EndsWith(" " + sw)) loc = loc[..^(sw.Length + 1)].Trim();
                        if (loc.StartsWith(sw + " ")) loc = loc[(sw.Length + 1)..].Trim();
                    }
                    return loc;
                }
            }
            return null;
        }

        private static string ExtractShortLocation(string location)
        {
            var parts = location.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[^1] : location;
        }
    }

    public class ChatbotContextResult
    {
        public string ContextText { get; set; } = "";
        public List<ProductSuggestion> Products { get; set; } = [];
        public List<SupermarketSuggestion> Supermarkets { get; set; } = [];
    }

    public class ProductSuggestion
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public double Discount { get; set; }
        public string Image { get; set; } = "";
        public string Supermarket { get; set; } = "";
    }

    public class SupermarketSuggestion
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Logo { get; set; }
        public string? Address { get; set; }
        public double Rating { get; set; }
    }
}
