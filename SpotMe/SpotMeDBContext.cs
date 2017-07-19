namespace SpotMe
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SpotMeDBContext : DbContext
    {
        public SpotMeDBContext()
            : base("name=SpotMeDBContext")
        {
        }

        public virtual DbSet<Classifier> Classifiers { get; set; }
        public virtual DbSet<Exercise> Exercises { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
