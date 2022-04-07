#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MvcBankingApplication.Models.Users;
using Myrmec;

namespace MvcBankingApplication.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _webHostEnv;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            IWebHostEnvironment webHostEnv)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _webHostEnv = webHostEnv;
        }

        public string ImageUrl { get; set; }

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
            [Display(Name = "user image")]
            public IFormFile UserImage { get; set; }
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

            ImageUrl = user.ImageUrl;

            if (errorsExist)
            {
                return Page();
            }

            if (Input.UserImage != null && Input.UserImage.Length > 0)
            {
                // reject invalid image files
                string imgType = ValidateImage(Input.UserImage);
                if (imgType == null)
                {
                    ModelState.AddModelError("Input.UserImage", "file uploaded is not a valid image. only jpg and png images are allowed");
                    return Page();
                }

                using (var memStream = new MemoryStream())
                {
                    await Input.UserImage.CopyToAsync(memStream);

                    // reject files > 2MB
                    if (memStream.Length < 2097152)
                    {

                        var wwwRoot = _webHostEnv.WebRootPath;
                        var userImgPath =
                            Path.Join(_config["Files:UserImagesRoot"],
                            Path.GetRandomFileName());

                        if (imgType == "jpg" || imgType == "jpeg")
                        {
                            userImgPath += ".jpg";
                        }
                        else
                        {
                            userImgPath += ".png";
                        }

                        var pathToSave = Path.Join(wwwRoot, userImgPath);

                        using (var stream = System.IO.File.Create(pathToSave))
                        {
                            await Input.UserImage.CopyToAsync(stream);

                            // delete previous image
                            if (user.ImageUrl != "/images/users/avatar.png")
                            {
                                string toDelete = Path.Join(wwwRoot, user.ImageUrl);
                                System.IO.File.Delete(toDelete);
                            }
                            user.ImageUrl = userImgPath;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Input.UserImage", "uploaded file is too large");
                    }
                }
            }

            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }

        /// <returns>
        /// array of 20 bytes containing file head data
        /// </returns>
        private static byte[] ReadFileHead(IFormFile file)
        {
            using (var fs = new BinaryReader(file.OpenReadStream()))
            {
                byte[] head = new byte[20];
                fs.Read(head, 0, 20);
                return head;
            }
        }

        /// <summary>
        /// sniffs file head to ensure it is a valid image.
        /// 
        /// <para>Supported file types:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>jpg,jpeg (image/jpeg)</description>
        /// </item>
        /// <item>
        /// <description>png (image/png)</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns>
        /// the image type found if image is valid, else null
        /// </returns>
        private static string ValidateImage(IFormFile file)
        {
            Sniffer sniffer = new Sniffer();
            var validFiles = new List<Record>
            {
                new Record("jpg,jpeg", "ff,d8,ff,db"),
                new Record("jpg,jpeg", "FF,D8,FF,DB"),
                new Record("jpg,jpeg", "FF,D8,FF,E0,00,10,4A,46,49,46,00,01"),
                new Record("jpg,jpeg", "FF,D8,FF,EE"),
                new Record("jpg,jpeg", "FF,D8,FF,E1,??,??,45,78,69,66,00,00"),
                new Record("png", "89,50,4e,47,0d,0a,1a,0a")
            };
            sniffer.Populate(validFiles);
            byte[] fileHead = ReadFileHead(file);
            var results = sniffer.Match(fileHead);
            if (results.Count > 0)
            {
                return results[0];
            }
            return null;
        }
    }
}
