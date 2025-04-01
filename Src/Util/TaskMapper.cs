using AutoMapper;
using MongoDB.Bson;
using WebApplication.Src.Dto.task;
using WebApplication.Src.Models.TaskModel;

namespace WebApplication.Src.Util
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