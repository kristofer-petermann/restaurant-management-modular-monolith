using Base.DAL.EF;

namespace Modules.Restaurants.Infrastructure;

internal class RestaurantsUow : BaseUow, IRestaurantsUow
{
    public RestaurantsUow(RestaurantsDbContext dbContext) : base(dbContext)
    {
    }
}
