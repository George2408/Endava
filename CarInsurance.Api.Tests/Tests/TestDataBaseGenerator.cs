using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using System;
using System.Linq;

namespace CarInsurance.Api.Tests;

public static class TestDataBaseGenerator
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.Owners.Any()) return;

        var ana = new Owner { Id = 1, Name = "Ana Pop", Email = "ana.pop@example.com" };
        var bogdan = new Owner { Id = 2, Name = "Bogdan Ionescu", Email = "bogdan.ionescu@example.com" };
        db.Owners.AddRange(ana, bogdan);

        var car1 = new Car { Id = 1, Vin = "VIN12345", Make = "Dacia", Model = "Logan", YearOfManufacture = 2018, OwnerId = ana.Id };
        var car2 = new Car { Id = 2, Vin = "VIN67890", Make = "VW", Model = "Golf", YearOfManufacture = 2021, OwnerId = bogdan.Id };
        db.Cars.AddRange(car1, car2);

        db.Policies.AddRange(
            new InsurancePolicy { CarId = car1.Id, Provider = "Allianz", StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 12, 31) },
            new InsurancePolicy { CarId = car2.Id, Provider = "Allianz", StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2025, 9, 30) }
        );

        db.SaveChanges();
    }
}