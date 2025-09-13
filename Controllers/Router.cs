using System.Text.Json;
using DemoAppDotNet.DTOs;
using Microsoft.AspNetCore.Mvc;
using DemoAppDotNet.Models;
using DemoAppDotNet.Services;

namespace DemoAppDotNet.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class Router : ControllerBase
    {   
        private readonly BuildingService _buildingService;
        private readonly FloorService _floorService;
        private readonly SpotService _spotService;
        private readonly BayService _bayService;
        private readonly CarService _carService;
        
        public Router(
            BuildingService buildingService,
            FloorService floorService,
            SpotService spotService,
            BayService bayService,
            CarService carService
            )
        {
            _buildingService = buildingService;
            _floorService = floorService;
            _spotService = spotService;
            _bayService = bayService;
            _carService = carService;
        }
        
        // Healthcheck
        // GET: api/v1/health
        [HttpGet("health")]
        public IActionResult Get()
        {
            return Ok(new { status = "OK" });
        }

        // Buildings CRUD
        [HttpGet("buildings")]
        public async Task<IActionResult> GetBuildings()
        {
            var buildings = await _buildingService.GetAllBuildingsAsync();
            return Ok(buildings);
        }

        [HttpGet("buildings/{id:int}")]
        public async Task<IActionResult> GetBuilding(int id)
        {
            var building = await _buildingService.GetBuildingByIdAsync(id);
            return Ok(building);
        }

        [HttpPost("buildings")]
        public IActionResult CreateBuilding([FromBody] Building building)
        {
            return CreatedAtAction(nameof(GetBuilding), new { id = building.Id }, building);
        }

        [HttpPut("buildings/{id:int}")]
        public IActionResult UpdateBuilding(int id, [FromBody] Building building)
        {
            return Ok();
        }

        [HttpDelete("buildings/{id:int}")]
        public IActionResult DeleteBuilding(int id)
        {
            return NoContent();
        }

        // Floors CRUD
        [HttpGet("floors")]
        public IActionResult GetFloors()
        {   
            return Ok();
        }

        [HttpGet("floors/{id:int}")]
        public IActionResult GetFloor(int id)
        {
            return Ok();
        }

        [HttpPost("floors")]
        public async Task<IActionResult> CreateFloor([FromBody] FloorDto floorDto)
        {
            try
            {
                var floor = new Floor
                {
                    Number = floorDto.Number,
                    BuildingId = floorDto.BuildingId
                };
        
                var createdFloor = await _floorService.CreateFloorAsync(floor);
                return CreatedAtAction(nameof(GetFloor), new { id = createdFloor.Id }, new
                {
                    id = createdFloor.Id,
                    number = createdFloor.Number,
                    buildingId = createdFloor.BuildingId
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create floor: {ex.Message}");
            }
        }

        [HttpPut("floors/{id:int}")]
        public IActionResult UpdateFloor(int id, [FromBody] Floor floor)
        {
            return Ok();
        }

        [HttpDelete("floors/{id:int}")]
        public async Task<IActionResult> DeleteFloor(int id)
        {
            try
            {
                var result = await _floorService.DeleteFloorAsync(id);
                if (!result)
                {
                    return NotFound($"Floor with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete floor: {ex.Message}");
            }
        }

        // Bays CRUD
        [HttpGet("bays")]
        public IActionResult GetBays()
        {
            return Ok();
        }

        [HttpGet("bays/{id:int}")]
        public IActionResult GetBay(int id)
        {
            return Ok();
        }

        [HttpPost("bays")]
        public async Task<IActionResult> CreateBay([FromBody] JsonElement jsonElement)
        {
            try
            {
                var bay = new Bay
                {
                    Name = jsonElement.GetProperty("name").GetString() ?? "Bay",
                    Location = jsonElement.GetProperty("location").GetString(),
                    FloorId = jsonElement.GetProperty("floorId").GetInt32(),
                    BuildingId = jsonElement.GetProperty("buildingId").GetInt32()
                };
        
                var createdBay = await _bayService.CreateBayAsync(bay);
        
                return CreatedAtAction(nameof(GetBay), new { id = createdBay.Id }, new { id = createdBay.Id });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create bay: {ex.Message}");
            }
        }

        [HttpPut("bays/{id:int}")]
        public IActionResult UpdateBay(int id, [FromBody] Bay bay)
        {
            return Ok();
        }

        [HttpDelete("bays/{id:int}")]
        public async Task<IActionResult> DeleteBay(int id)
        {
            try
            {
                var result = await _bayService.DeleteBayAsync(id);
                if (!result)
                {
                    return NotFound($"Bay with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete bay: {ex.Message}");
            }
        }

        // Spots CRUD
        [HttpGet("spots")]
        public async Task<IActionResult> GetAllSpots()
        {
            var spots = await _spotService.GetAllSpotsAsync();
            return Ok(spots);
        }

        [HttpGet("spots/available")]
        public async Task<IActionResult> GetAvailableSpots()
        {
            var spots = await _spotService.GetAvailableSpotsAsync();
            return Ok(spots);
        }

        [HttpGet("spots/{id:int}")]
        public IActionResult GetSpot(int id)
        {
            return Ok();
        }

        [HttpPost("spots")]
        public async Task<IActionResult> CreateSpot([FromBody] JsonElement jsonElement)
        {
            try
            {
                var spot = new Spot
                {
                    Name = jsonElement.GetProperty("name").GetString(),
                    Number = jsonElement.GetProperty("number").GetInt32(),
                    Status = jsonElement.GetProperty("status").GetString() ?? "Available",
                    BayId = jsonElement.GetProperty("bayId").GetInt32(),
                    FloorId = jsonElement.GetProperty("floorId").GetInt32(),
                    BuildingId = jsonElement.GetProperty("buildingId").GetInt32()
                };
        
                var createdSpot = await _spotService.CreateSpotAsync(spot);
                return CreatedAtAction(nameof(GetSpot), new { id = createdSpot.Id }, new { id = createdSpot.Id });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create spot: {ex.Message}");
            }
        }

        [HttpPut("spots/status/{id:int}")]
        public async Task<IActionResult> UpdateSpot(int id, [FromBody] JsonElement jsonElement)
        {
            try
            {
                var spotDto = await _spotService.GetSpotByIdAsync(id);
                if (spotDto == null)
                {
                    return NotFound($"Spot with ID {id} not found.");
                }
        
                string status = jsonElement.GetProperty("status").GetString();
                spotDto.Status = status;
        
                var updatedSpot = await _spotService.UpdateSpotAsync(spotDto);
                return Ok(updatedSpot);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update spot: {ex.Message}");
            }
        }

        [HttpDelete("spots/{id:int}")]
        public async Task<IActionResult> DeleteSpot(int id)
        {
            try
            {
                var result = await _spotService.DeleteSpotAsync(id);
                if (!result)
                {
                    return NotFound($"Spot with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to delete spot: {ex.Message}");
            }
        }
        
        // Cars CRUD
        [HttpGet("cars")]
        public async Task<IActionResult> GetCars([FromQuery] string? plate = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(plate))
                {
                    var filteredCars = await _carService.SearchCarsByPlateAsync(plate);
                    return Ok(filteredCars);
                }

                var allCars = await _carService.GetAllCarsAsync();
                return Ok(allCars);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve cars: {ex.Message}");
            }
        }
        
        [HttpGet("cars/unparked")]
        public async Task<IActionResult> GetUnparkedCars()
        {
            try
            {
                var unparkedCars = await _carService.GetUnparkedCarsAsync();
                return Ok(unparkedCars);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve unparked cars: {ex.Message}");
            }
        }
        
        [HttpPost("cars/checkin")]
        public async Task<IActionResult> CheckInCar([FromBody] JsonElement jsonElement)
        {
            try
            {
                string plate = jsonElement.GetProperty("plate").GetString();
                int spotId = jsonElement.GetProperty("spotId").GetInt32();
        
                var car = await _carService.GetCarByPlateAsync(plate);
                if (car == null)
                    return NotFound($"Car with plate {plate} not found.");
        
                var spot = await _spotService.GetSpotByIdAsync(spotId);
                if (spot == null)
                    return NotFound($"Spot with ID {spotId} not found.");
        
                if (spot.Status == "Occupied")
                    return BadRequest("Spot is already occupied.");
        
                car.SpotId = spotId;
                car.CheckIn = DateTime.UtcNow;
                spot.Status = "Occupied";
        
                await _carService.UpdateCarAsync(car);
                await _spotService.UpdateSpotAsync(spot);
        
                return Ok(new { message = "Car checked in successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to check in car: {ex.Message}");
            }
        }
        
        [HttpPost("cars/checkout")]
        public async Task<IActionResult> CheckOutCar([FromBody] JsonElement jsonElement)
        {
            try
            {
                string plate = jsonElement.GetProperty("plate").GetString();
                var car = await _carService.GetCarByPlateAsync(plate);
                if (car == null)
                    return NotFound($"Car with plate {plate} not found.");
        
                if (car.SpotId == null)
                    return BadRequest("Car is not currently parked.");
        
                var spot = await _spotService.GetSpotByIdAsync(car.SpotId.Value);
                if (spot == null)
                    return NotFound($"Spot with ID {car.SpotId} not found.");
        
                car.CheckOut = DateTime.UtcNow;
                car.SpotId = null;
                spot.Status = "Available";
        
                await _carService.UpdateCarAsync(car);
                await _spotService.UpdateSpotAsync(spot);
        
                return Ok(new { message = "Car checked out successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to check out car: {ex.Message}");
            }
        }
        
        [HttpGet("cars/{id:int}")]
        public async Task<IActionResult> GetCar(int id)
        {
            try
            {
                var car = await _carService.GetCarByIdAsync(id);
                if (car == null)
                {
                    return NotFound($"Car with ID {id} not found.");
                }
                return Ok(car);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve car: {ex.Message}");
            }
        }

    }
}