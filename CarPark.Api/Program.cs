using CarPark.Api.Models;
using CarPark.Core.Exceptions;
using CarPark.Core.Services;
using Microsoft.AspNetCore.Mvc;
using ParCark.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSingleton<ICarParkService, CarParkService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var carParkApi = app.MapGroup("/parking").WithTags("Car Park API");

carParkApi.MapGet("/", async (ICarParkService carParkService) =>
{
    var spaces = carParkService.GetAvailableSpaces();
    return Results.Ok(new AvailableSpacesResponse(spaces.AvailableSpaces, spaces.OccupiedSpaces));
})
.WithDescription("Gets available and occupied number of spaces")
.Produces<AvailableSpacesResponse>(StatusCodes.Status200OK);

carParkApi.MapPost("/", async (CheckInRequest request, ICarParkService carParkService) =>
{
    try
    {
        var vehicle = new Vehicle(request.VehicleReg, VehicleType.FromValue(request.VehicleType));
        var result = carParkService.CheckIn(vehicle);
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

carParkApi.MapPost("/exit", async (CheckOutRequest request, ICarParkService carParkService) =>
{
    try
    {
        var result = carParkService.CheckOut(request.VehicleReg);
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
