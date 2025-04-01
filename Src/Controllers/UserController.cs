using Microsoft.AspNetCore.Mvc;
using WebApplication.Src.Models;
using WebApplication.Src.View;
using MongoDB.Driver;
using System.Diagnostics;
using WebApplication.Src.Dto.User;
using AutoMapper;
using WebApplication.Src.Dto.user;
using Microsoft.AspNetCore.Authorization;
using WebApplication.Src.Interface;

namespace WebApplication.Src.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/users")]
    public class UserController(IUserView userViews, ILogger<UserController> logger, IMapper UserMapper) : ControllerBase
    {
        private readonly IMapper _mapper = UserMapper;
        private readonly IUserView _userViews = userViews;
        private readonly ILogger<UserController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("GET /api/v1/users - Fetching users...");
            try
            {
                var users = await _userViews.GetUsers();
                var response = _mapper.Map<IEnumerable<UserResponseDTO>>(users);

                _logger.LogInformation($"Retrieved {response.Count()} users successfully.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users.");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving users.",
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PostUser([FromBody] CreateUserDTO userDTO)
        {
            _logger.LogInformation("POST /api/v1/users - Creating a new user...");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validation failed for user creation.");
                    return BadRequest(new
                    {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                                                  .Select(e => e.ErrorMessage)
                    });
                }

                if (userDTO == null)
                {
                    _logger.LogWarning("UserDTO is null.");
                    return BadRequest(new { message = "Objeto usuário não pode ser nulo" });
                }

                var userModel = _mapper.Map<UserModel>(userDTO);
                string userId = await _userViews.CreateUsers(userModel);

                _logger.LogInformation($"User created successfully with ID: {userId}");

                return CreatedAtAction(nameof(GetUserById), new { id = userId }, new
                {
                    id = userId,
                    message = "Usuário criado com sucesso",
                    links = new { all = Url.Action(nameof(GetUsers)) }
                });
            }
            catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                _logger.LogWarning($"Duplicate user detected: {ex.Message}");
                return Conflict(new
                {
                    message = "User already exists",
                    field = ex.WriteError.Message.Contains("email") ? "email" : "username"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user.");
                return StatusCode(500, new
                {
                    message = "Error handling post User",
                    requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            _logger.LogInformation($"DELETE /api/v1/users/{id} - Deleting user...");
            try
            {
                bool isDeleted = await _userViews.DeleteUsers(id);
                if (isDeleted)
                {
                    _logger.LogInformation($"User {id} deleted successfully.");
                    return NoContent();
                }

                _logger.LogWarning($"User {id} not found.");
                return NotFound(new { error = "User Not Found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user {id}");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] UpdatedUserDTO userToUpdate)
        {
            _logger.LogInformation($"PUT /api/v1/users/{id} - Updating user...");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Validation failed for updating user {id}.");
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                var existingUser = await _userViews.GetUserByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning($"User {id} not found for update.");
                    return NotFound(new { message = $"User with id {id} not found" });
                }

                _mapper.Map(userToUpdate, existingUser);
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userViews.UpdateUsers(existingUser);

                _logger.LogInformation($"User {id} updated successfully.");

                return Ok(new
                {
                    data = _mapper.Map<UserResponseDTO>(existingUser),
                    message = "User updated successfully",
                    links = new { self = Url.Action(nameof(GetUserById), new { id }), all = Url.Action(nameof(GetUsers)) }
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
        public async Task<IActionResult> GetUserById(string id)
        {
            _logger.LogInformation($"GET /api/v1/users/{id} - Fetching user by ID...");
            try
            {
                UserModel? user = await _userViews.GetUserByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning($"User {id} not found.");
                    return NotFound(new { error = "User not found" });
                }

                _logger.LogInformation($"User {id} retrieved successfully.");
                var responseUser = _mapper.Map<UserResponseDTO>(user);
                return Ok(responseUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user {id}");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }
    }
}
