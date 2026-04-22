using Microsoft.AspNetCore.Mvc;

namespace Baytology.Api.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
public abstract class ApiController : ApiControllerBase;
