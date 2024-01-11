using Microsoft.EntityFrameworkCore.Design;

namespace cy.jinius.eventhorizon.tests.Models;

public class TodoReadDbContextFactory : IDesignTimeDbContextFactory<TodoReadDbContext>
{
    public TodoReadDbContext CreateDbContext(string[] args)
    {
        //string readConnectionString = "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

        //var optionsBuilder = new DbContextOptionsBuilder<TodoReadDbContext>();
        //optionsBuilder.UseNpgsql(readConnectionString);

        //return new TodoReadDbContext(optionsBuilder.Options);

        throw new NotImplementedException();
    }
}