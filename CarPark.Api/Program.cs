using CarPark.Api.Models;
using CarPark.Core.Exceptions;
using CarPark.Core.Persistence;
using CarPark.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDateTimeHelper, DateTimeHelper>();
builder.Services.AddScoped<ICarParkService, CarParkService>();

builder.Services.AddDbContext<CarParkDbContext>(opt => opt.UseInMemoryDatabase("CarPark"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var carParkApi = app.MapGroup("/parking").WithTags("Car Park API");

// GET /parking - Get available and occupied number of spaces
carParkApi.MapGet("/", async (ICarParkService carParkService, CancellationToken ct) =>
{
    var spaces = await carParkService.GetAvailableSpaces(ct);
    return Results.Ok(new AvailableSpacesResponse(spaces.AvailableSpaces, spaces.OccupiedSpaces));
})
.WithDescription("Gets available and occupied number of spaces")
.Produces<AvailableSpacesResponse>(StatusCodes.Status200OK);

// POST /parking - Parks a given vehicle in the first available space and returns the vehicle and its space number
carParkApi.MapPost("/", async (CheckInRequest request, ICarParkService carParkService, CancellationToken ct) =>
{
    try
    {
        var result = await carParkService.CheckIn(request.VehicleReg, request.VehicleType, ct);
        return Results.Ok(new CheckInResponse(result.VehicleReg, result.SpaceNumber, result.CheckInTime));
    }
    catch (Exception ex) when (ex is NoAvailableParkingSpacesException || ex is VehicleAlreadyParkedException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithDescription("Parks a given vehicle in the first available space and returns the vehicle and its space number")
.Produces<CheckInResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

// POST /parking/exit - Frees up the parking space for the given vehicle and returns the parking charge
carParkApi.MapPost("/exit", async (CheckOutRequest request, ICarParkService carParkService, CancellationToken ct) =>
{
    try
    {
        var result = await carParkService.CheckOut(request.VehicleReg, ct);
        return Results.Ok(new CheckOutResponse(result.VehicleReg, result.parkingCharge, result.CheckInTime, result.CheckOutTime));
    }
    catch (VehicleNotCheckedInException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithDescription("Frees up the parking space for the given vehicle and returns the parking charge")
.Produces<CheckOutResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

await app.RunAsync();
