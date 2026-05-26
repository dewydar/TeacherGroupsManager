using Microsoft.AspNetCore.Http;
using TeacherGroupsManager.Dtos;

namespace TeacherGroupsManager.WebUI.Infrastructure;

public static class DataTablesRequestHelper
{
    public static DataTableRequestDto Parse(HttpRequest request)
    {
        var form = request.Form;
        var sortColumnIndex = form["order[0][column]"].FirstOrDefault();
        var sortColumn = string.Empty;

        if (int.TryParse(sortColumnIndex, out var columnIndex))
        {
            sortColumn = form[$"columns[{columnIndex}][data]"].FirstOrDefault() ?? string.Empty;
        }

        return new DataTableRequestDto
        {
            Draw = ParseInt(form["draw"].FirstOrDefault(), 0),
            Start = ParseInt(form["start"].FirstOrDefault(), 0),
            Length = ParseInt(form["length"].FirstOrDefault(), 10),
            SearchValue = form["search[value]"].FirstOrDefault(),
            SortColumn = sortColumn,
            SortDirection = form["order[0][dir]"].FirstOrDefault(),
            Filters = ParseFilters(form)
        };
    }

    private static Dictionary<string, string?> ParseFilters(IFormCollection form)
    {
        var filters = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var key in form.Keys.Where(key => key.StartsWith("filters[", StringComparison.OrdinalIgnoreCase) && key.EndsWith(']')))
        {
            var filterName = key["filters[".Length..^1];
            filters[filterName] = form[key].FirstOrDefault();
        }

        return filters;
    }

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value, out var parsed) ? parsed : fallback;
}
