namespace PhotoGallery.Services
{
    public class TokenBuilder(IConfiguration configuration) : ITokenBuilder
    {
        private readonly IConfiguration _configuration = configuration;

        public string GenerateAccessToken(IdentityUser user, string Role, DateTime expiry)
        {
            List<Claim> authClaims =
            [
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("UserId", user.Id),
                new Claim(ClaimTypes.Expiration, expiry.ToString()),
                new Claim("Expiration",expiry.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // One User for One Role
                new Claim(ClaimTypes.Role, Role),
            ];

            SymmetricSecurityKey authSigningKey = new(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? ""));
            JwtSecurityToken token = new(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                expires: expiry
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string access_token)
        {

            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"] ?? "")),
                ValidateLifetime = false,
            };

            JwtSecurityTokenHandler tokenHandler = new();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(access_token, tokenValidationParameters, out SecurityToken securityToken);
            return securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
                ? throw new SecurityTokenException("Invalid token")
                : principal;
        }
    }
}
