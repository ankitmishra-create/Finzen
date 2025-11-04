namespace FinanceManagement.Infrastructure.Persistence.External.Exceptions
{
    public class ResponseNotFoundException : Exception
    {
        public ResponseNotFoundException(string message) : base(message)
        {

        }
    }
}
