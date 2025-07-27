using AutoMapper;
using DomainLayer.Models.IdentityModule;
using Shared.DataTransferObjects.IdentityDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.MappingProfiles
{
    class IdentityProfile : Profile
    {
        public IdentityProfile() 
        {
            CreateMap<Address,AddressDto>().ReverseMap();
        }
    }
}
