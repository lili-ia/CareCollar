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
    /// <summary>Adds a new pet to the user's profile.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] CreatePetDto model)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.AddPetAsync(model, userId, HttpContext.RequestAborted);
        if (!result.IsSuccess) return result.ToActionResult();

        return CreatedAtAction(nameof(GetPet), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>Retrieves all pets for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PetDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllPets()
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.GetAllPetsAsync(userId, HttpContext.RequestAborted);
        return Ok(result.Value);
    }

    /// <summary>Gets a specific pet by ID.</summary>
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPet(Guid id)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.GetPetByIdAsync(id, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    /// <summary>Updates pet information.</summary>
    [HttpPut("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] UpdatePetDto model)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.UpdatePetAsync(id, model, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    /// <summary>Deletes a pet by ID.</summary>
    [HttpDelete("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePet(Guid id)
    {
        var userId = userContext.UserId;
        if (userId == Guid.Empty) return Unauthorized();

        var result = await petService.DeletePetAsync(id, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }
}
