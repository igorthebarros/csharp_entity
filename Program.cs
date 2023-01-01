using EntityFC.Data;
using EntityFC.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

#region DATABASE
//EnsureCreatedAndDeleted();
//FixEnsureCreatedGap();
//HealthCheckDatabase();
//ManageDatabaseConnectionState(true);
//ManageDatabaseConnectionState(false);
//ExecuteSQL();
//SQLInjection();
//GetAllMigrations();
//GetAllPendingMigrations();
//ApplyMigrationOnExecution();
//GetAllMigrations();
//GetAllAppliedMigrations();
//GetDatabaseScript();

static void GetDatabaseScript()
{
    using var db = new Context();
    var script = db.Database.GenerateCreateScript();

    Console.WriteLine(script);
}

static void ApplyMigrationOnExecution()
{
    using var db = new Context();
    //Use this on Context
    db.Database.Migrate();
}

static void GetAllMigrations()
{
    using var db = new Context();
    var migrations = db.Database.GetMigrations();
    Console.WriteLine($"Total: {migrations.Count()}");

    foreach (var migration in migrations)
    {
        Console.WriteLine($"Migration: {migration}");
    }

}

static void GetAllAppliedMigrations()
{
    using var db = new Context();
    var migrations = db.Database.GetAppliedMigrations();
    Console.WriteLine($"Total: {migrations.Count()}");

    foreach (var migration in migrations)
    {
        Console.WriteLine($"Migration: {migration}");
    }
}

static void GetAllPendingMigrations()
{
    using var db = new Context();
    var pendingMigrations = db.Database.GetPendingMigrations();

    Console.WriteLine($"Total: {pendingMigrations.Count()}");

    foreach (var migration in pendingMigrations)
    {
        Console.WriteLine($"Migration: {migration}");
    }
}

static void SQLInjection()
{
    using var db = new Context();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();

    db.Departaments.AddRange(
        new Departament
        {
            Description = "Dpt 01"
        },
        new Departament
        {
            Description = "Dpt 02"
        });
    db.SaveChanges();

    var description = "Test ' or 1='1";
    //We should never use concatenation with the values sended by the users on sql instruction
    //It should be done using its values like arguments, with the method ExecuteSqlRaw - OR with interpolation
    db.Database.ExecuteSqlRaw($"update departaments set description='SQLInjectionAttack' where description='{description}'");
    foreach (var departament in db.Departaments.AsNoTracking())
    {
        Console.WriteLine($"Id: {departament.Id}, Description: {departament.Description}");
    }
}

static void ExecuteSQL()
{
    using var db = new Context();

    //First option
    using (var cmd = db.Database.GetDbConnection().CreateCommand())
    {
        cmd.CommandText = "SELECT 1";
        cmd.ExecuteNonQuery();
    }

    //Second option
    var description = "Test";
    db.Database.ExecuteSqlRaw("upate departaments set description={0} where id=1", description);

    //Third option - There is a Async option
    db.Database.ExecuteSqlInterpolated($"update departaments set description={description} where id=1");
}

static void ManageDatabaseConnectionState(bool manageConnectionState)
{
    uint _count = 0;
    using var db = new Context();
    var watch = Stopwatch.StartNew();

    var connection = db.Database.GetDbConnection();

    connection.StateChange += (_, __) => ++_count;

    if (manageConnectionState)
        connection.Open();

    for (var i = 0; i < 200; i++)
    {
        db.Departaments.AsNoTracking().Any();
    }

    watch.Stop();
    var message = $"Time spent: {watch.Elapsed}, {manageConnectionState}, Count: {_count}";
    Console.WriteLine(message);
}

static void EnsureCreatedAndDeleted()
{
    //Métodos são comumente usados em um ambiente de DESENVOLVIMENTO.
    //Para um ambiente de PRODUÇÃO, deve-se usa com cautela para que não haja a perda de dados.
    using var db = new Context();
    db.Database.EnsureCreated();
    db.Database.EnsureDeleted();
}

static void FixEnsureCreatedGap()
{
    var db1 = new Context();
    var db2 = new ContextSecondary();

    db1.Database.EnsureCreated();
    //O segundo não será executado, porque a primeira coisa que o EnsureCreated faz é checar se o banco já existe.
    //Se o banco já existe, ele não faz nada. Então como na primeira chamada ele já foi criado, na segunda não cria nada.
    db2.Database.EnsureCreated();

    //Para corrigir o gap, será necessário forçar a criação da tabela
    var dbCreator = db2.GetService<IRelationalDatabaseCreator>();
    dbCreator.CreateTables();
}

static void HealthCheckDatabase()
{
    using var db = new Context();
    var canConnect = db.Database.CanConnect();

    if (canConnect)
        Console.WriteLine("Database is ON!");
    else
        Console.WriteLine("Database is OFF!");
}
#endregion

#region TYPES OF LOADING

static void AdvanceLoad()
{
    //The advance load will bound the joins with the same DB select query and populate the navegation properties (e.g: employees inside departament)
    //E.g: When selecting the departaments, the advance load will also select the employees that are related to the departaments

    //When dealing with small tables, this function should do the trick quite well. But when its a large table with a bunch of columns and other complex joins,
    //this method of advance load should not be suitable - Also, this might duplicate the data. E.g: For every departament, all employees will be recovered.
    //So, if there is a employee with multiples departaments, this employee will be recovered in every single departament he's bound
    using var db = new Context();
    SeedDatabase(db);

    var departaments = db
        .Departaments
        .Include(x => x.Employees);

    foreach(var departament in departaments)
    {
        Console.WriteLine("___________________________");
        Console.WriteLine($"Departament: {departament.Description}");

        if (!departament.Employees.Any())
            Console.WriteLine($"\tNo employees were found!");
        else
        {
            foreach(var employee in departament.Employees)
            {
                Console.WriteLine($"\tEmployee: {employee.Name}");
            }
        }
    }
}
static void SeedDatabase(Context db)
{
    if (!db.Departaments.Any())
    {
        db.Departaments.AddRange(
            new Departament
            {
                Description = "Departament 01",
                Employees = new List<Employee>
                {
                    new Employee
                    {
                        Name = "Iguinho Bariloche",
                        CPF = "0000000001"
                    }
                },
            },
            new Departament
            {
                Description = "Departament 02",
                Employees = new List<Employee> 
                {
                    new Employee
                    {
                        Name = "Bariloche Iguinho",
                        CPF = "1111111110"
                    }
                }
            });
        db.SaveChanges();
        db.ChangeTracker.Clear();
    }
}
#endregion