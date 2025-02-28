namespace UserAuthService.Models.Auth
{
    public class ApiKey
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Key { get; set; }
        public required string ClientId { get; set; }
        public required DateTime ValidUntil { get; set; }
    }
}