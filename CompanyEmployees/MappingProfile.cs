using AutoMapper;
using Entities.Models;

namespace Entities.DataTransferObjects
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Object to DTO
            CreateMap<Company, CompanyDTO>()
              .ForMember(c => c.FullAddress, opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

            CreateMap<Employee, EmployeeDTO>();

            //DTO to Object
            CreateMap<CompanyCreateDTO, Company>();

            CreateMap<EmployeeCreateDTO, Employee>();

            CreateMap<EmployeeUpdateDTO, Employee>();
        }
    }
}
