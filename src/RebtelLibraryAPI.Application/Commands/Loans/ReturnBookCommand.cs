using MediatR;
using RebtelLibraryAPI.Application.DTOs;

namespace RebtelLibraryAPI.Application.Commands.Loans;

public record ReturnBookCommand(
    Guid BookId,
    Guid BorrowerId
) : IRequest<LoanDto>;