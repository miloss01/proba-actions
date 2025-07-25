using DockerHubBackend.Dto.Request;
using DockerHubBackend.Dto.Response;
using DockerHubBackend.Services.Interface;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System;
using System.Linq;

namespace DockerHubBackend.Controllers
{
    [Route("api/log")]
    [ApiController]
    public class LogSearchController : ControllerBase
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogService _logService;

        public LogSearchController(IElasticClient elasticClient, ILogService logService)
        {
            _elasticClient = elasticClient;
            _logService = logService;
        }

        [HttpPost("search")]
        public IActionResult SearchLogs([FromBody] LogSearchDto request)
        {
            try
            {
                bool noCriteriaProvided = (string.IsNullOrWhiteSpace(request.Query) &&
                                           string.IsNullOrWhiteSpace(request.Level) &&
                                           !request.StartDate.HasValue &&
                                           !request.EndDate.HasValue);

                QueryContainer query = new QueryContainer();

                // no critera
                if (noCriteriaProvided)
                {
                    var defaultResponse = _elasticClient.Search<LogDto>(s => s
                        .Sort(sort => sort.Descending(f => f.Timestamp))
                        .Size(100)
                    );

                    if (!defaultResponse.IsValid)
                    {
                        return BadRequest(defaultResponse.DebugInformation);
                    }

                    var defaultLogs = defaultResponse.Hits.Select(hit => hit.Source).ToList();
                    return Ok(defaultLogs);
                }

                // 1. query
                if (!string.IsNullOrWhiteSpace(request.Query))
                {
                    request.Query = this._logService.NormalizeQuery(request.Query);
                    bool containsLevel = request.Query.Contains("level:", StringComparison.OrdinalIgnoreCase);

                    query &= new QueryContainerDescriptor<LogDto>()
                           .QueryString(q => q
                               .Query(request.Query)
                               .Fields(f =>
                                   containsLevel
                                       ? f.Field(p => p.Message).Field(p => p.Level)
                                       : f.Field(p => p.Message)));
                }

                // 2. date and time filter
                if (request.StartDate.HasValue || request.EndDate.HasValue)
                {
                    query &= new QueryContainerDescriptor<LogDto>()
                        .DateRange(dr => dr
                            .Field(f => f.Timestamp)
                            .GreaterThanOrEquals(request.StartDate)
                            .LessThanOrEquals(request.EndDate));
                }

                // 3. level filter
                if (!string.IsNullOrWhiteSpace(request.Level))
                {
                    query &= new QueryContainerDescriptor<LogDto>()
                        .QueryString(q => q
                            .Fields(f => f.Field(p => p.Level))
                            .Query(request.Level));
                }

                // send query to ElasticSearch
                var response = _elasticClient.Search<LogDto>(s => s
                    .Query(q => query)
                    .Sort(sort => sort.Descending(f => f.Timestamp))
                    .Size(100)
                );

                if (!response.IsValid)
                {
                    return BadRequest(response.DebugInformation);
                }

                var logs = response.Hits.Select(hit => hit.Source).ToList();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
