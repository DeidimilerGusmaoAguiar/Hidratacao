using Hidratacao.Domain;

namespace Hidratacao.Application;

public interface ISettingsRepository
{
    Task<Settings> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(Settings settings, CancellationToken cancellationToken = default);
}
