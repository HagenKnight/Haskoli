using AutoMapper;
using Haskoli.Application.Features.Student;
using Haskoli.Domain.Custom;
using Haskoli.Domain.DTO;
using Haskoli.Domain.Entities;
using Haskoli.Domain.Parameters;

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

            CreateMap<Student, StudentDTO>().ReverseMap();
            /* Id se ignora al crear: la columna es identidad, así que un id enviado por el
               cliente no debe llegar a la inserción. */
            CreateMap<CreateStudentDTO, Student>().ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<Student, CreateStudentDTO>();
            CreateMap<StudentDTO, CreateStudentDTO>().ReverseMap();
            CreateMap<Student, UpdateStudentDTO>().ReverseMap();
            CreateMap<StudentDTO, UpdateStudentDTO>().ReverseMap();
            CreateMap<Student, DeleteStudentDTO>().ReverseMap();
            CreateMap<StudentDTO, DeleteStudentDTO>().ReverseMap();

            CreateMap<GetAllStudentQuery, GetAllStudentParameter>();
        }
    }
}
