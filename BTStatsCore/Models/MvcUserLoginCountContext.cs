using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace BTStatsCore.Models
{
    public class MvcUserLoginCountContext : DbContext
    {
        public MvcUserLoginCountContext(DbContextOptions<MvcUserLoginCountContext> options) : base(options)
        {

        }

        public DbSet<UserLoginCount> UserLoginCount { get; set; }
    }
}
