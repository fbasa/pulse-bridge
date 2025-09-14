using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace PulseBridge.Payment.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "payments.read")]
    public IActionResult GetPayments() => Ok(new[] { new { id = 1, amount = 2500 } });

    [HttpPost]
    [Authorize(Policy = "payments.write")]
    public IActionResult CreatePayment([FromBody] object payload) => Created(string.Empty, new { ok = true });
}
