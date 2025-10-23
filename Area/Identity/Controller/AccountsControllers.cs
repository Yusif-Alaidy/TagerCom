using Microsoft.AspNetCore.Identity;

namespace TagerCom.Area.Identity.Controller
{
    [Route("api/[area]/[controller]")]
    [Area("Identity")]
    [ApiController]
    public class AccountsControllers : ControllerBase
    {
        #region Fields

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IRepository<UserOTP> userOTP;

        #endregion

        #region Constructore
        public AccountsControllers( UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IRepository<UserOTP> userOTP)
        {
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.userOTP = userOTP;
        }
        #endregion

        #region Register
        // 🔹 Register new user and send confirmation email
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            ApplicationUser applicationuser = new()
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
            };

            var result = await userManager.CreateAsync(applicationuser, registerDTO.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate email confirmation link
            var token = await userManager.GenerateEmailConfirmationTokenAsync(applicationuser);
            var link = Url.Action("ConfirmEmail", "Accounts", new { Area = "Identity", token = token, UserId = applicationuser }, Request.Scheme);
            await emailSender.SendEmailAsync(applicationuser.Email, "Confirm Your Email", $"<h1>Confirm your email by clicking <a href='{link}'>Here</a></h1>");

            return Ok(new { SuccMsg = "User created successfully. Please confirm your email." });
        }
        #endregion

        #region ConfirmEmail
        //Confirm user email using token
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string token, string UserId)
        {
            var user = await userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound();

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
                return BadRequest(new { msg = "Link expired, please resend confirmation email." });

            return Ok(new { msg = "Email confirmed successfully." });
        }

        //Login user and return access + refresh tokens
        [HttpPost("Login")]
        #endregion

        #region Login
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await userManager.FindByEmailAsync(loginDTO.EmailOrUserName)
                  ?? await userManager.FindByNameAsync(loginDTO.EmailOrUserName);

            if (user == null)
            {
                return NotFound(new NotificationDTO
                {
                    Msg = "Invalid username or password",
                    TraceID = Guid.NewGuid().ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }

            var result = await signInManager.PasswordSignInAsync(user, loginDTO.Password, loginDTO.RememberME, true);

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

            var roles = await userManager.GetRolesAsync(user);

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
            await userManager.UpdateAsync(user);

            return Ok(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = refreshToken.Token,
                expiresAt = token.ValidTo
            });
        }
        #endregion

        #region GenerateRefreshToken
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
        #endregion

        #region ResendEmailConfirmation
        //Resend email confirmation link
        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationDTO resendEmailConfirmationDTO)
        {
            var user = await userManager.FindByEmailAsync(resendEmailConfirmationDTO.EmailOrUserName)
                        ?? await userManager.FindByNameAsync(resendEmailConfirmationDTO.EmailOrUserName);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            if (user.EmailConfirmed)
                return BadRequest(new { msg = "Already confirmed!" });

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Account", new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);
            await emailSender.SendEmailAsync(user.Email!, "Confirm Your Email!", $"<h1>Confirm your email by clicking <a href='{link}'>Here</a></h1>");

            return Ok(new { msg = "Email confirmation link sent successfully." });
        }
        #endregion

        #region ForgetPassword
        //Send OTP for password reset
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO forgetPasswordDTO)
        {
            var user = await userManager.FindByEmailAsync(forgetPasswordDTO.EmailOrUserName)
                        ?? await userManager.FindByNameAsync(forgetPasswordDTO.EmailOrUserName);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var OTPNumber = new Random().Next(1000, 9999);
            await emailSender.SendEmailAsync(user.Email!, "Reset Password!", $"<h1>Reset password using {OTPNumber}. Don't share it!</h1>");

            await userOTP.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                OTPNumber = OTPNumber.ToString(),
                ValidTo = DateTime.UtcNow.AddDays(1)
            });
            await userOTP.CommitAsync();

            return Ok(new { msg = "OTP sent to your email successfully", userId = user.Id });
        }
        #endregion

        #region ResetPassword

        //Verify OTP before resetting password[HttpPost("ResetPassword")]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await userManager.FindByIdAsync(resetPasswordDTO.ApplicationUserId);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var otpRecords = (await userOTP.GetAsync(e => e.ApplicationUserId == resetPasswordDTO.ApplicationUserId))
                     ?? Enumerable.Empty<UserOTP>();

            var latestOtp = otpRecords
                    .OrderByDescending(e => e.Id) 
                    .FirstOrDefault();

            if (latestOtp is null)
                return NotFound(new { msg = "No OTP found for this user." });

            if (latestOtp.OTPNumber != resetPasswordDTO.OTPNumber)
                return BadRequest(new { msg = "Invalid OTP" });

            if (DateTime.UtcNow > latestOtp.ValidTo)
                return BadRequest(new { msg = "Expired OTP" });

            // latestOtp.IsUsed = true;
            // await userOTP.UpdateAsync(latestOtp); 
            // await userOTP.CommitAsync();

            // هنا نعيد نجاح التحقق ونعطي الـ userId للخطوة التالية (NewPassword)
            return Ok(new { msg = "OTP verified successfully", userId = user.Id });
        }


        #endregion

        #region NewPassword
        //Set new password after OTP verification
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordDTO newPasswordDTO)
        {
            var user = await userManager.FindByIdAsync(newPasswordDTO.ApplicationUserId);

            if (user is null)
                return NotFound(new { msg = "Invalid username or email" });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, newPasswordDTO.Password);

            return Ok(new { msg = "Password changed successfully!" });
        }
        #endregion

        #region LoginByGoogle
        [HttpGet]

        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction(nameof(Login));
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Try signing in with an external login
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            // If the user cannot log in, try finding them by email
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            if (email != null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new user if they do not exist
                    Random rand=new Random();
                    string randUser=rand.Next(1000,9999).ToString();
                    user = new ApplicationUser
                    {
                        UserName =$"{name.Replace(" ", "")}{randUser}" ,
                        Email = email
                    };
                    var createUserResult = await userManager.CreateAsync(user);
                    if (!createUserResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error creating user.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                // Ensure the external login is linked
                var existingLogins = await userManager.GetLoginsAsync(user);
                var hasGoogleLogin = existingLogins.Any(l => l.LoginProvider == info.LoginProvider);

                if (!hasGoogleLogin)
                {
                    var addLoginResult = await userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, "Error linking external login.");
                        return RedirectToAction(nameof(Login));
                    }
                }

                // Sign in the user
                await signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/");
            }

            return RedirectToAction(nameof(Login));
        }
        #endregion

    }
}
