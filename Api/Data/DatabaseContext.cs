using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; set; }
}