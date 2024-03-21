# LINQ to DB connection pool depletion

This project demonstrates an issue with connection pooling in a specific scenario when using LINQ to DB. When a query is executed using `new DataContext(...)`, using `LoadWith` and `FirstOrDefault` (or `SingleOrDefault`) together in same the query it looks like connections are not correctly closed/disposed, thus not returned to the connection pool. This quickly results in a connection pool depletion and eventually the exception "Connect Timeout expired. All pooled connections are in use.". The problem occurs with both the MySqlConnector and the MySql.Official data provider.

## Running the project

First run the Docker Compose file which will start a MariaDB container and create a database with the necessary tables and columns:

```shell
docker compose up
```

After that, just start the project by selecting Debug in your editor, or run the command:

```shell
dotnet run --project LinqToDbConnectionIssue/LinqToDbConnectionIssue.csproj
```

Set maxExecutionCount to 20 (a low number) and leave the application running to see that connections are not correctly returned to the connection pool. When using the command `SHOW PROCESSLIST;` MariaDB will show the open connections, which all will be in Sleep state and will not be terminated when the configured Connection Idle Timeout expires.
