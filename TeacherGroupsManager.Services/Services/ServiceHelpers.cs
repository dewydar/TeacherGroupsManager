using Microsoft.EntityFrameworkCore;
using TeacherGroupsManager.Shared.Results;

namespace TeacherGroupsManager.Services.Services;

internal static class ServiceHelpers
{
    public static async Task<OperationResult> SaveDeleteAsync(Func<CancellationToken, Task> saveChanges, string successMessage, string failureMessage, CancellationToken cancellationToken)
    {
        try
        {
            await saveChanges(cancellationToken);
            return OperationResult.Success(successMessage);
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure(failureMessage);
        }
    }
}
