using CareCollar.Application.Contracts;
using CareCollar.Application.DTOs;
using CareCollar.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareCollar.Controllers;

[Authorize] 
[ApiController]
[Route("api/pets")] // TODO: Add api versioning
public class PetController(IPetService petService, IUserContext userContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Add([FromBody] CreatePetDto model)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var result = await petService.AddPetAsync(model, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PetDto>))]
    public async Task<IActionResult> GetAllPets()
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var result = await petService.GetAllPetsAsync(userId, HttpContext.RequestAborted);
        return Ok(result.Value);
    }
    
    [HttpGet("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPet(Guid id)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }
        
        var result = await petService.GetPetByIdAsync(id, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }

    [HttpPut("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PetDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePet(Guid id, [FromBody] UpdatePetDto model)
    {
        var userId = userContext.UserId;

        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await petService.UpdatePetAsync(id, model, userId, HttpContext.RequestAborted);
        return result.ToActionResult();
    }
}