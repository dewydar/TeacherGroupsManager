using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Services.Mapping;
using TeacherGroupsManager.Shared.Localization;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class AcademicYearService(IUnitOfWork unitOfWork, AppMapper mapper, IStringLocalizer<SharedResource> localizer) : IAcademicYearService
{
    public async Task<IReadOnlyList<AcademicYearDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map(await unitOfWork.Repository<AcademicYear>().Query().OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken));

    public async Task<AcademicYearDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await unitOfWork.Repository<AcademicYear>().GetByIdAsync(id, cancellationToken) is { } academicYear ? mapper.Map(academicYear) : null;

    public Task<DataTableResponseDto<AcademicYearDto>> GetPagedAsync(DataTableRequestDto request, CancellationToken cancellationToken = default) =>
        DataTableQueryHelper.ToDataTableResponseAsync(
            unitOfWork.Repository<AcademicYear>().Query().AsNoTracking(),
            request,
            ApplyFilters,
            ApplySearch,
            ApplySorting,
            mapper.Map,
            cancellationToken);

    public async Task<OperationResult> CreateAsync(CreateAcademicYearDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        if (dto.MonthlyPrice < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateAcademicYear"]);
        }

        await unitOfWork.Repository<AcademicYear>().AddAsync(new AcademicYear { Name = name, StartDate = dto.StartDate, EndDate = dto.EndDate, MonthlyPrice = dto.MonthlyPrice, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["AcademicYearSaved"]);
    }

    private static IQueryable<AcademicYear> ApplyFilters(IQueryable<AcademicYear> query, DataTableRequestDto request)
    {
        if (request.Filter("name") is { } name) query = query.Where(x => x.Name.Contains(name));
        if (request.FilterDateOnly("startDate") is { } startDate) query = query.Where(x => x.StartDate >= startDate);
        if (request.FilterDateOnly("endDate") is { } endDate) query = query.Where(x => x.EndDate <= endDate);
        if (request.FilterBool("isActive") is { } isActive) query = query.Where(x => x.IsActive == isActive);
        return query;
    }

    private static IQueryable<AcademicYear> ApplySearch(IQueryable<AcademicYear> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        search = search.Trim();
        return query.Where(x => x.Name.Contains(search));
    }

    private static IQueryable<AcademicYear> ApplySorting(IQueryable<AcademicYear> query, string? sortColumn, string? sortDirection)
    {
        var desc = DataTableQueryHelper.Descending(sortDirection);
        return sortColumn switch
        {
            "name" => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
            "startDate" => desc ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
            "endDate" => desc ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
            "monthlyPrice" => desc ? query.OrderByDescending(x => x.MonthlyPrice) : query.OrderBy(x => x.MonthlyPrice),
            "isActive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
            _ => query.OrderByDescending(x => x.StartDate)
        };
    }

    public async Task<OperationResult> UpdateAsync(EditAcademicYearDto dto, CancellationToken cancellationToken = default)
    {
        var year = await unitOfWork.Repository<AcademicYear>().GetByIdAsync(dto.Id, cancellationToken);
        if (year is null) return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        var name = dto.Name.Trim();
        if (dto.MonthlyPrice < 0) return OperationResult.Failure(localizer["MonthlyPriceCannotBeNegative"]);
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id != dto.Id && x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure(localizer["DuplicateAcademicYear"]);
        }

        year.Name = name;
        year.StartDate = dto.StartDate;
        year.EndDate = dto.EndDate;
        year.MonthlyPrice = dto.MonthlyPrice;
        year.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success(localizer["AcademicYearUpdated"]);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var year = await unitOfWork.Repository<AcademicYear>().GetByIdAsync(id, cancellationToken);
        if (year is null) return OperationResult.Failure(localizer["AcademicYearNotFound"]);
        unitOfWork.Repository<AcademicYear>().Delete(year);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, localizer["AcademicYearDeleted"], localizer["CannotDeleteLinkedRecord"], cancellationToken);
    }
}
