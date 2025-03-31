using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;
using WebApplication.View;
using MongoDB.Driver;
using System.Diagnostics;
using WebApplication.Dto.User;
using AutoMapper;
using WebApplication.Dto.user;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController(UserViews userViews, ILogger<UserController> logger, IMapper UserMapper) : ControllerBase
    {
        private readonly IMapper _mapper = UserMapper;
        private readonly UserViews _userViews = userViews;
        private readonly ILogger<UserController> _logger = logger;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userViews.GetUsers();
            var response = _mapper.Map<IEnumerable<UserResponseDTO>>(users);
            
            _logger.LogInformation($"Retrieved {response.Count()} users");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new 
            {
                Message = "An error occurred while retrieving users",
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDTO userDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                                                .Select(e => e.ErrorMessage)
                    });
                }

                if (userDTO == null)
                {
                    return BadRequest(new { message = "Objeto usuário não pode ser nulo" });
                }

                // Validação específica da senha
                if (string.IsNullOrWhiteSpace(userDTO.Password))
                {
                    return BadRequest(new { message = "Senha é obrigatória" });
                }

                // Criptografia da senha
                var userModel = _mapper.Map<UserModel>(userDTO);
                // Criação do usuário
                string userId = await _userViews.CreateUsers(userModel);
                
                // Retorno com URI do recurso criado
                return CreatedAtAction(
                    nameof(GetUserById), 
                    new { id = userId }, 
                    new { 
                        id = userId,
                        message = "Usuário criado com sucesso",
                        links = new {
                            // self = Url.Action(nameof(), new { id = userId }),
                            all = Url.Action(nameof(GetUsers))
                        }
                    });
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                return Conflict(new { 
                    message = "Usuário já existe",
                    field = ex.WriteError.Message.Contains("email") ? "email" : "username"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar usuário: {ex.Message}");
                return StatusCode(500, new { 
                    message = "Ocorreu um erro ao processar sua requisição",
                    requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            bool isDeleted =  await _userViews.DeleteUsers(id);
            if (isDeleted )
            {
                return  NoContent();
            }
                _logger.LogWarning($"User with id {id} not found.");
                return NotFound(new {error = "User Not Found" });
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] string id,
            [FromBody] UpdatedUserDTO userToUpdate)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                var existingUser = await _userViews.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    return NotFound(new { message = $"User with id {id} not found" });
                }

                _mapper.Map(userToUpdate, existingUser);
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userViews.UpdateUsers(existingUser);

                return Ok(new
                {
                    data = _mapper.Map<UserResponseDTO>(existingUser),
                    message = "User updated successfully",
                    links = new
                    {
                        self = Url.Action(nameof(GetUserById), new { id }),
                        all = Url.Action(nameof(GetUsers))
                    }
                });
            }
            catch (MongoException ex)
            {
                _logger.LogError(ex, $"Error updating user {id}");
                return StatusCode(500, new
                {
                    message = "Failed to update user",
                    requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userViews.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new {error = "User not found"});
            }
            var responseUser  = _mapper.Map<UserResponseDTO>(user);
            return Ok(responseUser);
        }
    }
}
