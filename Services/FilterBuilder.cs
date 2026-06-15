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

				ParameterExpression parameter = Expression.Parameter(typeof(T), "x");

				PropertyInfo? propertyInfo = typeof(T).GetProperty(filter.field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo == null)
					continue;

				MemberExpression property = Expression.Property(parameter, propertyInfo);
				Expression? comparison;

				if (propertyInfo.PropertyType == typeof(string))
				{
					MethodInfo? toLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);
					MethodInfo? containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });

					MethodCallExpression left = Expression.Call(property, toLower!);
					ConstantExpression right = Expression.Constant(filter.value.ToString()!.ToLower());

					BinaryExpression notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));

					if (filter.field.Equals("nazwa", StringComparison.OrdinalIgnoreCase))
					{
						MethodCallExpression contains = Expression.Call(left, containsMethod!, right);
						comparison = Expression.AndAlso(notNull, contains);
					}
					else
					{
						BinaryExpression equal = Expression.Equal(left, right);
						comparison = Expression.AndAlso(notNull, equal);
					}
				}
				else
				{
					try
					{
						object typedValue = Convert.ChangeType(filter.value, propertyInfo.PropertyType);
						ConstantExpression constant = Expression.Constant(typedValue);
						comparison = Expression.Equal(property, constant);
					}
					catch
					{
						continue;
					}
				}

				Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
				query = query.Where(lambda);
			}

			return query;
		}
	}
}