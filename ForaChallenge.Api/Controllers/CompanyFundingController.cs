using Microsoft.AspNetCore.Mvc;

namespace ForaChallenge.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CompanyFundingController : ControllerBase
{
    private readonly ILogger<CompanyFundingController> _logger;
    private readonly IEdgarService _edgarService;

    public CompanyFundingController(ILogger<CompanyFundingController> logger, IEdgarService edgarService)
    {
        _logger = logger;
        _edgarService = edgarService;
    }

    [HttpGet(Name = "GetCompanyFunding")]
    public async Task<IActionResult> Get(string nameLimiter, CancellationToken token)
    {
        try
        {
            var results = await _edgarService.GetCompanyFundingDataAsync(nameLimiter, token);
            return Ok(results);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request was canceled.");
            return StatusCode(StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company funding data.");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }
}