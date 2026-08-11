using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

public abstract class AppControllerBase : Controller
{
    protected bool IsRoot()
    {
        return User.IsInRole("root");
    }
}
