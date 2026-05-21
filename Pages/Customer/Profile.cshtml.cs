using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NearGo.Models;
using System.ComponentModel.DataAnnotations;

namespace NearGo.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public ProfileModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();
        public AppUser? UserProfile { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Họ tên là bắt buộc")]
            public string FullName { get; set; } = string.Empty;
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string? Phone { get; set; }
            public string? Address { get; set; }
        }

        public async Task OnGetAsync()
        {
            UserProfile = await _userManager.GetUserAsync(User);
            if (UserProfile != null)
            {
                Input.FullName = UserProfile.FullName;
                Input.Phone = UserProfile.PhoneNumber;
                Input.Address = UserProfile.Address;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage("/Auth/Login");

            user.FullName = Input.FullName;
            user.PhoneNumber = Input.Phone;
            user.Address = Input.Address;
            await _userManager.UpdateAsync(user);
            ViewData["Message"] = "Cập nhật hồ sơ thành công!";
            UserProfile = user;
            return Page();
        }
    }
}
