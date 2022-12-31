using EntityFC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;

EnsureCreatedAndDeleted();
FixEnsureCreatedGap();
HealthCheckDatabase();
ManageDatabaseConnectionState(true);
ManageDatabaseConnectionState(false);
ExecuteSQL();

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

    if(manageConnectionState)
        connection.Open();

    for(var i = 0; i < 200; i++)
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