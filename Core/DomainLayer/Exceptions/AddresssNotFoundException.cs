using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Exceptions
{
    public sealed class AddresssNotFoundException(string UserName) : NotFoundException($"User {UserName} Has No Address")
    {
    }
}
