using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PulseBridge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly ISender Sender;
    protected BaseApiController(ISender sender) => Sender = sender;
}
