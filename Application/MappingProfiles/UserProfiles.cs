using Application.Model.User;
using AutoMapper;
using Core.Entities.Identity;

namespace Application.MappingProfiles
{
    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            // ==============================
            // MAP CHO LOGIN RESPONSE
            // ==============================
            CreateMap<ApplicationUser, UserLoginResponse>()
                .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(src =>
                        src.Roles.Select(r => r.Role.Name))
                );

            // ==============================
            // MAP CHO PROFILE NHAN VIEN
            // ==============================
            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(
                    dest => dest.Roles,
                    opt => opt.MapFrom(src =>
                        src.Roles.Select(ur => ur.Role.Name))
                )
                .ForMember(
                    dest => dest.Gender,
                    opt => opt.MapFrom(src => src.Gender)
                )
                .ForMember(
                    dest => dest.Address,
                    opt => opt.MapFrom(src => src.Address)
                )
                .ForMember(
                    dest => dest.StaffType,
                    opt => opt.MapFrom(src => src.StaffType)
                )
                .ForMember(
                    dest => dest.CreatedBy,
                    opt => opt.MapFrom(src => src.CreatedBy)
                );
        }
    }
}