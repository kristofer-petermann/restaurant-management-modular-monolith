using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers;

namespace WebApp.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "RootOnly")]
public abstract class AdminBaseController : AppControllerBase
{
}

