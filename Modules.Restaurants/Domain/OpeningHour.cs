using Base.Domain;

namespace Modules.Restaurants.Domain;

public class OpeningHour : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }

    public LangStr DayOfWeek { get; set; } = default!;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public LangStr? Summary { get; set; } = default!;
}
