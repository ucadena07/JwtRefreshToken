using JwtRefreshToken.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace JwtRefreshToken.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    }
}
