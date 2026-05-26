namespace TeacherGroupsManager.Dtos;

public class DataTableRequestDto
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; } = 10;
    public string? SearchValue { get; set; }
    public string? SortColumn { get; set; }
    public string? SortDirection { get; set; }
    public Dictionary<string, string?> Filters { get; set; } = [];
}

public class DataTableResponseDto<T>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<T> Data { get; set; } = [];
}
