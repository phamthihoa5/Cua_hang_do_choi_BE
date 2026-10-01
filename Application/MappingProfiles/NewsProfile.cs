using Application.Model.New;
using Application.Model.News;
using AutoMapper;
using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.MappingProfiles
{
    public class NewsProfile : Profile
    {
        public NewsProfile()
        {
            CreateMap<News, NewsResponse>();
            CreateMap<NewsRequestDto, News>();
        }
    }
}
