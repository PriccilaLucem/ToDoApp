using Microsoft.AspNetCore.Mvc;
using WebApplication.Src.Config;
using WebApplication.Src.Dto.task;
using WebApplication.Src.Models.TaskModel;
using WebApplication.View;
using AutoMapper;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication.Src.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/task")]
    public class TaskController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly TaskViews _taskViews;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ILogger<TaskController> logger, TaskViews taskViews, IMapper taskMapper)
        {
            _logger = logger;
            _taskViews = taskViews;
            _mapper = taskMapper;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> Post([FromBody] TaskDTO taskDto)
        {
            _logger.LogInformation("POST /api/v1/task - Creating a new task...");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validation failed for task creation.");
                    return BadRequest(new
                    {
                        message = "Invalid Data",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                TaskModel task = _mapper.Map<TaskModel>(taskDto);
                string taskId = await _taskViews.CreateTaskView(task);
                _logger.LogInformation($"Task created successfully with ID: {taskId}");

                return CreatedAtAction(
                    nameof(GetOneTask),
                    new { id = taskId },
                    new
                    {
                        id = taskId,
                        message = "Task created",
                        link = new
                        {
                            all = Url.Action(nameof(GetAllTasks))
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task.");
                return StatusCode(500, new
                {
                    message = "Unexpected error",
                    requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> GetAllTasks()
        {
            _logger.LogInformation("GET /api/v1/task - Fetching all tasks...");
            try
            {
                var tasks = await _taskViews.ListTaskView();
                _logger.LogInformation($"Retrieved {tasks.Count()} tasks successfully.");
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks.");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> GetOneTask(string id)
        {
            _logger.LogInformation($"GET /api/v1/task/{id} - Fetching task by ID...");
            try
            {
                TaskModel? task = await _taskViews.GetOneTaskView(id);
                if (task == null)
                {
                    _logger.LogWarning($"Task {id} not found.");
                    return NotFound(new { message = "Task Not Found" });
                }

                _logger.LogInformation($"Task {id} retrieved successfully.");
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving task {id}.");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteTask(string id)
        {
            _logger.LogInformation($"DELETE /api/v1/task/{id} - Deleting task...");
            try
            {
                bool isDeleted = await _taskViews.DeleteTaskView(id);
                if (!isDeleted)
                {
                    _logger.LogWarning($"Task {id} not found.");
                    return NotFound(new { message = "Task Not Found" });
                }

                _logger.LogInformation($"Task {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task.");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> UpdateTask([FromBody] TaskModel task)
        {
            _logger.LogInformation($"PUT /api/v1/task/{task.Id} - Updating task...");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Validation failed for updating task {task.Id}.");
                    return BadRequest(new
                    {
                        message = "Validation failed",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                await _taskViews.UpdateTaskView(task);
                _logger.LogInformation($"Task {task.Id} updated successfully.");

                return Ok(new
                {
                    data = task,
                    message = "Task updated successfully",
                    links = new
                    {
                        self = Url.Action(nameof(GetOneTask), new { task.Id }),
                        all = Url.Action(nameof(GetAllTasks))
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating task {task.Id}");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                });
            }
        }
    }
}
