namespace Shared.AppModel.Abstractions;

public interface IFeatureGate
{
    Task<bool> IsEnabledAsync(string feature, CancellationToken ct = default);
}
