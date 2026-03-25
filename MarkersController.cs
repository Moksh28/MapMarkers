using MapMarkers.Data;
using MapMarkers.Models;
using Microsoft.AspNetCore.Mvc;

namespace MapMarkers.Controllers;

/// <summary>
/// REST API for map markers.
/// All data access goes through <see cref="MarkerRepository"/> (ADO.NET).
/// The frontend calls these endpoints via jQuery AJAX.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly MarkerRepository _repo;
    private readonly ILogger<MarkersController> _logger;

    public MarkersController(MarkerRepository repo, ILogger<MarkersController> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    // GET api/markers
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Marker>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var markers = await _repo.GetAllAsync();
            return Ok(markers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all markers");
            return StatusCode(500, new { error = "Failed to retrieve markers." });
        }
    }

    // GET api/markers/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Marker), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var marker = await _repo.GetByIdAsync(id);
            return marker is null ? NotFound(new { error = $"Marker {id} not found." }) : Ok(marker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching marker {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve marker." });
        }
    }

    // POST api/markers
    [HttpPost]
    [ProducesResponseType(typeof(Marker), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMarkerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required." });

        try
        {
            var created = await _repo.InsertAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating marker");
            return StatusCode(500, new { error = "Failed to create marker." });
        }
    }

    // PUT api/markers/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(Marker), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMarkerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { error = "Title is required." });

        try
        {
            bool updated = await _repo.UpdateAsync(id, request);
            if (!updated) return NotFound(new { error = $"Marker {id} not found." });

            var marker = await _repo.GetByIdAsync(id);
            return Ok(marker);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating marker {Id}", id);
            return StatusCode(500, new { error = "Failed to update marker." });
        }
    }

    // DELETE api/markers/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _repo.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { error = $"Marker {id} not found." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting marker {Id}", id);
            return StatusCode(500, new { error = "Failed to delete marker." });
        }
    }

    // GET api/markers/apikey   (exposes the key to the frontend safely)
    [HttpGet("apikey")]
    public IActionResult GetApiKey([FromServices] IConfiguration config)
        => Ok(new { key = config["GoogleMaps:ApiKey"] ?? "" });
}
