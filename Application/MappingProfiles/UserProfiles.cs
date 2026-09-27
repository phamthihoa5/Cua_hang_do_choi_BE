using Application.Model.Auth;
using Application.Model.User;
using AutoMapper;
using Core.Entities.Identity;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.MappingProfiles
{
    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            // Map cho login response
            CreateMap<ApplicationUser, UserLoginResponse>()
                .ForMember(dto => dto.Roles,
                    opt => opt.MapFrom(x => x.Roles.Select(r => r.Role.Name)));

            // Map cho profile
            CreateMap<ApplicationUser, UserProfileDto>()
     .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.Roles.Select(ur => ur.Role.Name)))
     .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
     .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
     .ForMember(dest => dest.StaffType, opt => opt.MapFrom(src => src.StaffType.ToString()))  // Chuyển StaffType thành String để hiển thị
     .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy));


            // Map cho view DTO
            CreateMap<ApplicationUser, UserViewDto>()
                //.ForMember(dest => dest.Roles, opt => opt.MapFrom(x => x.Roles.Select(r => r.Role.Name)))
                //.ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName));

            // Map từ request đăng ký sang DTO
            CreateMap<SignUpRequest, UserProfileDto>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender));
        }
    }
}
