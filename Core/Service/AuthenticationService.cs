using AutoMapper;
using DomainLayer.Exceptions;
using DomainLayer.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ServiceAbstraction;
using Shared.DataTransferObjects.IdentityDtos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class AuthenticationService(UserManager<ApplicationUser> _userManager, IConfiguration _configuration, IMapper _mapper) : IAuthenticationService
    {
        public async Task<bool> CheckEmailAsync(string Email)
        {
            var User = await _userManager.FindByEmailAsync(Email);
            return User is not null;
        }


        public async Task<UserDto> GetCurrentUserAsync(string Email)
        {
            var User = await _userManager.FindByEmailAsync(Email) ?? throw new UserNotFoundException(Email);

            return new UserDto()
            {
                DisplayName = User.DisplayName,
                Email = User.Email,
                Token = await CreateTokenAsync(User)
            };
        }
        public async Task<AddressDto> GetCurrentUserAddressAsync(string Email)
        {
            var User = await _userManager.Users.Include(U => U.Address)
                           .FirstOrDefaultAsync(U => U.Email == Email) ?? throw new UserNotFoundException(Email);
                return _mapper.Map<Address, AddressDto>(User.Address);


        }
        public async Task<AddressDto> UpdateCurrentUserAddressAsync(string Email, AddressDto addressDto)
        {
            var User = await _userManager.Users.Include(U => U.Address)
                           .FirstOrDefaultAsync(U => U.Email == Email) ?? throw new UserNotFoundException(Email);
            if (User.Address is not null)
            {
                User.Address.FirstName = addressDto.FirstName;
                User.Address.LastName = addressDto.LastName;
                User.Address.City = addressDto.City;
                User.Address.Country = addressDto.Country;
                User.Address.Street = addressDto.Street;
            }
            else
            {
                User.Address = _mapper.Map<AddressDto, Address>(addressDto);
            }

            await _userManager.UpdateAsync(User);
            return _mapper.Map<AddressDto>(User.Address);
        }

        public async Task<UserDto> LoginAsync(LoginDto loginDto)
        {
            //check if email exist
            var User = await _userManager.FindByEmailAsync(loginDto.Email) ?? throw new UserNotFoundException(loginDto.Email);

            //check password
            var IsPasswordValid = await _userManager.CheckPasswordAsync(User, loginDto.Password);
            if (IsPasswordValid)
            {
                return new UserDto()
                {
                    DisplayName = User.DisplayName,
                    Email = User.Email,
                    Token = await CreateTokenAsync(User)
                };
            }
            else
            {
                throw new UnAuthorizedException();
            }


        }
        public async Task<UserDto> RegisterAsync(RegisterDto registerDto)
        {
            //manual map from dto to user

            var User = new ApplicationUser()
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserName = registerDto.UserName,
            };

            //create user

            var Result = await _userManager.CreateAsync(User, registerDto.Password);
            if (Result.Succeeded)
            {
                return new UserDto()
                {
                    DisplayName = User.DisplayName,
                    Email = User.Email,
                    Token = await CreateTokenAsync(User)
                };
            }
            else
            {
                var Errors = Result.Errors.Select(E => E.Description).ToList();
                throw new BadRequestException(Errors);
            }


        }


        private async Task<string> CreateTokenAsync(ApplicationUser user)
        {
            var Claims = new List<Claim>()
            {
                new(ClaimTypes.Email,user.Email!),
                new(ClaimTypes.Name,user.UserName!),
                new(ClaimTypes.NameIdentifier,user.Id!),
            };
            var Roles = await _userManager.GetRolesAsync(user);

            foreach (var role in Roles)
            {
                Claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var SecretKey = _configuration.GetSection("JWTOptions")["SecretKey"];
            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var Creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
            var Token = new JwtSecurityToken(
                issuer: _configuration.GetSection("JWTOptions")["Issuer"],
                audience: _configuration.GetSection("JWTOptions")["Audience"],
                claims: Claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: Creds
                );
            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
