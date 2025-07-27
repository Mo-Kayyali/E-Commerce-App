using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.OrderModule;
using DomainLayer.Models.ProductModule;
using Service.Specifications;
using Service.Specifications.OrderModuleSpecificatins;
using ServiceAbstraction;
using Shared.DataTransferObjects.IdentityDtos;
using Shared.DataTransferObjects.OrderDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class OrderService(IMapper _mapper,IBasketRepository _basketRepository,IUnitOfWork _unitOfWork) : IOrderService
    {
        public async Task<OrderToReturnDto> CreateOrder(OrderDto orderDto, string Email)
        {
            var OrderAddress = _mapper.Map<AddressDto, OrderAddress>(orderDto.ShipToAddress);

            var Basket = await _basketRepository.GetBasketAsync(orderDto.BasketId) 
                ?? throw new BasketNotFoundException(orderDto.BasketId);

            ArgumentNullException.ThrowIfNullOrEmpty(Basket.paymentIntentId);

            var OrderRepo = _unitOfWork.GetRepository<Order, Guid>();
            var OrderSpec = new OrderWithPaymentIntentIdSpecifications(Basket.paymentIntentId);
            var ExistingOrder = await OrderRepo.GetByIdAsync(OrderSpec);
            if (ExistingOrder is not null) OrderRepo.Remove(ExistingOrder);

            List<OrderItem> OrderItems = [];

            var ProductRepo = _unitOfWork.GetRepository<Product, int>();

            foreach (var item in Basket.Items)
            {
                var Product = await ProductRepo.GetByIdAsync(item.Id)
                    ?? throw new ProductNotFoundException(item.Id);

                OrderItems.Add(CreateOrderItem(item, Product));
            }

            var DeliveryMethod = await _unitOfWork.GetRepository<DeliveryMethod, int>().GetByIdAsync(orderDto.DeliveryMethodId)
                ?? throw new DeliveryMethodNotFoundException(orderDto.DeliveryMethodId);

            var SubTotal = OrderItems.Sum(I => I.Quantity * I.Price);

            var Order = new Order(Email, OrderAddress, DeliveryMethod, OrderItems, SubTotal,Basket.paymentIntentId);

            await OrderRepo.AddAsync(Order);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<Order,OrderToReturnDto>(Order);
        }

        private static OrderItem CreateOrderItem(DomainLayer.Models.BasketModule.BasketItem item, Product Product)
        {
            return new OrderItem()
            {
                Product = new ProductItemOrdered() { ProductId = Product.Id, PictureUrl = Product.PictureUrl, ProductName = Product.Name },
                Price = Product.Price,
                Quantity = item.Quantity
            };
        }

        public async Task<IEnumerable<DeliveryMethodDto>> GetDeliveryMethodsAsync()
        {
            var DeliveryMethods = await _unitOfWork.GetRepository<DeliveryMethod, int>().GetAllAsync();
            return _mapper.Map<IEnumerable<DeliveryMethod>,IEnumerable<DeliveryMethodDto>>(DeliveryMethods);
        }

        public async Task<IEnumerable<OrderToReturnDto>> GetAllOrdersAsync(string Email)
        {
            var Spec = new OrderSpecifications(Email);
            var Orders = await _unitOfWork.GetRepository<Order, Guid>().GetAllAsync(Spec);
            return _mapper.Map<IEnumerable<Order>,IEnumerable<OrderToReturnDto>>(Orders);
        }

        public async Task<OrderToReturnDto> GetOrderByIdAsync(Guid Id)
        {
            var Spec = new OrderSpecifications(Id);
            var Order = await _unitOfWork.GetRepository<Order, Guid>().GetByIdAsync(Spec);
            return _mapper.Map<Order, OrderToReturnDto>(Order);
        }
        
    }
}
