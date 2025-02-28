namespace UserAuthService.Models.Auth
{
    public class ValidatePasswordRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
