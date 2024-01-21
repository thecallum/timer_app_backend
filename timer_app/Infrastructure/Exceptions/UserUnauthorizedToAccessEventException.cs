namespace timer_app.Infrastructure.Exceptions
{
    public class UserUnauthorizedToAccessEventException : Exception
    {
        public readonly int UserId;

        public UserUnauthorizedToAccessEventException(int userId) : base($"User {userId} is not authorized to access the requested entity.")
        {
            UserId = userId;
        }
    }
}
