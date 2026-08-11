namespace Shared.Contracts.Identity;

public class RestaurantUserDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public RestaurantUserRole UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }
}

public class RestaurantUserReadDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public Guid UserId { get; set; }
    public string? UserEmail { get; set; }
    public RestaurantUserRole UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }
}

public class AllowedRestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserOptionDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? UserName { get; set; }
}

public enum RestaurantUserRole
{
    Normal,
    Admin
}
