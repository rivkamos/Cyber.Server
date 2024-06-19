using Cyberbit.TaskManager.Server.Interfaces;
using Cyberbit.TaskManager.Server.Models;
using Cyberbit.TaskManager.Server.Models.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cyberbit.TaskManager.Server.Dal
{
    public class TasksDal : ITasksDal
    {
        private readonly ILogger<TasksDal> _logger;
        private readonly BackendDbContext _dbContext;

        public TasksDal(ILogger<TasksDal> logger, BackendDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<IList<Models.Task>> GetAllTask()
        {
            _logger.LogInformation($"GetAllTask - Enter");
            var retValue = await _dbContext.Tasks.AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.User).ToListAsync();
            _logger.LogInformation($"GetAllTask - Exit");
            return retValue;
        }

        public async Task<Models.Task> GetTaskById(int id)
        {
            _logger.LogInformation($"GetTaskById - Enter");
            var retValue = await _dbContext.Tasks.AsNoTracking()
                .Include(t => t.CreatedByUser)
                .Include(t => t.User)
                .FirstOrDefaultAsync(task => task.Id == id);
            _logger.LogInformation($"GetTaskById - Exit");
            return retValue;
        }

        public async Task<Models.Task> AddTask(Models.Task task)
        {
            _logger.LogInformation($"AddTask - Enter");
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"AddTask - Exit");
            return task;
        }

        public async Task<Models.Task> UpdateTask(Models.Task task)
        {
            _logger.LogInformation($"UpdateTask - Enter");
            var updatedEntity = _dbContext.Tasks.Update(task);
            await _dbContext.SaveChangesAsync();
            updatedEntity.State = EntityState.Detached;
            _logger.LogInformation($"UpdateTask - Exit");
            return updatedEntity.Entity;
        }

        public async Task<Models.Task> DeleteTaskById(int id)
        {
            _logger.LogInformation($"DeleteTaskById - Enter");
            var taskToDelete = new Models.Task { Id = id };
            var deletedEntity = _dbContext.Tasks.Remove(taskToDelete);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"DeleteTaskById - Exit");
            return deletedEntity.Entity;
        }

        public async Task<IList<Models.Task>> DoneAll(int employeeId)
        {
            _logger.LogInformation($"DoneAll - Enter");
            var updateEntities = _dbContext.Tasks.Where(task => task.UserId == employeeId &&
                                task.Status == Models.Enums.TasksStatus.Open).ToList();
            updateEntities.ForEach(task => task.Status = Models.Enums.TasksStatus.Done);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation($"DoneAll - Exit");
            return updateEntities;
        }

        public async Task<Models.Task> AddTaskCategories(List<Category> categories, int taskId)
        {
            _logger.LogInformation($"AddTaskCategories - Enter");
            var currentTaskValue = await _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(task => task.Id == taskId);
            if (currentTaskValue != null)
            {
                currentTaskValue.Categories = categories;
                var updatedEntity = _dbContext.Tasks.Update(currentTaskValue);
                await _dbContext.SaveChangesAsync();
                updatedEntity.State = EntityState.Detached;
                _logger.LogInformation($"AddTaskCategories - Exit");
                return updatedEntity.Entity;
            }
            return null;
        }
    }
}
