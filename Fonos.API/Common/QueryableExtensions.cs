using Fonos.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Fonos.API.Common
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            return query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortBy) where T : class
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query;

            var allowedProperties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var sortExpressions = new List<string>();

            foreach (var part in sortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0 || !allowedProperties.Contains(tokens[0]))
                    continue;

                var direction = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase)
                    ? "descending"
                    : "ascending";

                sortExpressions.Add($"{tokens[0]} {direction}");
            }

            return sortExpressions.Count > 0
                ? query.OrderBy(string.Join(", ", sortExpressions))
                : query;
        }

        public static IQueryable<Book> ApplySearch(this IQueryable<Book> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            return query.Where(m =>
                EF.Functions.Like(m.Title, $"%{search}%") ||
                EF.Functions.Like(m.Author.Name, $"%{search}%"));
        }

        public static IQueryable<Category> ApplySearch(this IQueryable<Category> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            return query.Where(m =>EF.Functions.Like(m.Name, $"%{search}%"));
        }

        public static IQueryable<Author> ApplySearch(this IQueryable<Author> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            return query.Where(m => EF.Functions.Like(m.Name, $"%{search}%"));
        }
    }
}
