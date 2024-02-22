namespace timer_app.Infrastructure.Exceptions
{
    public class UserUnauthorizedToAccessEventException : Exception
    {
        public readonly string UserId;

        public UserUnauthorizedToAccessEventException(string userId) : base($"User {userId} is not authorized to access the requested entity.")
        {
            UserId = userId;
        }
    }
}
