using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hospital.models;

public interface ITokenService
{
    string CreateToken(UserDbo user);
}

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;


    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

   /* public string GenerateToken(UserDbo user)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, _configuration["JWT:ValidIssuer"]),
            new Claim(JwtRegisteredClaimNames.Aud, _configuration["JWT:ValidAudience"]),
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role) // Add role claim
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:ValidIssuer"],
            audience: _configuration["JWT:ValidAudience"],
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
   */

    public string CreateToken(UserDbo user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };


        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWT:Secret").Value!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(2),
            signingCredentials: cred
            );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
}
