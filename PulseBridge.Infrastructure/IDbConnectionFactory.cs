using System.Data;

namespace PulseBridge.Infrastructure;

public interface IDbConnectionFactory
{
    Task<IDbConnection> OpenAsync(CancellationToken ct = default);
}
