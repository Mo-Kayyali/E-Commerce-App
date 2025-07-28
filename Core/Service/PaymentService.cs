using AutoMapper;
using DomainLayer.Contracts;
using DomainLayer.Exceptions;
using DomainLayer.Models.OrderModule;
using Microsoft.Extensions.Configuration;
using ServiceAbstraction;
using Shared.DataTransferObjects.BasketModuleDtos;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Product = DomainLayer.Models.ProductModule.Product;

namespace Service
{
    public class PaymentService(IConfiguration _configuration,IBasketRepository _basketRepository,IUnitOfWork _unitOfWork,IMapper _mapper) : IPaymentService
    {
        public async Task<BasketDto> CreateOrUpdatePaymentIntentAsync(string BasketId)
        {
            StripeConfiguration.ApiKey = _configuration["StripeSettings:SecretKey"];
            var Basket = await _basketRepository.GetBasketAsync(BasketId) ?? throw new BasketNotFoundException(BasketId);

            var ProductRepo = _unitOfWork.GetRepository<Product, int>();

            foreach(var item in Basket.Items)
            {
                var Product = await ProductRepo.GetByIdAsync(item.Id)?? throw new ProductNotFoundException(item.Id);
                item.Price=Product.Price;
            }
            ArgumentNullException.ThrowIfNull(Basket.deliveryMethodId);
            var DeliveryMethod =await _unitOfWork.GetRepository<DeliveryMethod, int>().GetByIdAsync(Basket.deliveryMethodId.Value) ?? throw new DeliveryMethodNotFoundException(Basket.deliveryMethodId.Value);
            Basket.shippingPrice = DeliveryMethod.Cost;

            var BasketAmount = (long)(Basket.Items.Sum(item => item.Quantity * item.Price) + DeliveryMethod.Cost) * 100;

            var PaymentService = new PaymentIntentService();
            if(Basket.paymentIntentId is null)
            {
                var Options = new PaymentIntentCreateOptions()
                {
                    Amount = BasketAmount,
                    Currency = "USD",
                    PaymentMethodTypes = ["card"]
                };
                var PaymentIntent = await PaymentService.CreateAsync(Options);
                Basket.paymentIntentId = PaymentIntent.Id;
                Basket.clientSecret = PaymentIntent.ClientSecret;
            }
            else
            {
                var Options = new PaymentIntentUpdateOptions() { Amount = BasketAmount };
                await PaymentService.UpdateAsync(Basket.paymentIntentId, Options);
            }

            await _basketRepository.CreateOrUpdateAsync(Basket);
            return _mapper.Map<BasketDto>(Basket);

        }
    }
}
