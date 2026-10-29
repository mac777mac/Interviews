using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace Boovatech.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerRetriver _ledgerRetriever;

        public LedgerController(ILedgerRetriver ledgerRetriever)
        {
            _ledgerRetriever = ledgerRetriever;
        }
        [HttpGet]
        public async Task<IActionResult> GetLedger(GetLedgerRequest request)
        {
            var response = await _ledgerRetriever.GetLedgerAsync(request);
            return Ok(response);
        }
    }

    public class GetLedgerRequest
    {
        public BigInteger TillEvent { get; set; }
    }

    public class GetLedgerResponse
    {
        public required List<LedgerAccountDto> Accounts { get; set; }
    }

    public class LedgerAccountDto
    {
        public required string AccountName { get; set; }
        public int Value { get; set; }
    }
}
