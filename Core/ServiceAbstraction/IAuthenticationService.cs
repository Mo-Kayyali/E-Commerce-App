using Shared.DataTransferObjects.IdentityDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction
{
    public interface IAuthenticationService
    {
        Task<UserDto> LoginAsync(LoginDto loginDto);
        Task<UserDto> RegisterAsync(RegisterDto registerDto);
        Task<bool> CheckEmailAsync(string Email);
        Task<AddressDto> GetCurrentUserAddressAsync(string Email);
        Task<AddressDto> UpdateCurrentUserAddressAsync(string Email,AddressDto addressDto);
        Task<UserDto> GetCurrentUserAsync(string Email);
        
    }
}
