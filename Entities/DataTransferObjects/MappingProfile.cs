using AutoMapper;
using Entities.Models;

namespace Entities.DataTransferObjects
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));

            //mapping rule for the Employee and EmployeeDto objects
            CreateMap<Employee, EmployeeDto>();

            //mapping rule for the Company and CompanyForCreationDto objects
            CreateMap<CompanyForCreationDto, Company>();

            //mapping rule for the Employee and EmployeeForCreationDto objects
            CreateMap<EmployeeForCreationDto, Employee>(); //.ReverseMap()
        }
    }
}
