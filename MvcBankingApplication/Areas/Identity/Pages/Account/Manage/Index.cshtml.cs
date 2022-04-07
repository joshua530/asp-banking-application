#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MvcBankingApplication.Models.Users;

namespace MvcBankingApplication.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string ImageUrl { get; set; }

        // TODO Image

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "username")]
            public string UserName { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "email")]
            public string Email { get; set; }
            [Required]
            [Display(Name = "first name")]
            public string FirstName { get; set; }
            [Required]
            [Display(Name = "last name")]
            public string LastName { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            ImageUrl = user.ImageUrl;

            Input = new InputModel
            {
                UserName = userName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
            }
            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
            }
            bool errorsExist = false;
            if (Input.Email != user.Email)
            {
                // email should be unique
                var userWithEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (userWithEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "that email is already in use, try another one");
                    errorsExist = true;
                }
                else
                {
                    user.Email = Input.Email;
                }
            }
            if (Input.UserName != user.UserName)
            {
                // username should be unique
                var userWithUsername = await _userManager.FindByNameAsync(Input.UserName);
                if (userWithUsername != null)
                {
                    ModelState.AddModelError("Input.UserName", "that username is already in use, try another one");
                    errorsExist = true;
                }
                else
                {
                    user.UserName = Input.UserName;
                }
            }
            if (errorsExist)
            {
                ImageUrl = user.ImageUrl;
                return Page();
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
