using Hidratacao.Domain;

namespace Hidratacao.Application;

public interface IDailySummaryRepository
{
    Task<IReadOnlyList<DailySummary>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAllAsync(IReadOnlyList<DailySummary> summaries, CancellationToken cancellationToken = default);
}
