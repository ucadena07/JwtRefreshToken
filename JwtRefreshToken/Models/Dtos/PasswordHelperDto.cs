namespace JwtRefreshToken.Models.Dtos
{
    public class PasswordHelperDto
    {
        public byte[] passwordHash { get; set; }
        public byte[] passwordSalt { get; set; }
    }
}
