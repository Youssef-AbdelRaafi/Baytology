using Baytology.Domain.Common.Enums;

namespace Baytology.Application.Common.Interfaces;

public interface IAiDispatchPolicy
{
    bool ShouldDeferSearchResolution(SearchInputType inputType);

    bool ShouldDeferRecommendationResolution();
}
