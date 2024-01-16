namespace timer_app.Infrastructure.Exceptions
{
    public class UserUnauthorizedException : Exception
    {
        public readonly int UserId;

        public UserUnauthorizedException(int userId) : base($"User {userId} is not authorized to access the requested entity.") { 
            UserId = userId;
        }
    }
}
