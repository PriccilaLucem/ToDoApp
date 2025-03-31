using AutoMapper;
using MongoDB.Bson;
using WebApplication.Dto.task;
using WebApplication.Models.TaskModel;

namespace WebApplication.Util
{
    public class TaskMapper: Profile
    {
        public TaskMapper()
        {
            CreateMap<TaskDTO, TaskModel>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(_ => ObjectId.GenerateNewId().ToString()));


            CreateMap<RecurrencePattern, RecurrencePatternDTO>()
                .ReverseMap();
        }
    }
}