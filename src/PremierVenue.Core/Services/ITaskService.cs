using PremierVenue.Core.DTOs;
namespace PremierVenue.Core.Services;

public interface ITaskService
{
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<List<TaskDto>> GetBookingTasksAsync(int bookingId);
    Task<List<TaskDto>> GetUserTasksAsync(int userId);
    Task<TaskDto?> CreateTaskAsync(CreateTaskDto model);
    Task<TaskDto?> UpdateTaskAsync(int id, UpdateTaskDto model);
    Task<bool> DeleteTaskAsync(int id);
    Task<bool> CompleteTaskAsync(int id);
}