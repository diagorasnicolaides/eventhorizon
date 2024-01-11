using Microsoft.EntityFrameworkCore.Design;

namespace Cy.Jinius.EventHorizon.Write;

public class WriteDbContextFactory : IDesignTimeDbContextFactory<WriteDbContext>
{
    public WriteDbContext CreateDbContext(string[] args)
    {
        //    string readConnectionString = "PORT = 5432; HOST = 127.0.0.1; TIMEOUT = 15; POOLING = True; MINPOOLSIZE = 1; MAXPOOLSIZE = 100; COMMANDTIMEOUT = 20; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'";

        //    var optionsBuilder = new DbContextOptionsBuilder<WriteDbContext>();
        //    optionsBuilder.UseNpgsql(readConnectionString);

        //    return new WriteDbContext(optionsBuilder.Options);
        //}

        throw new NotImplementedException();
    }
}