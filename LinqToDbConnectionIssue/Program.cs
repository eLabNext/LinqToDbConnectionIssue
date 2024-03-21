using LinqToDB;
using LinqToDB.DataProvider.MySql;
using LinqToDB.Mapping;

var connectionString = "Server=127.0.0.1; Port=13306; User ID=foobar; Password=foobar; Database=foobar; Pooling=true; Connection Idle Timeout=10";

// Set maxExecutionCount to 20 (a low number) and leave the application running to see that connections are not
// correctly returned to the connection pool. When using the command `SHOW PROCESSLIST;` MariaDB will show the open
// connections, which all will be in Sleep state and will not be terminated when the configured Connection Idle Timeout
// expires.
const int maxExecutionCount = int.MaxValue;
var currentExecutionCount = 0;

while (true)
{
    if (currentExecutionCount == maxExecutionCount)
    {
        break;
    }
    
    Console.WriteLine($"{DateTime.Now:O}: Executing query.");

    // For both MySqlConnector and MySql.Official the connection pool will be depleted. Due to differences in
    // connection pooling logic, MySql.Official will take longer to break compared to MySqlConnector.
    var dataContext = new DataContext(MySqlTools.GetDataProvider("MySqlConnector"), connectionString);
    // var dataContext = new DataContext(MySqlTools.GetDataProvider("MySql.Official"), connectionString);

    // The combination of using DataContext, LoadWith and FirstOrDefault (or SingleOrDefault) will quickly deplete the connection pool.
    var result = dataContext.GetTable<Foo>()
        .LoadWith(foo => foo.Bars)
        .FirstOrDefault();

    // By using FirstOrDefault without LoadWith the connection pool will not deplete.
    // var result = dataContext.GetTable<Foo>()
    //     .FirstOrDefault();

    // By using ToList with LoadWith the connection pool will not deplete.
    // var result = dataContext.GetTable<Foo>()
    //     .LoadWith(foo => foo.Bars)
    //     .Take(1)
    //     .ToList()[0];

    currentExecutionCount++;

    Thread.Sleep(10);
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();

[Table("Foo")]
public class Foo
{
    [Column]
    public string Id { get; set; }

    [Association(ThisKey = nameof(Id), OtherKey = nameof(Bar.FooId))]
    public List<Bar> Bars { get; set; }
}

[Table("Bar")]
public class Bar
{
    [Column]
    public string Id { get; set; }

    [Column]
    public string FooId { get; set; }
}