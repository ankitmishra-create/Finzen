namespace FinanceManagement.Application.Exceptions
{
    public class BudgetNotFoundException : Exception
    {
        public BudgetNotFoundException(string message) : base(message)
        {

        }
    }
}
