using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Core.Entities;
using TeacherGroupsManager.Data.Repositories;
using TeacherGroupsManager.Dtos;
using TeacherGroupsManager.Services.Interfaces;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

public class AcademicYearService(IUnitOfWork unitOfWork, IMapper mapper) : IAcademicYearService
{
    public async Task<IReadOnlyList<AcademicYearDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        mapper.Map<List<AcademicYearDto>>(await unitOfWork.Repository<AcademicYear>().Query().OrderByDescending(x => x.StartDate).ToListAsync(cancellationToken));

    public async Task<AcademicYearDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        mapper.Map<AcademicYearDto?>(await unitOfWork.Repository<AcademicYear>().GetByIdAsync(id, cancellationToken));

    public async Task<OperationResult> CreateAsync(CreateAcademicYearDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure("السنة الدراسية موجودة من قبل");
        }

        await unitOfWork.Repository<AcademicYear>().AddAsync(new AcademicYear { Name = name, StartDate = dto.StartDate, EndDate = dto.EndDate, IsActive = dto.IsActive }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم حفظ السنة الدراسية بنجاح");
    }

    public async Task<OperationResult> UpdateAsync(EditAcademicYearDto dto, CancellationToken cancellationToken = default)
    {
        var year = await unitOfWork.Repository<AcademicYear>().GetByIdAsync(dto.Id, cancellationToken);
        if (year is null) return OperationResult.Failure("السنة الدراسية غير موجودة");
        var name = dto.Name.Trim();
        var normalizedName = name.ToLower();
        if (await unitOfWork.Repository<AcademicYear>().AnyAsync(x => x.Id != dto.Id && x.Name.Trim().ToLower() == normalizedName, cancellationToken))
        {
            return OperationResult.Failure("السنة الدراسية موجودة من قبل");
        }

        year.Name = name;
        year.StartDate = dto.StartDate;
        year.EndDate = dto.EndDate;
        year.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("تم تعديل السنة الدراسية بنجاح");
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var year = await unitOfWork.Repository<AcademicYear>().GetByIdAsync(id, cancellationToken);
        if (year is null) return OperationResult.Failure("السنة الدراسية غير موجودة");
        unitOfWork.Repository<AcademicYear>().Delete(year);
        return await ServiceHelpers.SaveDeleteAsync(unitOfWork.SaveChangesAsync, "تم حذف السنة الدراسية بنجاح", cancellationToken);
    }
}
