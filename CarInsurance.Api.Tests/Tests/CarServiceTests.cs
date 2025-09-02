using CarInsurance.Api.Data;
using CarInsurance.Api.Models;
using CarInsurance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarInsurance.Api.Tests;

public class CarServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);

        TestDataBaseGenerator.EnsureSeeded(context);

        return context;
    }

    [Fact]
    public async Task IsInsuranceValid_ReturnsTrue_WhenInsuranceIsValid()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var carId = 1;
        var date = new DateOnly(2024, 6, 15);

        // Act
        var isValid = await service.IsInsuranceValidAsync(carId, date);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task IsInsuranceValid_ReturnsTrue_OnStartDateBoundary()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var carId = 1;
        var startDate = new DateOnly(2024, 1, 1);

        // Act
        var isValid = await service.IsInsuranceValidAsync(carId, startDate);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task IsInsuranceValid_ReturnsTrue_OnEndDateBoundary()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var carId = 1;
        var endDate = new DateOnly(2024, 12, 31);

        // Act
        var isValid = await service.IsInsuranceValidAsync(carId, endDate);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task IsInsuranceValid_ReturnsFalse_JustBeforeStartDate()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var carId = 1;
        var date = new DateOnly(2023, 12, 31);

        // Act
        var isValid = await service.IsInsuranceValidAsync(carId, date);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task IsInsuranceValid_ReturnsFalse_JustAfterEndDate()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var carId = 1;
        var date = new DateOnly(2025, 1, 1);

        // Act
        var isValid = await service.IsInsuranceValidAsync(carId, date);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task IsInsuranceValid_ThrowsKeyNotFoundException_WhenCarDoesNotExist()
    {
        // Arrange
        var context = GetDbContext();
        var service = new CarService(context);
        var nonExistentCarId = 999;
        var date = new DateOnly(2024, 6, 1);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.IsInsuranceValidAsync(nonExistentCarId, date));
    }
}