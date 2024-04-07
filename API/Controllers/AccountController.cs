using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;

    public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _mapper = mapper;
    }

    [HttpPost("register")] // POST: api/account/register
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO)
    {

        if (await UserExists(registerDTO.UserName)) return BadRequest("Username is taken");

        var user = _mapper.Map<AppUser>(registerDTO);

        user.UserName = registerDTO.UserName.ToLower();
       
        var result = await _userManager.CreateAsync(user, registerDTO.Password);

        if (!result.Succeeded) return BadRequest(result.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, "Member");

        if (!roleResult.Succeeded) return BadRequest(result.Errors);

        return new UserDTO
        {
            UserName = user.UserName,
            Token = await _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO)
    {
        var user = await _userManager.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(user => user.UserName == loginDTO.UserName.ToLower());

        if (user == null) return Unauthorized("invalid username");

        var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);

        if (!result) return Unauthorized("Invalid password");

        return new UserDTO
        {
            UserName = user.UserName,
            Token = await _tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(photo => photo.IsMain)?.Url,
            KnownAs = user.KnownAs,
            Gender = user.Gender
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _userManager.Users.AnyAsync(user => user.UserName == username.ToLower());
    }
}
