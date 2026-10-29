namespace Boovatech.Web.Controllers
{
    public interface ILedgerRetriver
    {
        Task<GetLedgerResponse> GetLedgerAsync(GetLedgerRequest request);
    }

    public class LedgerRetriver : ILedgerRetriver
    {
        public Task<GetLedgerResponse> GetLedgerAsync(GetLedgerRequest request)
        {
            return Task.FromResult(new GetLedgerResponse
            {
                Accounts = new List<LedgerAccountDto>
                {
                    new LedgerAccountDto { AccountName = "Bank", Value=5000},
                    new LedgerAccountDto { AccountName = "Vault1", Value=5000 },
                    new LedgerAccountDto { AccountName = "Vault2", Value=5000},
                }
            });
        }
    }
}