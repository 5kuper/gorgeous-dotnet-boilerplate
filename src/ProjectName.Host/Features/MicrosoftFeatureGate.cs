using Microsoft.FeatureManagement;
using Shared.AppModel.Abstractions;

namespace ProjectName.Host.Features;

internal sealed class MicrosoftFeatureGate(IFeatureManagerSnapshot featureManager) : IFeatureGate
{
    public Task<bool> IsEnabledAsync(string feature, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);

        return featureManager.IsEnabledAsync(feature);
    }
}
