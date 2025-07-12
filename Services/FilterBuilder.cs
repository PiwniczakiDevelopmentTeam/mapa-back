using mapa_back.Data;
using System.Linq.Expressions;

namespace mapa_back.Services
{
	public static class FilterBuilder
	{
		public static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterParams> filters)
		{
			foreach (var filter in filters)
			{
				if (filter.value == null) continue;

				var parameter = Expression.Parameter(typeof(T), "x");
				var property = Expression.Property(parameter, filter.field);
				if(property == null) continue;
				var constant = Expression.Constant(filter.value);

				var equals = Expression.Equal(property, constant);

				var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

				query = query.Where(lambda);
			}

			return query;
		}
	}
}
