namespace WebApp.Tests.Helpers;

public static class TestUsers
{
    public const string Password = "Test.pass1";

    public static readonly Guid UserAId = new("11111111-1111-1111-1111-111111111111");
    public const string UserAEmail = "user-a@test.ee";

    public static readonly Guid UserBId = new("22222222-2222-2222-2222-222222222222");
    public const string UserBEmail = "user-b@test.ee";

    public static readonly Guid UserAItemId = new("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid UserBItemId = new("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public const string UserAItemEnSummary = "user-a item summary EN";
    public const string UserAItemEtSummary = "user-a item summary ET";
    public const string UserBItemEnSummary = "user-b item summary EN";
    public const string UserBItemEtSummary = "user-b item summary ET";

    public const string UserAItemDescription = "user-a item";
    public const string UserBItemDescription = "user-b item";

    public static readonly Guid RestaurantAId = new("aaaaaaaa-0000-0000-0000-000000000001");
    public static readonly Guid RestaurantBId = new("bbbbbbbb-0000-0000-0000-000000000001");

    public static readonly Guid RestaurantUserAId = new("aaaaaaaa-0000-0000-0000-000000000002");
    public static readonly Guid RestaurantUserBId = new("bbbbbbbb-0000-0000-0000-000000000002");
    public static readonly Guid RestaurantUserForeignInAId = new("cccccccc-0000-0000-0000-000000000002");

    public static readonly Guid MenuAId = new("aaaaaaaa-0000-0000-0000-000000000003");
    public static readonly Guid MenuBId = new("bbbbbbbb-0000-0000-0000-000000000003");

    public static readonly Guid MenuCategoryAId = new("aaaaaaaa-0000-0000-0000-000000000004");
    public static readonly Guid MenuCategoryBId = new("bbbbbbbb-0000-0000-0000-000000000004");

    public static readonly Guid MenuItemAId = new("aaaaaaaa-0000-0000-0000-000000000005");
    public static readonly Guid MenuItemBId = new("bbbbbbbb-0000-0000-0000-000000000005");

    public static readonly Guid OpeningHourAId = new("aaaaaaaa-0000-0000-0000-000000000006");
    public static readonly Guid OpeningHourBId = new("bbbbbbbb-0000-0000-0000-000000000006");

    public static readonly Guid OrderAId = new("aaaaaaaa-0000-0000-0000-000000000007");
    public static readonly Guid OrderBId = new("bbbbbbbb-0000-0000-0000-000000000007");

    public static readonly Guid OrderRowAId = new("aaaaaaaa-0000-0000-0000-000000000008");
    public static readonly Guid OrderRowBId = new("bbbbbbbb-0000-0000-0000-000000000008");

    public static readonly Guid PaymentAId = new("aaaaaaaa-0000-0000-0000-000000000009");
    public static readonly Guid PaymentBId = new("bbbbbbbb-0000-0000-0000-000000000009");

    public static readonly Guid ReviewAId = new("aaaaaaaa-0000-0000-0000-000000000010");
    public static readonly Guid ReviewBId = new("bbbbbbbb-0000-0000-0000-000000000010");
}
