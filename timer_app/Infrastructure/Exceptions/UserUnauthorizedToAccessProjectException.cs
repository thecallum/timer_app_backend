namespace timer_app.Infrastructure.Exceptions
{
    public class UserUnauthorizedToAccessProjectException : Exception
    {
        public readonly int UserId;

        public UserUnauthorizedToAccessProjectException(int userId) : base($"User {userId} is not authorized to access the requested entity.")
        {
            UserId = userId;
        }
    }
}
