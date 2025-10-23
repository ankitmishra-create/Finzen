using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceManagement.Infrastructure.Interface
{
    public interface ILoggedInUser
    {
        Guid CurrentLoggedInUser();
    }
}
