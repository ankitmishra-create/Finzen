namespace FinanceManagement.Application.Exceptions
{
    public class CategoryNotFoundException : Exception
    {
        public CategoryNotFoundException(string message) : base(message) { }
    }

    public class CategoryAlreadyExistException : Exception
    {
        public CategoryAlreadyExistException(string message) : base(message) { }
    }
}
