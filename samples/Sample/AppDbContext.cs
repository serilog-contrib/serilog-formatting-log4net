using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Sample;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
}

public class Employee
{
    public int Id { get; init; }

    [MaxLength(200)]
    public required string Name { get; init; }

    public required DateOnly DateOfBirth { get; init; }
}
