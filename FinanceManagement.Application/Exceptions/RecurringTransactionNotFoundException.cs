using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Application.Exceptions
{
    internal class RecurringTransactionNotFoundException : Exception
    {
        public RecurringTransactionNotFoundException(string message) :base(message)
        {
            
        }
    }
}
