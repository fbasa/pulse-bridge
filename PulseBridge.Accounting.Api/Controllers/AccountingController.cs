using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PulseBridge.Accounting.Api.Controllers;

[ApiController]
[Route("api/accounting")]
public class AccountingController : Controller
{
    [HttpGet]
    [Authorize(Policy = "accounting.read")]
    public IActionResult GetPayments() => Ok(new[] { new { id = 1, amount = 1500 } });

    [HttpPost]
    [Authorize(Policy = "accounting.write")]
    public IActionResult CreatePayment([FromBody] object payload) => Created(string.Empty, new { ok = true });
}