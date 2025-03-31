using System.Diagnostics;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Config;
using WebApplication.Dto.task;
using WebApplication.Models.TaskModel;
using WebApplication.View;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("/api/v1/task")]
    public class TaskController(ILogger<TaskController> logger, TaskViews taskViews, IMapper taskMapper) : ControllerBase
    {
        private readonly IMapper _mapper = taskMapper;

        private readonly TaskViews _taskViews = taskViews;
        private readonly ILogger<TaskController> _logger = logger;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(IEnumerable<ErrorDetails>))]
        
        public async Task<IActionResult> Post([FromBody] TaskDTO taskDto)
        {
            try
            {
                
                if(!ModelState.IsValid)
                {
                    return BadRequest(new {
                        message = "Invalid Data",
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        
                    });
                }
                TaskModel task = _mapper.Map<TaskModel>(taskDto);
                string taskId = await _taskViews.CreateTaskView(task);
                return CreatedAtAction(
                    nameof(getOneTask),
                    new { id = taskId },
                    new{
                        id = taskId,
                        message = "Task created",
                        link = new {
                            all = Url.Action(nameof(GetAllTasks))
                        }
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar Task");
                return StatusCode(500, new {
                    message = "Unnexpected error",
                    requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> GetAllTasks()
        {
            try
            {
                var task = await _taskViews.ListTaskView();
                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at getting all tasks");
                return StatusCode(500, new {
                    message = "Internal server error",
                });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TaskModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        public async Task<IActionResult> getOneTask(string id)
        {
            try
            {
                TaskModel task = await _taskViews.GetOneTaskView(id);
                return Ok(task); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error at getting single task");
                return StatusCode(500, new {
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
           try
            {
                bool isDeleted = await _taskViews.DeleteTaskView(id);
                if(!isDeleted)
                {
                    return NotFound(new {
                        message = "Task Not Found",
                    });
                }
                return NoContent();
            }
            catch(Exception ex)
            {
                _logger.LogError( ex, "Error at deleting Task");
                return StatusCode(500, new {
                    message = "Internal server error",
                });
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(IEnumerable<ErrorDetails>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type=typeof(IEnumerable<ErrorDetails>))]   
        public async Task<IActionResult> UpdateTask([FromBody] TaskModel task)
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
                
                await _taskViews.UpdateTaskView(task);
                return Ok(new
                {
                    data = task,
                    message = "Task updated successfully",
                    links = new
                    {
                        self = Url.Action(nameof(getOneTask), new { task.Id }),
                        all = Url.Action(nameof(GetAllTasks))
                    }
                });

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error at updating task {task.Id}");
                return StatusCode(500, new {
                    message = "Internal server error",
                });
            }
        }    
    }
}