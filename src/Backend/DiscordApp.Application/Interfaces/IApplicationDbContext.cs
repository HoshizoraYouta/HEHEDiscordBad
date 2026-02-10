using DiscordApp.Domain.Entities;

namespace DiscordApp.Application.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
