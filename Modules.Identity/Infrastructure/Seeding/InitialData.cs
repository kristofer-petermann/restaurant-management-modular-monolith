namespace Modules.Identity.Infrastructure.Seeding;

public static class InitialData
{
    public static readonly string[] Roles =
    [
        "user",
        "admin",
        "root"
    ];

    public static readonly (string email, string password, string[] roles)[] Users =
    [
        ("admin@taltech.ee", "Kala.12345", ["admin"]),
        ("user@taltech.ee", "Kala.12345", ["user"]),
        ("root@taltech.ee", "Kala.12345", ["root"]),
    ];
}
