using AutoMapper;
using MongoDB.Bson;
using WebApplication.Dto.user;
using WebApplication.Dto.User;
using WebApplication.Models;
using WebApplication.Util;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Mapeamento CreateUserDTO → UserModel
        CreateMap<CreateUserDTO, UserModel>()
            .ForMember(dest => dest.Id, 
                opt => opt.MapFrom(_ => ObjectId.GenerateNewId().ToString()))
            .ForMember(dest => dest.Password,
                opt => opt.MapFrom(src => Auth.HashPasswordUtil(src.Password))) 
            .ForMember(dest => dest.CreatedAt,
                opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(_ => true))
            .AfterMap((src, dest) => 
            {
                dest.Email = src.Email.ToLower().Trim();
            });

        CreateMap<UserModel, UserResponseDTO>();

        CreateMap<UpdatedUserDTO, UserModel>()
            .ForMember(dest => dest.UpdatedAt,
                opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opts => 
                opts.Condition((src, dest, srcMember) => 
                    srcMember != null)); 
    }
}