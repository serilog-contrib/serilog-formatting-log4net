using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sample;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Log4Net;

var formatter = new Log4NetTextFormatter(options => options.UseCDataMode(CDataMode.IfNeeded).UseNoXmlNamespace());
var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File(formatter, "logs.xml")
    .CreateLogger();

logger.Information("Running on .NET {DotnetVersion:l}", Environment.Version);

using var sqliteConnection = new SqliteConnection("Data Source=:memory:");
sqliteConnection.Open();
using var loggerFactory = new SerilogLoggerFactory(logger);

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseLoggerFactory(loggerFactory)
    .EnableSensitiveDataLogging()
    .UseSqlite(sqliteConnection)
    .Options;

using var context = new AppDbContext(options);
context.Database.EnsureCreated();
context.Employees.Add(new Employee { Name = "Ric Weiland", DateOfBirth = new DateOnly(1953, 4, 21) });
context.SaveChanges();
