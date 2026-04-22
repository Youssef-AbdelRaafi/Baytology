using Baytology.Application.Common.Interfaces;
using Baytology.Domain.Common.Enums;
using Baytology.Infrastructure.Settings;

using Microsoft.Extensions.Options;

namespace Baytology.Infrastructure.AI;

internal sealed class RabbitMqAiDispatchPolicy(IOptions<RabbitMqSettings> rabbitOptions) : IAiDispatchPolicy
{
    private readonly RabbitMqSettings _rabbitSettings = rabbitOptions.Value;

    public bool ShouldDeferSearchResolution(SearchInputType inputType)
    {
        return _rabbitSettings.Enabled && inputType == SearchInputType.Text;
    }

    public bool ShouldDeferRecommendationResolution()
    {
        return _rabbitSettings.Enabled;
    }
}
