using RebtelLibraryAPI.Domain.Entities;

namespace RebtelLibraryAPI.Domain.Specifications;

public class LoanMustBeActive : Specification<Loan>
{
    public override string ErrorMessage => "Loan must be active";

    public override bool IsSatisfiedBy(Loan loan)
    {
        return loan.Status == LoanStatus.Active;
    }
}

public class LoanMustNotBeOverdue : Specification<Loan>
{
    public override string ErrorMessage => "Loan must not be overdue";

    public override bool IsSatisfiedBy(Loan loan)
    {
        return !loan.IsOverdue();
    }
}

public class LoanMustHaveValidDuration : Specification<Loan>
{
    private readonly int _maxDays;

    public LoanMustHaveValidDuration(int maxDays = 28)
    {
        _maxDays = maxDays;
    }

    public override string ErrorMessage => $"Loan duration must be between 1 and {_maxDays} days";

    public override bool IsSatisfiedBy(Loan loan)
    {
        var duration = loan.DueDate - loan.BorrowDate;
        return duration.TotalDays > 0 && duration.TotalDays <= _maxDays;
    }
}

public class LoanMustBeWithinStandardPeriod : Specification<Loan>
{
    private const int StandardLoanPeriod = 14;
    private const int MaxVariation = 14;

    public override string ErrorMessage =>
        $"Loan period must be within {StandardLoanPeriod - MaxVariation} to {StandardLoanPeriod + MaxVariation} days";

    public override bool IsSatisfiedBy(Loan loan)
    {
        var duration = loan.DueDate - loan.BorrowDate;
        var days = (int)duration.TotalDays;

        return Math.Abs(days - StandardLoanPeriod) <= MaxVariation;
    }
}