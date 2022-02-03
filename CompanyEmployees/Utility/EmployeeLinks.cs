
using Contracts;
using Entities.DataTransferObjects;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace CompanyEmployees.Utility
{
    public class EmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDTO> _dataShaper;

        public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDTO> dataShaper)
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDTO> employeesDTO, string fields, Guid companyID, HttpContext httpContext)
        {
            var shapedEmployees = ShapeData(employeesDTO, fields);

            if (ShouldGenerateLinks(httpContext)) return ReturnLinkedEmployees(employeesDTO, fields, companyID, httpContext, shapedEmployees);

            return ReturnShapedEmployees(shapedEmployees);
        }

        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees)
            => new LinkResponse { ShapedEntities = shapedEmployees };

        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDTO> employeesDTO, string fields, Guid companyID, HttpContext httpContext, List<Entity> shapedEmployees)
        {
            var employeesDTOList = employeesDTO.ToList();
            
            for (int index = 0; index < employeesDTOList.Count(); index++)
            {
                var employeeLinks = CreateLinksForEmployee(httpContext, companyID, employeesDTOList[index].EmployeeID, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployee(httpContext, employeeCollection);

            return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
        }

        private LinkCollectionWrapper<Entity> CreateLinksForEmployee(HttpContext httpContext, LinkCollectionWrapper<Entity> employeeCollectionWrapper)
        {
            employeeCollectionWrapper.Links.Add(new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeesForCompany", values: new { }),
                "self",
                "GET"));

            return employeeCollectionWrapper;
        }

        private List<Link> CreateLinksForEmployee(HttpContext httpContext, Guid companyID, Guid id, string fields = "")
        {
            var links = new List<Link>
            { 
                new Link(_linkGenerator.GetUriByAction(httpContext, "GetEmployeeForCompany", values: new { companyID, id, fields }), "self", "GET"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "DeleteEmployeeForCompany", values: new { companyID, id }), "delete_employee", "DELETE"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "UpdateEmployeeForCompany", values: new { companyID, id }), "update_employee", "PUT"),
                new Link(_linkGenerator.GetUriByAction(httpContext, "PartiallyUpdateEmployeeForCompany", values: new { companyID, id, fields }), "partially_update_employee", "PATCH"),
            };

            return links;
        }

        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            var mediaType = httpContext.Items["AcceptHeaderMediaType"].ToString();
            return mediaType.Contains("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        private List<Entity> ShapeData(IEnumerable<EmployeeDTO> employeesDTO, string fields)
        {
            var shapedData = _dataShaper.ShapeData(employeesDTO, fields);
            var entities = shapedData.Select(e => e.Entity);
            
            return entities.ToList();
        }
    }
}
