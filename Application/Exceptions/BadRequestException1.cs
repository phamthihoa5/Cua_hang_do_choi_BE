using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class BadRequestException1 : Exception
    {
        public bool Success { get; } = false;
        public string Details { get; }

        public BadRequestException1(string message, string details = "") : base(message)
        {
            Details = details;
        }
    }
  
}
