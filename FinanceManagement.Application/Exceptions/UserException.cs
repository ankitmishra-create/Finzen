namespace FinanceManagement.Application.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }

    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string message) : base(message) { }
    }
}
