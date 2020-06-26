using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(RepositoryContext repositoryContext) : base(repositoryContext)
        {

        }

        public IEnumerable<Company> GetAllCompanies(bool trackChanges) =>
            FindAll(trackChanges)
            .OrderBy(c => c.Name)
            .ToList();

        public Company GetCompany(Guid companyId, bool trackChanges) =>
            FindByCondition(c => c.Id.Equals(companyId), trackChanges)
            .SingleOrDefault();

        public void CreateCompany(Company company) => Create(company);

        //get child Employee resources added alongside Company
        public List<Employee> GetEmployees(IEnumerable<EmployeeForCreationDto> Employees)
        {
            List<Employee> employees = new List<Employee>();
            foreach (EmployeeForCreationDto comp in Employees)
            {
                employees.Add(new Employee {Name = comp.Name, Age = comp.Age, Position = comp.Position });
            }

            return employees;
        }

        public IEnumerable<Company> GetByIds(IEnumerable<Guid> ids, bool trackChanges) =>
            FindByCondition(x => ids.Contains(x.Id), trackChanges)
            .ToList();

        public void DeleteCompany(Company company) =>
            Delete(company);
    }
}
