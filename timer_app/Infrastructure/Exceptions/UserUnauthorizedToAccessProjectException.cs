namespace timer_app.Infrastructure.Exceptions
{
    public class UserUnauthorizedToAccessProjectException : Exception
    {
        public readonly string UserId;

        public UserUnauthorizedToAccessProjectException(string userId) : base($"User {userId} is not authorized to access the requested entity.")
        {
            UserId = userId;
        }
    }
}
