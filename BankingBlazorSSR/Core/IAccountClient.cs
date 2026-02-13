using BankingBlazorSsr.Core.Dto;
namespace BankingBlazorSsr.Core;

public interface IAccountClient {
   Task<Result<IEnumerable<AccountDto>?>> GetAll();
   Task<Result<IEnumerable<AccountDto>?>> GetAllByOwner(Guid ownerId);
   Task<Result<AccountDto?>> GetById(Guid accountId);
   Task<Result<AccountDto?>> GetByIban(string iban);
   Task<Result<AccountDto?>> Post(Guid ownerId, AccountDto accountDto);
}