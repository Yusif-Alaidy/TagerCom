using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using TagerCom.Models;
using TagerCom.Services;
using TagerCom.ViewModels;
using System.Security.Claims;
using TagerCom.Models;

namespace TagerCom.Area.Identity.Controller
{
    [Area("Identity")]
    [ApiController]
    [Route("api/Identity/[controller]")]
    public class AccountsController : ControllerBase
    {
        #region Fields

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly IRepository<UserOTP> userOTP;
        private readonly ApplicationDbContext context;
        private readonly TokenService tokenService;
        private readonly IRepository<RefreshToken> refreshToken;

        #endregion

        #region Constructore
        public AccountsController(ApplicationDbContext _context ,TokenService tokenService,IRepository<RefreshToken> refreshToken, UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, IRepository<UserOTP> userOTP)
        {
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.emailSender = emailSender;
            this.context = _context;
            this.tokenService = tokenService;
            this.refreshToken = refreshToken;
            this.userManager = userManager;
            this.userOTP = userOTP;
        }
        #endregion

        #region Register
        // 🔹 Register new user and send confirmation email
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            ApplicationUser applicationUser = new()
            {
                UserName = registerDTO.UserName,
                Email = registerDTO.Email,
            };

            var result = await userManager.CreateAsync(applicationUser, registerDTO.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate email confirmation link
            var token = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            //var link = Url.Action("ConfirmEmail", "Accounts", new { Area = "Identity", token = token, UserId = applicationUser }, Request.Scheme);
            var link = $"{Request.Scheme}://{Request.Host}/api/Identity/Accounts/ConfirmEmail?userId={applicationUser.Id}&token={encodedToken}";
            await emailSender.SendEmailAsync(applicationUser.Email, "Confirm Your Email", $"<h1>Confirm your email by clicking <a href='{link}'>Here</a></h1>");

            return Ok(new { SuccMsg = "User created successfully. Please confirm your email." });
        }
        #endregion
        
        #region ConfirmEmail
        //Confirm user email using token
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery]string userId, [FromQuery]string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
                return BadRequest(new { msg = "Link expired, please resend confirmation email." });

            return Ok(new { msg = "Email confirmed successfully." });
        }
    
        #endregion

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await userManager.FindByEmailAsync(model.EmailOrUserName)
                       ?? await userManager.FindByNameAsync(model.EmailOrUserName);

            if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { msg = "Invalid username or password" });

            //var oldTokens = await refreshToken.GetAsync(e=>e.UserId == user.Id);
            var oldToken = context.RefreshTokens.Where(e => e.UserId == user.Id);
            context.RefreshTokens.RemoveRange(oldToken);
            context.SaveChanges();

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(15),
                Created = DateTime.UtcNow,

            };
            context.RefreshTokens.Add(refreshToken);
            context.SaveChanges();

            // TokenService
            var accessToken = tokenService.GenerateAccessToken(claims);

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(15);
            
            return Ok(new
            {
                accessToken,
                access_token_expires_at = accessTokenExpiration.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                refreshToken = refreshToken.Token,
                refresh_token_expires_at = refreshToken.Expires.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            });
        }

        #endregion

        #region GenerateRefreshToken
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest refreshToken)
        {
            var storedToken = await context.RefreshTokens.FirstOrDefaultAsync(e => e.Token == refreshToken.RefreshToken);
            if (storedToken == null) return Unauthorized("Invalid refresh token");
            if (storedToken.IsExpired) return Unauthorized("Refresh Token Expired");
            var user = await userManager.FindByIdAsync(storedToken.UserId);
            if (user == null) return Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // TokenService
            var newAccessToken = tokenService.GenerateAccessToken(claims);

            return Ok(new { AccessToken = newAccessToken});
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
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            //var link = Url.Action("ConfirmEmail", "Accounts", new { Area = "Identity", token = token, UserId = applicationUser }, Request.Scheme);
            var link = $"{Request.Scheme}://{Request.Host}/api/Identity/Accounts/ConfirmEmail?userId={user.Id}&token={encodedToken}";
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

        

    }
}
