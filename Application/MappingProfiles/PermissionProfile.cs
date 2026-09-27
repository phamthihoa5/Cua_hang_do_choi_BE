using Application.Model.Permission;
using Application.Model.User;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

namespace Application.Common.Mapping
{
    public class PermissionProfile : Profile
    {
        public PermissionProfile()
        {
            // ===== MAPPING FROM STAFFTYPE (ENUM) TO STRING =====
            // Mapping khi tạo UserProfileDTO
            CreateMap<ApplicationUser, UserProfileDto>()
                .ForMember(dest => dest.StaffType, opt => opt.MapFrom(src => src.StaffType.ToString())); // Convert StaffType Enum to String

            // ===== MAP PERMISSION TO PERMISSION DTO =====
            CreateMap<Permission, PermissionDto>()
                .ForMember(dest => dest.IsGranted, opt => opt.Ignore());

            // ===== MAP USER PERMISSION TO PERMISSION DTO =====
            CreateMap<UserPermission, PermissionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(dest => dest.IsGranted, opt => opt.MapFrom(src => src.IsGranted));

            // ===== MAP STAFF TYPE PERMISSION =====
            CreateMap<StaffTypePermission, PermissionDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Permission.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Permission.Code))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Permission.Name))
                .ForMember(dest => dest.IsGranted, opt => opt.MapFrom(src => src.IsGranted));

            // ===== MAP STAFF TYPE PERMISSION TO STAFF TYPE PERMISSION DTO =====
            CreateMap<StaffTypePermission, StaffTypePermissionDto>()
                .ForMember(dest => dest.StaffType, opt => opt.MapFrom(src => src.StaffType.ToString()))  // Mapping Enum to String
                .ForMember(dest => dest.Permissions, opt => opt.Ignore());  // Ignore permissions field (can be handled separately)

            // ===== MAP UPDATE STAFF TYPE PERMISSION DTO =====
            CreateMap<UpdateStaffTypePermissionDto, StaffTypePermission>()
                .ForMember(dest => dest.StaffType, opt => opt.MapFrom(src => src.StaffType))
                .ForMember(dest => dest.PermissionId, opt => opt.MapFrom(src => src.PermissionIds)); // Mapping permission IDs to StaffTypePermission
        }
    }
}
