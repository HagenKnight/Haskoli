using AutoMapper;
using Haskoli.Domain.Custom;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Entities;

namespace Haskoli.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            /* Mapping PagedList objects. */
            CreateMap(typeof(PagedList<>), typeof(MetaData<>)).ConvertUsing(typeof(ConverterPaging<,>));

            /* Mapping queries and parameters. */

            CreateMap<Country, CountryDTO>().ReverseMap();
            CreateMap<Country, CreateCountryDTO>().ReverseMap();
            CreateMap<CountryDTO, CreateCountryDTO>().ReverseMap();
            CreateMap<Country, UpdateCountryDTO>().ReverseMap();
            CreateMap<CountryDTO, UpdateCountryDTO>().ReverseMap();
            CreateMap<Country, DeleteCountryDTO>().ReverseMap();
            CreateMap<CountryDTO, DeleteCountryDTO>().ReverseMap();
        }
    }
}
