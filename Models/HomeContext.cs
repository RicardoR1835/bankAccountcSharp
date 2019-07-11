using Microsoft.EntityFrameworkCore;
 
namespace bankAccount_cSharp.Models
{
    public class HomeContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public HomeContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users {get;set;}
        public DbSet<Transaction> Transactions {get;set;}
    }
}