using Application.Model.Warehouse;
using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MappingProfiles
{
    public class WarehouseProfile: Profile
    {
        public WarehouseProfile()
        {
            //CreateMap<Warehouse, WarehouseResponseDto>();
            CreateMap<WarehouseRequestDto, Warehouse>();
        }
    }
}
