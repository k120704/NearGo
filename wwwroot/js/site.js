async function addToCart(productId, quantity) {
    quantity = quantity || 1;
    try {
        var resp = await fetch('/cart/cartapi?handler=add', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'productId=' + productId + '&quantity=' + quantity
        });
        var data = await resp.json();
        if (data.success) {
            Swal.fire({
                icon: 'success',
                title: 'Đã thêm vào giỏ hàng',
                text: 'Sản phẩm đã được thêm vào giỏ hàng của bạn',
                timer: 2000,
                showConfirmButton: false,
                toast: true,
                position: 'top-end'
            });
            var cartBadges = document.querySelectorAll('[x-text="cartCount"]');
            cartBadges.forEach(function(el) { el.textContent = data.count; });
        } else {
            if (data.message) {
                Swal.fire({ icon: 'warning', title: data.message, toast: true, position: 'top-end', showConfirmButton: false, timer: 2000 });
            } else {
                Swal.fire({ icon: 'warning', title: 'Vui lòng đăng nhập để mua hàng', toast: true, position: 'top-end', showConfirmButton: false, timer: 2000 });
            }
        }
    } catch(e) {
        Swal.fire({ icon: 'error', title: 'Có lỗi xảy ra', toast: true, position: 'top-end', showConfirmButton: false, timer: 2000 });
    }
}

async function toggleWishlist(productId) {
    try {
        var resp = await fetch('/wishlist?handler=Toggle', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: 'productId=' + productId
        });
        var data = await resp.json();
        if (data.liked) {
            Swal.fire({ icon: 'success', title: 'Đã thêm vào yêu thích', toast: true, position: 'top-end', showConfirmButton: false, timer: 1500 });
        } else {
            Swal.fire({ icon: 'info', title: 'Đã xóa khỏi yêu thích', toast: true, position: 'top-end', showConfirmButton: false, timer: 1500 });
        }
    } catch(e) {
        Swal.fire({ icon: 'error', title: 'Có lỗi xảy ra', toast: true, position: 'top-end', showConfirmButton: false, timer: 2000 });
    }
}

var connection = null;
function initSignalR() {
    if (typeof signalR === 'undefined') return;
    if (connection && connection.state === signalR.HubConnectionState.Connected) return;
    connection = new signalR.HubConnectionBuilder().withUrl('/notificationHub').build();
    connection.on('ReceiveNotification', function(title, message, url) {
        Swal.fire({ icon: 'info', title: title, text: message, toast: true, position: 'top-end', showConfirmButton: true, timer: 5000 });
        loadNotifications();
    });
    connection.start().catch(function(err) { });
}

async function loadNotifications() {
    try {
        var [countResp, listResp] = await Promise.all([
            fetch('/notifications?handler=UnreadCount'),
            fetch('/notifications?handler=List')
        ]);
        var countData = await countResp.json();
        updateUnreadBadge(countData.count);
        var notifData = await listResp.json();
        updateNotificationList(notifData);
    } catch(e) { }
}

function updateUnreadBadge(count) {
    var el = document.querySelector('[x-data]');
    if (el && window.Alpine) {
        Alpine.$data(el).unreadCount = count;
    }
}

function updateNotificationList(list) {
    var el = document.querySelector('[x-data]');
    if (el && window.Alpine) {
        Alpine.$data(el).notifList = list;
    }
}

function timeAgo(dateStr) {
    var now = new Date();
    var date = new Date(dateStr);
    var diff = Math.floor((now - date) / 1000);
    if (diff < 60) return 'Vài giây trước';
    if (diff < 3600) return Math.floor(diff / 60) + ' phút trước';
    if (diff < 86400) return Math.floor(diff / 3600) + ' giờ trước';
    if (diff < 2592000) return Math.floor(diff / 86400) + ' ngày trước';
    return date.toLocaleDateString('vi-VN');
}

document.addEventListener('DOMContentLoaded', function() {
    var toast = document.getElementById('toast-success');
    if (toast) {
        setTimeout(function() { toast.classList.add('opacity-0', 'translate-y-2'); setTimeout(function() { toast.remove(); }, 300); }, 3000);
    }
});
