

namespace TagerCom.Area.Identity.Controller
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    public class AccountsControllers : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration configuration;
        private readonly IRepository<UserOTP> _userOTP;

        public AccountsControllers(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender, SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration, IRepository<UserOTP> userOTP)
        {
            _signInManager = signInManager;
            this.configuration = configuration;
            _emailSender = emailSender;
            _userManager = userManager;
            _userOTP = userOTP;
        }

        // 🔹 Register new user and send confirmation email
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            ApplicationUser applicationuser = new()
            {
                Name = registerDTO.Name,
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
                City = registerDTO.City,
                street = registerDTO.street,
                PostalCode = registerDTO.PostalCode,
            };

            var result = await _userManager.CreateAsync(applicationuser, registerDTO.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate email confirmation link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationuser);
            var link = Url.Action("ConfirmEmail", "Accounts", new { Area = "Identity", token = token, UserId = applicationuser }, Request.Scheme);
            await _emailSender.SendEmailAsync(applicationuser.Email, "Confirm Your Email", $"<h1>Confirm your email by clicking <a href='{link}'>Here</a></h1>");

            return Ok(new { SuccMsg = "User created successfully. Please confirm your email." });
        }

        //Confirm user email using token
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string token, string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { msg = "Link expired, please resend confirmation email." });

            return Ok(new { msg = "Email confirmed successfully." });
        }

        //Login user and return access + refresh tokens
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.EmailOrUserName)
                  ?? await _userManager.FindByNameAsync(loginDTO.EmailOrUserName);

            if (user == null)
            {
                return NotFound(new NotificationDTO
                {
                    Msg = "Invalid username or password",
                    TraceID = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberME, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new { msg = "Too many attempts" });

                return NotFound(new { msg = "Invalid username or password" });
            }

            if (!user.EmailConfirmed)
                return BadRequest(new { msg = "Please confirm your email first." });

            if (!user.LockoutEnabled)
                return BadRequest(new { msg = $"You are blocked until {user.LockoutEnd}" });

            var roles = await _userManager.GetRolesAsync(user);

            var Claims = new List<Claim>() {
                new Claim(ClaimTypes.NameIdentifier,user.Id),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Email,user.Email)
            };

            foreach (var item in roles)
                Claims.Add(new Claim(ClaimTypes.Role, item));

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:key"] ?? " "));
            SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: configuration["JWT:issuer"],
                audience: configuration["JWT:audience"],
                claims: Claims,
                expires: DateTime.Now.AddMinutes(50),
                signingCredentials: signingCredentials
            );

            //Create new refresh token
            var refreshToken = GenerateRefreshToken(HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

            //Remove old expired refresh tokens
            user.RefreshTokens.RemoveAll(t => t.IsExpired);

            // Link new refresh token to user
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshToken.Token,
                expiresAt = token.ValidTo
            });
        }

        //Helper method to generate refresh token
        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        //Resend email confirmation link
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationDTO resendEmailConfirmationDTO)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationDTO.EmailOrUserName)
                        ?? await _userManager.FindByNameAsync(resendEmailConfirmationDTO.EmailOrUserName);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            if (user.EmailConfirmed)
                return BadRequest(new { msg = "Already confirmed!" });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);
            await _emailSender.SendEmailAsync(user.Email!, "Confirm Your Email!", $"<h1>Confirm your email by clicking <a href='{link}'>Here</a></h1>");

            return Ok(new { msg = "Email confirmation link sent successfully." });
        }

        //Send OTP for password reset
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO forgetPasswordDTO)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordDTO.EmailOrUserName)
                        ?? await _userManager.FindByNameAsync(forgetPasswordDTO.EmailOrUserName);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var OTPNumber = new Random().Next(1000, 9999);
            await _emailSender.SendEmailAsync(user.Email!, "Reset Password!", $"<h1>Reset password using {OTPNumber}. Don't share it!</h1>");

            await _userOTP.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTPNumber = OTPNumber.ToString(),
                ValidTo = DateTime.UtcNow.AddDays(1)
            });
            await _userOTP.CommitAsync();

            return Ok(new { msg = "OTP sent to your email successfully", userId = user.Id });
        }

        //Verify OTP before resetting password
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(resetPasswordDTO.ApplicationUserId);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var userOTP = (await _userOTP.GetAsync(e => e.ApplicationUserId == resetPasswordDTO.ApplicationUserId))
                .OrderBy(e => e.Id).LastOrDefault();

            if (userOTP is null)
                return NotFound();

            if (userOTP.OTPNumber != resetPasswordDTO.OTPNumber)
                return BadRequest(new { msg = "Invalid OTP" });

            if (DateTime.UtcNow > userOTP.ValidTo)
                return BadRequest(new { msg = "Expired OTP" });

            return Ok(new { msg = "OTP verified successfully", userId = user.Id });
        }

        //Set new password after OTP verification
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordDTO newPasswordDTO)
        {
            var user = await _userManager.FindByIdAsync(newPasswordDTO.ApplicationUserId);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPasswordDTO.Password);

            return Ok(new { msg = "Password changed successfully!" });
        }

        //Logout current user session
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account", new { area = "Identity" });
        }
    }
}
