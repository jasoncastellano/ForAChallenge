using System.Collections.Concurrent;

namespace ForaChallenge.Api;

public interface IEdgarService
{
    Task<IEnumerable<CompanyFundingResponse>> GetCompanyFundingDataAsync(string nameLimiter, CancellationToken token);
}

public class EdgarService : IEdgarService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EdgarService> _logger;
    private readonly int _maxParallelRequests;

    public EdgarService(HttpClient httpClient, ILogger<EdgarService> logger, int maxParallelRequests = 5)
    {
        _httpClient = httpClient;
        _logger = logger;
        _maxParallelRequests = maxParallelRequests;
    }

    public async Task<IEnumerable<CompanyFundingResponse>> GetCompanyFundingDataAsync(string nameLimiter, CancellationToken token)
    {
        var edgarCompanyInfos = new ConcurrentQueue<CompanyFundingResponse>();

        await Parallel.ForEachAsync(Constants.CompanyCentralIndexKeys, new ParallelOptions
        {
            MaxDegreeOfParallelism = _maxParallelRequests,
            CancellationToken = token
        },
        async (companyId, cancellationToken) =>
        {
            try
            {
                var company = await GetEdgarCompanyInfoAsync(companyId, cancellationToken);

                if (company?.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd != null &&
                    (string.IsNullOrWhiteSpace(nameLimiter) || company.EntityName.StartsWith(nameLimiter, StringComparison.CurrentCultureIgnoreCase)))
                {
                    var response = new CompanyFundingResponse
                    {
                        Id = company.Cik,
                        Name = company.EntityName,
                        StandardFundableAmount = company.GetStandardFundableAmount(),
                        SpecialFundableAmount = company.GetSpecialFundableAmount()
                    };

                    edgarCompanyInfos.Enqueue(response);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred while fetching data for company with ID {companyId}");
            }
        });

        return edgarCompanyInfos.OrderBy(p => p.Name);
    }

    private async Task<EdgarCompanyInfo> GetEdgarCompanyInfoAsync(int companyId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"{Constants.EdgarUrl}/CIK{companyId:D10}.json", cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content
                .ReadFromJsonAsync<EdgarCompanyInfo>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        
        _logger.LogWarning($"Failed to fetch data for company with ID {companyId}. Status code: {response.StatusCode}");
        return null;

    }
}