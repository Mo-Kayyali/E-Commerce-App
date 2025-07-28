using AutoMapper;
using DomainLayer.Models.OrderModule;
using Shared.DataTransferObjects.IdentityDtos;
using Shared.DataTransferObjects.OrderDtos;

namespace Service.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<AddressDto, OrderAddress>().ReverseMap();

            CreateMap<Order, OrderToReturnDto>()
                .ForMember(d => d.DeliveryMethod, o => o.MapFrom(s => s.DeliveryMethod.ShortName))
                .ForMember(d => d.DeliveryCost, o => o.MapFrom(s => s.DeliveryMethod.Cost))
                .ForMember(d => d.Total, o => o.MapFrom(s => s.GetTotal()));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(d => d.PictureUrl, o => o.MapFrom<OrderItemPictureUrlResolver>())
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.ProductName));

            CreateMap<DeliveryMethod, DeliveryMethodDto>();
        }
    }
}
