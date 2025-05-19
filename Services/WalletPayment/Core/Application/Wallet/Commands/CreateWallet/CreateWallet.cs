using BuildingBlocks.CQRS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Wallet.Commands.CreateWallet;
public record CreateWalletCommand(Guid UserId) : ICommand<CreateWalletResponse>;
public record CreateWalletResponse(Guid WalletId, Guid DefaultAccountId);
