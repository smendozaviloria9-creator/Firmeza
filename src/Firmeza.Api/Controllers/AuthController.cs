using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firmeza.Application.DTOs.Auth;
using Firmeza.Domain.Entities;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Firmeza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            return Unauthorized(new { mensaje = "Credenciales invalidas." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized(new { mensaje = "Credenciales invalidas." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerarToken(user, roles);

        return Ok(token);
    }

    [HttpPost("registro-cliente")]
    public async Task<ActionResult<LoginResponseDto>> RegistroCliente(RegistroClienteDto dto)
    {
        var existente = await _userManager.FindByEmailAsync(dto.Email);
        if (existente is not null)
        {
            return BadRequest(new { mensaje = "Ya existe un usuario registrado con ese correo." });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            NombreCompleto = dto.NombreCompleto
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { mensaje = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }

        await _userManager.AddToRoleAsync(user, Roles.Cliente);

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerarToken(user, roles);

        return Ok(token);
    }

    private LoginResponseDto GenerarToken(ApplicationUser user, IList<string> roles)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;
        var expiraMinutos = int.Parse(_configuration["Jwt:ExpiraMinutos"] ?? "120");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new("nombreCompleto", user.NombreCompleto)
        };

        foreach (var rol in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddMinutes(expiraMinutos);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return new LoginResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEn = expira,
            Email = user.Email ?? string.Empty,
            NombreCompleto = user.NombreCompleto,
            Roles = roles.ToList()
        };
    }
}
