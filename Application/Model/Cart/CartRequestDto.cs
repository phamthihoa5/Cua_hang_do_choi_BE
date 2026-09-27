using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Cart
{
    public class CartRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}