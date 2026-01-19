using Hidratacao.Domain;

namespace Hidratacao.Application;

public interface IWaterEventRepository
{
    Task<IReadOnlyList<WaterEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAllAsync(IReadOnlyList<WaterEvent> events, CancellationToken cancellationToken = default);
}
