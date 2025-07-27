using Shared.DataTransferObjects.IdentityDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects.OrderDtos
{
    public class OrderToReturnDto
    {
        public Guid Id { get; set; }
        public string BuyerEmail { get; set; } = default!;
        public DateTimeOffset OrderDate { get; set; }
        public ICollection<OrderItemDto> Items { get; set; } = [];
        public AddressDto ShipToAddress { get; set; } = default!;
        public string DeliveryMethod { get; set; } = default!;
        public decimal DeliveryCost { get; set; }
        public string Status { get; set; } = default!;
        public decimal SubTotal { get; set; }

        //[NotMapped]
        //public decimal Total { get => SubTotal + DeliveryMethod.Price; }

        public decimal Total { get; set; } 
    }
}
