﻿using CarInsurance.Api.Data;
using CarInsurance.Api.Dtos;
using CarInsurance.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Api.Services;

public class CarService(AppDbContext db)
{
    private readonly AppDbContext _db = db;

    public async Task<List<CarDto>> ListCarsAsync()
    {
        return await _db.Cars.Include(c => c.Owner)
            .Select(c => new CarDto(c.Id, c.Vin, c.Make, c.Model, c.YearOfManufacture,
                                    c.OwnerId, c.Owner.Name, c.Owner.Email))
            .ToListAsync();
    }

    public async Task<bool> IsInsuranceValidAsync(long carId, DateOnly date)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found");

        return await _db.Policies.AnyAsync(p =>
            p.CarId == carId &&
            p.StartDate <= date &&
            (p.EndDate == null || p.EndDate >= date)
        );
    }
    public async Task<InsuranceClaim> RegisterClaimAsync(long carId, ClaimDto claimDto)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found.");

        var claim = new InsuranceClaim
        {
            CarId = carId,
            ClaimDate = claimDto.ClaimDate,
            Description = claimDto.Description,
            Amount = claimDto.Amount
        };

        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        return claim;
    }

    public async Task<List<HistoryEventDto>> GetCarHistoryAsync(long carId)
    {
        var carExists = await _db.Cars.AnyAsync(c => c.Id == carId);
        if (!carExists) throw new KeyNotFoundException($"Car {carId} not found.");

        var policies = await _db.Policies
            .Where(p => p.CarId == carId)
            .Select(p => new HistoryEventDto("Policy", $"Asigurare cu {p.Provider} (de la {p.StartDate.Year} la {p.EndDate.Year})", p.StartDate))
            .ToListAsync();

        var claims = await _db.Claims
            .Where(c => c.CarId == carId)
            .Select(c => new HistoryEventDto("Claim", $"Cerere de despăgubire de {c.Amount:C} pentru '{c.Description}'", c.ClaimDate))
            .ToListAsync();

        var history = new List<HistoryEventDto>();
        history.AddRange(policies);
        history.AddRange(claims);

        return history.OrderBy(e => e.Date).ToList();
    }
}
