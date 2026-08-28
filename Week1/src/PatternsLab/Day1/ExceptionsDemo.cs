namespace PatternsLab.Day1;

/// <summary>
/// Task 1.1 - Custom exception carrying extra context (DeficitAmount),
/// used inside a withdrawal scenario with try/catch/finally, plus a
/// demonstration of correct catch-block ordering (specific -> general).
/// </summary>
public sealed class InsufficientFundsException : Exception
{
    public decimal DeficitAmount { get; }

    public InsufficientFundsException(decimal deficitAmount)
        : base($"Insufficient funds. You are short by {deficitAmount:C}.")
    {
        DeficitAmount = deficitAmount;
    }

    public InsufficientFundsException(decimal deficitAmount, string message, Exception inner)
        : base(message, inner)
    {
        DeficitAmount = deficitAmount;
    }
}

public class BankAccount
{
    public string Owner { get; }
    public decimal Balance { get; private set; }

    public BankAccount(string owner, decimal openingBalance)
    {
        Owner = owner;
        Balance = openingBalance;
    }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance)
        {
            decimal deficit = amount - Balance;
            throw new InsufficientFundsException(deficit);
        }

        Balance -= amount;
    }
}

public static class ExceptionsDemo
{
    /// <summary>
    /// Withdrawal scenario: try/catch/finally where finally always logs the attempt,
    /// regardless of whether the withdrawal succeeded.
    /// </summary>
    public static void RunWithdrawalScenario()
    {
        var account = new BankAccount("Asha Rao", openingBalance: 500m);

        Console.WriteLine("--- Task 1.1: Withdrawal scenario ---");
        TryWithdraw(account, 200m); // succeeds
        TryWithdraw(account, 1000m); // fails -> InsufficientFundsException
    }

    private static void TryWithdraw(BankAccount account, decimal amount)
    {
        try
        {
            account.Withdraw(amount);
            Console.WriteLine($"Withdrew {amount:C}. New balance: {account.Balance:C}");
        }
        catch (InsufficientFundsException ex)
        {
            Console.WriteLine($"Withdrawal failed: {ex.Message} (deficit = {ex.DeficitAmount:C})");
        }
        finally
        {
            // Always runs: succeeded, failed, or even if an unexpected exception
            // propagates past the catch above.
            Console.WriteLine($"[log] Withdrawal attempt of {amount:C} for {account.Owner} recorded.");
        }
    }

    /// <summary>
    /// Correct catch ordering: most specific exceptions first, general Exception last.
    /// If you swap the order and put `catch (Exception)` first, the compiler reports
    /// CS0160 "A previous catch clause already catches all exceptions of this or a
    /// super type" for FormatException/OverflowException, because they can never be
    /// reached — that's the "reordering causes a compile error" check for this task.
    /// </summary>
    public static void ParseAndCompute(string input)
    {
        try
        {
            int value = int.Parse(input);           // may throw FormatException
            checked
            {
                int result = value * int.MaxValue;   // forces OverflowException
                Console.WriteLine($"Result: {result}");
            }
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"Format problem: '{input}' is not a valid integer. ({ex.Message})");
        }
        catch (OverflowException ex)
        {
            Console.WriteLine($"Overflow problem: the computation exceeded Int32 range. ({ex.Message})");
        }
        catch (Exception ex)
        {
            // General catch-all must come last; anything unexpected lands here.
            Console.WriteLine($"Unexpected error: {ex.GetType().Name} - {ex.Message}");
        }
    }

    // NOTE (compile-error proof, kept as a comment rather than real code so the
    // project still builds):
    //
    // try { ... }
    // catch (Exception ex) { ... }
    // catch (FormatException ex) { ... }   // CS0160: unreachable, already caught above
    // catch (OverflowException ex) { ... } // CS0160: unreachable, already caught above
}
