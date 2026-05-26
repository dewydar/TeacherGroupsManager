using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.Services.Services;

internal static class DataTableQueryHelper
{
    public static async Task<DataTableResponseDto<TDto>> ToDataTableResponseAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        DataTableRequestDto request,
        Func<IQueryable<TEntity>, DataTableRequestDto, IQueryable<TEntity>> applyFilters,
        Func<IQueryable<TEntity>, string?, IQueryable<TEntity>> applySearch,
        Func<IQueryable<TEntity>, string?, string?, IQueryable<TEntity>> applySorting,
        Func<List<TEntity>, List<TDto>> map,
        CancellationToken cancellationToken)
        where TEntity : class
    {
        var recordsTotal = await query.CountAsync(cancellationToken);
        query = applyFilters(query, request);
        query = applySearch(query, request.SearchValue);
        var recordsFiltered = await query.CountAsync(cancellationToken);

        var length = request.Length <= 0 ? 10 : request.Length;
        var data = await applySorting(query, request.SortColumn, request.SortDirection)
            .Skip(Math.Max(0, request.Start))
            .Take(length)
            .ToListAsync(cancellationToken);

        return new DataTableResponseDto<TDto>
        {
            Draw = request.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
            Data = map(data)
        };
    }

    public static string? Filter(this DataTableRequestDto request, string key) =>
        request.Filters.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    public static int? FilterInt(this DataTableRequestDto request, string key) =>
        int.TryParse(request.Filter(key), out var value) ? value : null;

    public static bool? FilterBool(this DataTableRequestDto request, string key) =>
        bool.TryParse(request.Filter(key), out var value) ? value : null;

    public static DateOnly? FilterDateOnly(this DataTableRequestDto request, string key) =>
        DateOnly.TryParse(request.Filter(key), out var value) ? value : null;

    public static DateTime? FilterDateTime(this DataTableRequestDto request, string key) =>
        DateTime.TryParse(request.Filter(key), out var value) ? value.Date : null;

    public static bool Descending(string? direction) =>
        string.Equals(direction, "desc", StringComparison.OrdinalIgnoreCase);
}
