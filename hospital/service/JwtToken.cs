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
