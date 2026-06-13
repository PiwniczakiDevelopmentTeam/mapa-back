using mapa_back.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace mapa_back.Services
{
	public static class FilterBuilder
	{
		public static IQueryable<T> ApplyFilters<T>(IQueryable<T> query, List<FilterParams> filters)
		{
			foreach (var filter in filters)
			{
				if (string.IsNullOrWhiteSpace(filter.field) || filter.value == null)
					continue;

				var parameter = Expression.Parameter(typeof(T), "x");

				var propertyInfo = typeof(T).GetProperty(filter.field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo == null)
					continue;

				var property = Expression.Property(parameter, propertyInfo);
				Expression? comparison;

				if (propertyInfo.PropertyType == typeof(string))
				{
					var toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
					var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });

					var left = Expression.Call(property, toLower!);
					var right = Expression.Constant(filter.value.ToString()!.ToLower());

					var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

					if (filter.field.Equals("nazwa", StringComparison.OrdinalIgnoreCase))
					{
						var contains = Expression.Call(left, containsMethod!, right);
						comparison = Expression.AndAlso(notNull, contains);
					}
					else
					{
						var equal = Expression.Equal(left, right);
						comparison = Expression.AndAlso(notNull, equal);
					}
				}
				else
				{
					try
					{
						var typedValue = Convert.ChangeType(filter.value, propertyInfo.PropertyType);
						var constant = Expression.Constant(typedValue);
						comparison = Expression.Equal(property, constant);
					}
					catch
					{
						continue;
					}
				}

				var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
				query = query.Where(lambda);
			}

			return query;
		}
	}
}
