using System.Linq;
using System.Linq.Dynamic.Core;
using Entities.Models;
using Repositories.Extensions.Utility;

namespace Repositories.Extensions
{
    public static class RepositoryCompanyExtensions
    {
        public static IQueryable<Company> Sort(this IQueryable<Company> companies, string orderByQueryString)
        {
            if (string.IsNullOrWhiteSpace(orderByQueryString))
                return companies.OrderBy(e => e.Name);

            var orderQuery = OrderQueryBuilder.CreateOrderQuery<Company>(orderByQueryString);

            if (string.IsNullOrWhiteSpace(orderQuery))
                return companies.OrderBy(e => e.Name);

            return companies.OrderBy(orderQuery);
        }
    }
}
