namespace PhotoGallery.Controllers.Auth
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(UserManager<IdentityUser> userManager,
        PhotoGalleryDbContext context,
        ILogger<AuthController> logger,
        ITokenBuilder tokenBuilder) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("access-token")]
        [EndpointSummary("Access Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(LoginModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            IdentityUser? user = await userManager.FindByNameAsync(model.Username);
            user ??= await userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                return ResponseHelper.NotFound_Request(null, new DefaultResponseMessageModel
                {
                    EN = "Invalid username or invalid email. User not found.",
                    MM = "အသုံးပြုသူအမည် (သို့) Email မှားနေပါသည်။ အသုံးပြုသူ အချက်အလက် ရှာမတွေ့ပါ။"
                });
            }

            if (await userManager.CheckPasswordAsync(user, model.Password))
            {
                IList<string> role = await userManager.GetRolesAsync(user);

                DateTime expiry = DateTime.Now.AddDays(1);
                DateTime refreshTokeExpiry = DateTime.Now.AddDays(7);
                string accessToken = tokenBuilder.GenerateAccessToken(user, role[0], expiry);
                string refreshToken = tokenBuilder.GenerateRefreshToken();

                TokenClaim? tokeClaim = await context.TokenClaims.FindAsync(user.Id);
                if (tokeClaim != null)
                {
                    tokeClaim.AccessToken = accessToken;
                    tokeClaim.RefreshToken = refreshToken;
                    tokeClaim.TokenExpiry = refreshTokeExpiry;
                }
                else
                {
                    TokenClaim newTokenClaim = new()
                    {
                        UserId = user.Id,
                        RefreshDate = DateTime.Now,
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        TokenExpiry = refreshTokeExpiry
                    };
                    _ = context.TokenClaims.Add(newTokenClaim);
                }

                _ = await context.SaveChangesAsync();

                logger.LogInformation("Access token generated! [UserId:{id}] [UserName:{userName}]", user.Id,
                    user.UserName);

                return ResponseHelper.OK_Result(
                    new
                    {
                        access_token = accessToken,
                        refresh_token = refreshToken,
                        user = new
                        {
                            user.Id,
                            user.UserName,
                            user.Email,
                            user.PhoneNumber,
                            user_role = role[0].ToLower()
                        }
                    },
                    new DefaultResponseMessageModel
                    { EN = "Successfully Generate Access Token.", MM = "Successfully Generate Access Token." });
            }

            logger.LogInformation("Unauthorized request [UserId:{Id}] [UserName:{userName}]", user.Id, user.UserName);

            return ResponseHelper.Unauthorized_Request(null, new DefaultResponseMessageModel
            {
                EN = "The username or password is invalid. Please try again.",
                MM = "အသုံးပြုသူအမည် သို့မဟုတ် စကားဝှက်သည် မမှန်ကန်ပါ။ ထပ်စမ်းကြည့်ပါ။"
            });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromForm] RefreshTokenModel model)
        {
            string accessToken = model.Access_Token;
            string refreshToken = model.Refresh_Token;

            ClaimsPrincipal principal;
            try
            {
                principal = tokenBuilder.GetPrincipalFromExpiredToken(accessToken);
            }
            catch
            {
                return ResponseHelper.Unauthorized_Request(null, new DefaultResponseMessageModel
                {
                    EN = "Invalid access token.",
                    MM = "Access token မှားနေပါသည်။"
                });
            }

            IdentityUser? user = await userManager.FindByIdAsync(principal.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value);

            if (user == null)
                return ResponseHelper.NotFound_Request(null, new DefaultResponseMessageModel
                {
                    EN = "User not found.",
                    MM = "အသုံးပြုသူ ရှာမတွေ့ပါ။"
                });

            TokenClaim? tokenClaim = await context.TokenClaims.FindAsync(user.Id);
            if (tokenClaim == null || tokenClaim.RefreshToken != refreshToken || tokenClaim.TokenExpiry < DateTime.Now)
            {
                return ResponseHelper.Unauthorized_Request(null, new DefaultResponseMessageModel
                {
                    EN = "Invalid or expired refresh token.",
                    MM = "Refresh token မမှန်ပါ သို့မဟုတ် သက်တမ်းကုန်သွားပါပြီ။"
                });
            }

            // Generate new tokens
            IList<string> roles = await userManager.GetRolesAsync(user);
            DateTime newExpiry = DateTime.Now.AddDays(1);        
            DateTime newRefreshExpiry = DateTime.Now.AddDays(7);    

            string newAccessToken = tokenBuilder.GenerateAccessToken(user, roles[0], newExpiry);
            string newRefreshToken = tokenBuilder.GenerateRefreshToken();

            // Update DB
            tokenClaim.AccessToken = newAccessToken;
            tokenClaim.RefreshToken = newRefreshToken;
            tokenClaim.TokenExpiry = newRefreshExpiry;
            tokenClaim.RefreshDate = DateTime.Now;
            await context.SaveChangesAsync();

            return ResponseHelper.OK_Result(new
            {
                access_token = newAccessToken,
                refresh_token = newRefreshToken,
                expiration = 1, 
                user = new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.PhoneNumber,
                    user_role = roles[0].ToLower()
                }
            },
            new DefaultResponseMessageModel
            {
                EN = "Successfully refreshed token.",
                MM = "Token ကို အောင်မြင်စွာ refresh လုပ်ပြီးပါပြီ။"
            });
        }


        [HttpPost("revoke-token/{username}")]
        [EndpointSummary("Revoke Token")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(LoginModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Revoke(string username)
        {
            IdentityUser? user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return ResponseHelper.NotFound_Request(null, new DefaultResponseMessageModel
                {
                    EN = "Invalid username or invalid email. User not found.",
                    MM = "အသုံးပြုသူအမည် (သို့) Email မှားနေပါသည်။ အသုံးပြုသူ အချက်အလက် ရှာမတွေ့ပါ။"
                });
            }

            TokenClaim? tokenClaim = await context.TokenClaims.FindAsync(user.Id);
            if (tokenClaim != null)
            {
                tokenClaim.RefreshDate = DateTime.Now;
                tokenClaim.AccessToken = null;
                tokenClaim.RefreshToken = null;
                tokenClaim.TokenExpiry = null;
                context.Entry(tokenClaim).State = EntityState.Modified;
            }

            _ = await context.SaveChangesAsync();

            logger.LogInformation("Revoke Token successfully! [UserId:{userId}]", user.Id);

            return ResponseHelper.OK_Result(
                new
                {
                    user
                },
                new DefaultResponseMessageModel
                {
                    EN = "Token Revoke Successfully",
                    MM = "Token Revoke Successfully"
                });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [EndpointSummary("Register")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                if (model.Password != model.ConfirmPassword)
                {
                    return ResponseHelper.Bad_Request(null, new DefaultResponseMessageModel
                    {
                        EN = "Password and ConfirmPassword do not match.",
                        MM = "Password နှင့် ConfirmPassword မကိုက်ညီပါ။"
                    });
                }

                // Check if username or email already exists
                IdentityUser? existingByName = await userManager.FindByNameAsync(model.Username);
                IdentityUser? existingByEmail = await userManager.FindByEmailAsync(model.Email);
                if (existingByName != null || existingByEmail != null)
                {
                    return ResponseHelper.Bad_Request(null, new DefaultResponseMessageModel
                    {
                        EN = "Username or Email already exists.",
                        MM = "အသုံးပြုသူအမည် သို့မဟုတ် Email သည် ရှိပြီးသား ဖြစ်သည်။"
                    });
                }

                IdentityUser user = new()
                {
                    UserName = model.Username,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                };

                IdentityResult createResult = await userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    string errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return ResponseHelper.Bad_Request(null, new DefaultResponseMessageModel
                    {
                        EN = $"Unable to create user: {errors}",
                        MM = "အသုံးပြုသူ ဖန်တီး၍ မရပါ။"
                    });
                }

                // Try to assign a default role "User" if it exists in the database
                string defaultRole = "User";
                bool roleExists = await context.AspNetRoles.AnyAsync(r => r.NormalizedName == defaultRole.ToUpper());
                if (roleExists)
                {
                    // Swallow role-add failure but log it
                    IdentityResult roleAddResult = await userManager.AddToRoleAsync(user, defaultRole);
                    if (!roleAddResult.Succeeded)
                    {
                        logger.LogWarning("User created but adding default role failed. [UserId:{Id}] {Errors}", user.Id,
                            string.Join(", ", roleAddResult.Errors.Select(e => e.Description)));
                    }
                }

                // Generate tokens for the newly created user (mirror Login behaviour)
                IList<string> roles = await userManager.GetRolesAsync(user);
                string roleName = roles.Count > 0 ? roles[0] : defaultRole.ToLower();

                DateTime expiry = DateTime.Now.AddDays(1);
                DateTime refreshTokeExpiry = DateTime.Now.AddDays(7);
                string accessToken = tokenBuilder.GenerateAccessToken(user, roleName, expiry);
                string refreshToken = tokenBuilder.GenerateRefreshToken();

                TokenClaim newTokenClaim = new()
                {
                    UserId = user.Id,
                    RefreshDate = DateTime.Now,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiry = refreshTokeExpiry
                };
                _ = context.TokenClaims.Add(newTokenClaim);
                _ = await context.SaveChangesAsync();

                logger.LogInformation("User registered and tokens generated. [UserId:{id}] [UserName:{userName}]", user.Id, user.UserName);

                return ResponseHelper.OK_Result(
                    new
                    {
                        access_token = accessToken,
                        refresh_token = refreshToken,
                        user = new
                        {
                            user.Id,
                            user.UserName,
                            user.Email,
                            user.PhoneNumber,
                            user_role = roleName.ToLower()
                        }
                    },
                    new DefaultResponseMessageModel
                    {
                        EN = "User registered successfully.",
                        MM = "အသုံးပြုသူအား အောင်မြင်စွာ မှတ်ပုံတင်ပြီးပါပြီ။"
                    });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Register failed for Username:{userName}", model?.Username);
                return ResponseHelper.InternalServerError_Request(null, new DefaultResponseMessageModel
                {
                    EN = "An error occurred while registering user.",
                    MM = "အသုံးပြုသူ မှတ်ပုံတင်စဉ် အမှားတစ်ခု ဖြစ်ပွားခဲ့သည်။"
                });
            }
        }

        [HttpGet("status")]
        [EndpointSummary("Status")]
        public async Task<IActionResult> GetResultAsync()
        {
            ClaimsIdentity? claimsIdentity = User.Identity as ClaimsIdentity;
            await Task.Delay(1000);
            return User.Identity.IsAuthenticated
                ? Ok(new DefaultResponseModel
                {
                    Success = true,
                    Code = StatusCodes.Status200OK,
                    Data = new
                    {
                        id = User.Identity.Name,
                        userName = claimsIdentity?.FindFirst(ClaimTypes.GivenName)?.Value,
                        role = claimsIdentity?.FindFirst(ClaimTypes.Role)?.Value,
                        time = DateTime.Now
                    },
                    Message = new DefaultResponseMessageModel("authorized", "")
                })
                : BadRequest(new DefaultResponseModel
                {
                    Success = true,
                    Code = StatusCodes.Status400BadRequest,
                    Message = new DefaultResponseMessageModel("Unauthorized", "")
                });
        }

        [HttpGet("check-token")]
        [EndpointSummary("Check Token Authorization")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DefaultResponseModel), StatusCodes.Status401Unauthorized)]
        public IActionResult CheckToken()
        {
            try
            {
                var userName = User.Identity?.Name;
                var userId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                var expiration = User.Claims.FirstOrDefault(c => c.Type == "Expiration")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return ResponseHelper.Unauthorized_Request(null, new DefaultResponseMessageModel
                    {
                        EN = "Unauthorized: Invalid or expired token.",
                        MM = "ခွင့်ပြုချက် မရှိပါ။ Token သက်တမ်းကုန်သွားပြီး ဖြစ်နိုင်သည်။"
                    });
                }

                return ResponseHelper.OK_Result(
                    new
                    {
                        isAuthorized = true,
                        user = new
                        {
                            userId,
                            userName,
                            role,
                            expiration
                        }
                    },
                    new DefaultResponseMessageModel
                    {
                        EN = "Token is valid and authorized.",
                        MM = "Token သက်တမ်း မကုန်သေးပါ။ အသုံးပြုခွင့် ရှိသည်။"
                    });
            }
            catch (Exception)
            {
                return ResponseHelper.Unauthorized_Request(null, new DefaultResponseMessageModel
                {
                    EN = "Unauthorized or expired token.",
                    MM = "Token သက်တမ်းကုန် သို့မဟုတ် မမှန်ပါ။"
                });
            }
        }

    }
}
