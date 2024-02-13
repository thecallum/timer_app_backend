namespace timer_app.Domain
{
    public class Auth0User
    {
        public string Sub { get; set; }
        public string Nickname { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string UpdatedAt { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
    }
}
