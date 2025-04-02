using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Src.Interface;
using WebApplication.Src.Models.TaskModel;

namespace WebApplication.WebApplication.Tests.Interface
{
    public interface IFakeTaskView: ITaskViews
    {

        new Task<string> CreateTaskView(TaskModel task);
        new Task<TaskModel> UpdateTaskView(TaskModel task);
        new Task<List<TaskModel>> ListTaskView();
        new Task<bool> DeleteTaskView(string taskId);
        new Task<TaskModel?> GetOneTaskView(string taskId);
    }
}