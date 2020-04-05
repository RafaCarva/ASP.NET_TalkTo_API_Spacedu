using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TalkTo_API.V1.Models;

namespace TalkTo_API.DataBase
{
    public class TalkToContext : IdentityDbContext<ApplicationUser>
    {
        public TalkToContext(DbContextOptions<TalkToContext> options) : base(options)
        {

        }

        public DbSet<Mensagem> Mensagem { get; set; }
        public DbSet<Token> Token { get; set; }
    }
}
