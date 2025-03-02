using Microsoft.EntityFrameworkCore;

namespace SpiderForJobInCore.Model.EntityContext
{
    public class RecruitmentInformationContext : DbContext
    {
        public DbSet<RecruitmentInformationContext> RecruitmentInformations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }
    }
}
