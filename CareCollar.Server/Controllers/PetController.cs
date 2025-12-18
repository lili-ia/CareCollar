using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace CareCollar.Controllers;

[Authorize]
[ApiController]
[Route("api/pets")]
[Produces(MediaTypeNames.Application.Json)]
public class PetController(IPetService petService, IUserContext userContext) : ControllerBase
{
    /// <summary>
    /// Adds a new pet to the user's profile.
    /// </summary>
    /// <param name="model">Details of the pet (Name, Species, Breed, etc.).</param>
    /// <returns>The created pet object.</returns>
    /// <response code="201">Pet successfully created.</response>
    /// <response code="401">Unauthorized: User context missing.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] CreatePetDto model)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await petService.AddPetAsync(model, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    /// <summary>
    /// Retrieves all pets associated with the current user.
    /// </summary>
    /// <response code="200">Returns a list of pets.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PetDto>))]
    public async Task<IActionResult> GetAllPets()
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await petService.GetAllPetsAsync(userId, HttpContext.RequestAborted);
        return Ok(result.Value);
    }
    
    /// <summary>
    /// Gets specific pet details by ID.
    /// </summary>
    /// <param name="id">The unique Guid of the pet.</param>
    /// <response code="200">Pet found.</response>
    /// <response code="404">Pet not found for the current user.</response>
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPet(Guid id)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();
        
        var result = await petService.GetPetByIdAsync(id, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    /// <summary>
    /// Updates pet information (Name, Breed, or Weight).
    /// </summary>
    /// <param name="id">The Guid of the pet to update.</param>
    /// <param name="model">New data for the pet.</param>
    [HttpPut("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] UpdatePetDto model)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.UpdatePetAsync(id, model, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }
}