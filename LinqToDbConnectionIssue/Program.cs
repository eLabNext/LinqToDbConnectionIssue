using LinqToDB;
using LinqToDB.DataProvider.MySql;
using LinqToDB.Mapping;

var connectionString = "server=127.0.0.1; Port=13306; user id=foobar; password=foobar; database=foobar; pooling=true;Allow Zero Datetime=False;Convert Zero Datetime=true;Allow User Variables=True;charset=utf8;";

while (true)
{
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

    Thread.Sleep(10);
}

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