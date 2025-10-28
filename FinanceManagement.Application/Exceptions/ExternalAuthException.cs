using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Application.Exceptions
{
    public class ExternalAuthException : Exception
    {
        public ExternalAuthException(string message) : base(message)
        {
            
        }
    }
}
