using Microsoft.EntityFrameworkCore;
using NearGo.Data;
using NearGo.Models;

namespace NearGo.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrder(string userId, int supermarketId, string shippingAddress,
            string? customerName, string? customerPhone, string? note, int? voucherId = null, int loyaltyPointsUsed = 0)
        {
            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && c.Product.SupermarketId == supermarketId)
                .ToListAsync();

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var subTotal = cartItems.Sum(c => c.Product.DiscountedPrice * c.Quantity);
            decimal discountAmount = 0;
            decimal loyaltyDiscount = 0;

            if (voucherId.HasValue)
            {
                var voucher = await _context.Vouchers.FindAsync(voucherId.Value);
                if (voucher != null && voucher.IsActive && voucher.CurrentUsage < voucher.MaxUsage
                    && subTotal >= voucher.MinOrderAmount && voucher.ExpiryDate > DateTime.UtcNow)
                {
                    discountAmount = voucher.DiscountType == "Percentage"
                        ? Math.Min(subTotal * voucher.DiscountValue / 100, voucher.MaxDiscountAmount)
                        : Math.Min(voucher.DiscountValue, subTotal);

                    voucher.CurrentUsage++;
                }
            }

            if (loyaltyPointsUsed > 0)
            {
                var points = await _context.LoyaltyPoints
                    .Where(lp => lp.UserId == userId && lp.ExpiryDate > DateTime.UtcNow)
                    .SumAsync(lp => lp.Points);

                var canUse = Math.Min(loyaltyPointsUsed, points);
                loyaltyDiscount = canUse * 100;
            }

            var totalAmount = subTotal - discountAmount - loyaltyDiscount;
            if (totalAmount < 0) totalAmount = 0;

            var orderCode = $"NG{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

            var order = new Order
            {
                OrderCode = orderCode,
                CustomerId = userId,
                SupermarketId = supermarketId,
                SubTotal = subTotal,
                DiscountAmount = discountAmount,
                ShippingFee = 0,
                TotalAmount = totalAmount,
                VoucherId = voucherId,
                LoyaltyPointsUsed = loyaltyPointsUsed,
                LoyaltyDiscount = loyaltyDiscount,
                Status = "Pending",
                PaymentStatus = "Unpaid",
                ShippingAddress = shippingAddress,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                CustomerNote = note,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.DiscountedPrice,
                    TotalPrice = cartItem.Product.DiscountedPrice * cartItem.Quantity
                };
                _context.OrderItems.Add(orderItem);

                cartItem.Product.StockQuantity -= cartItem.Quantity;
                cartItem.Product.SoldCount += cartItem.Quantity;
            }

            _context.CartItems.RemoveRange(cartItems);

            var loyaltyPoints = new LoyaltyPoint
            {
                UserId = userId,
                Points = (int)(totalAmount / 1000),
                Source = "Purchase",
                Description = $"Mua hàng đơn {orderCode}",
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };
            _context.LoyaltyPoints.Add(loyaltyPoints);

            var notification = new Notification
            {
                UserId = userId,
                Title = "Đơn hàng thành công",
                Message = $"Đơn hàng {orderCode} đã được tạo thành công",
                Type = "Order",
                RelatedUrl = $"/customer/orders/detail?id={order.Id}",
                IsRead = false
            };
            _context.Notifications.Add(notification);

            var supermarketNotification = new Notification
            {
                UserId = userId,
                Title = "Đơn hàng mới",
                Message = $"Bạn nhận được đơn hàng mới {orderCode}",
                Type = "Order",
                RelatedUrl = $"/supermarket/orders/detail?id={order.Id}",
                IsRead = false
            };
            _context.Notifications.Add(supermarketNotification);

            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetUserOrders(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Supermarket)
                .Include(o => o.PaymentTransaction)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetSupermarketOrders(int supermarketId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .Include(o => o.PaymentTransaction)
                .Where(o => o.SupermarketId == supermarketId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderById(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Category)
                .Include(o => o.Customer)
                .Include(o => o.Supermarket)
                .Include(o => o.PaymentTransaction)
                .Include(o => o.Voucher)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<bool> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            if (status == "Delivered") order.DeliveredDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdatePaymentStatus(int orderId, string paymentStatus, string? transactionId = null)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            order.PaymentStatus = paymentStatus;
            order.PaymentDate = paymentStatus == "Paid" ? DateTime.UtcNow : order.PaymentDate;
            if (transactionId != null) order.TransactionId = transactionId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalRevenue(int supermarketId)
        {
            return await _context.Orders
                .Where(o => o.SupermarketId == supermarketId && o.PaymentStatus == "Paid" && o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetTotalOrders(int supermarketId)
        {
            return await _context.Orders
                .Where(o => o.SupermarketId == supermarketId && o.Status != "Cancelled")
                .CountAsync();
        }

        public async Task UpdateOrderTransaction(int orderId, string? sessionId, string paymentMethod)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.TransactionId = sessionId;
                order.PaymentMethod = paymentMethod;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Order>> GetOrdersBySessionId(string sessionId)
        {
            return await _context.Orders
                .Where(o => o.TransactionId == sessionId)
                .ToListAsync();
        }

        public async Task CreatePaymentTransaction(int orderId, string paymentMethod, string? transactionId, string? bankCode, decimal amount, string status, string? responseCode, string? responseMessage)
        {
            var transaction = new PaymentTransaction
            {
                OrderId = orderId,
                PaymentMethod = paymentMethod,
                TransactionId = transactionId,
                BankCode = bankCode,
                Amount = amount,
                Status = status,
                ResponseCode = responseCode,
                ResponseMessage = responseMessage,
                CreatedAt = DateTime.UtcNow,
                PaidAt = status == "Success" ? DateTime.UtcNow : null
            };
            _context.PaymentTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
    }
}
