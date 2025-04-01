
using WebApplication.Src.Models.TaskModel;

namespace WebApplication.Src.Interface
{
    public interface TaskInterface
    {
        public Task<string> CreateTaskView(TaskModel task);
        public Task<TaskModel> UpdateTaskView(TaskModel task);
        public Task<List<TaskModel>> ListTaskView();

        public Task<bool> DeleteTaskView(string taskId);

        public Task<TaskModel> GetOneTaskView(string taskId);
    }
}