using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjeYonetim.API.Data;
using ProjeYonetim.API.DTOs;
using ProjeYonetim.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjeYonetim.API.Controllers
{
    [Route("api/projects/{projectId}/tasks")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            // ClaimTypes.NameIdentifier olduğundan emin olalım
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksForProject(int projectId)
        {
            var userId = GetUserId();
            // Projenin varlığını ve kullanıcıya ait olduğunu kontrol et
            if (!await _context.Projects.AnyAsync(p => p.Id == projectId && p.UserId == userId))
            {
                return Forbid();
            }

            var tasks = await _context.TaskItems
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTaskForProject(int projectId, TaskCreateDto taskCreateDto)
        {
            var userId = GetUserId();
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

            if (project == null)
            {
                return Forbid();
            }

            var taskItem = new TaskItem
            {
                Title = taskCreateDto.Title,
                Description = taskCreateDto.Description,
                DueDate = taskCreateDto.DueDate,
                IsCompleted = false,
                ProjectId = projectId
            };

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            // Rota için taskId de gereklidir, ancak şu anlık bu şekilde bırakabiliriz.
            // En doğru hali CreatedAtRoute kullanmaktır. Şimdilik basitleştirelim.
            return Ok(taskItem);
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int projectId, int taskId, TaskUpdateDto taskUpdateDto)
        {
            var userId = GetUserId();
            // İlişki üzerinden güvenlik kontrolü
            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (taskItem == null || taskItem.Project.UserId != userId)
            {
                return NotFound();
            }

            taskItem.Title = taskUpdateDto.Title;
            taskItem.Description = taskUpdateDto.Description;
            taskItem.IsCompleted = taskUpdateDto.IsCompleted;
            taskItem.DueDate = taskUpdateDto.DueDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int projectId, int taskId)
        {
            var userId = GetUserId();
            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);

            if (taskItem == null || taskItem.Project.UserId != userId)
            {
                return NotFound();
            }

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}