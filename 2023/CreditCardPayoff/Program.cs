using CreditCardPayoff.Main;
using Spectre.Console;

Console.WriteLine();
AnsiConsole.MarkupLine("[red]Credit[/] [green]Card[/] [red]Payoff[/] [green]Calculator![/]");
Console.WriteLine();

var annualPercentageRate = AnsiConsole.Prompt(
    new TextPrompt<double>("What is the APR on the card (as a percent)?")
        .PromptStyle("green")
        .ValidationErrorMessage("[red]That's not a valid rate[/]")
        .Validate(apr =>
            apr switch
            {
                < 0 => ValidationResult.Error("[red]The APR must be great than or equal to zero.[/]"),
                > 100 => ValidationResult.Error("[red]The APR cannot be greater than 100.[/]"),
                _ => ValidationResult.Success(),
            })
);

var cardBalance= AnsiConsole.Prompt(
    new TextPrompt<double>("What is the balance on the card?")
        .PromptStyle("green")
        .ValidationErrorMessage("[red]That's not a valid balance[/]")
        .Validate(balance =>
            balance switch
            {
                <= 0 => ValidationResult.Error("[red]The balance must be greater than zero.[/]"),
                _ => ValidationResult.Success(),
            })
);

var monthlyPayment= AnsiConsole.Prompt(
    new TextPrompt<double>("How much do you pay each month?")
        .PromptStyle("green")
        .ValidationErrorMessage("[red]That's not a valid amount.[/]")
        .Validate(payment =>
            payment switch
            {
                <= 0 => ValidationResult.Error("[red]The amount must be greater than zero.[/]"),
                _ => ValidationResult.Success(),
            })
);

var calc = new CreditCardPayoffCalculator();

var monthsToPayoff = calc.GetMonthsUntilPayoff(cardBalance, monthlyPayment, annualPercentageRate);
Console.WriteLine();
Console.WriteLine($"It will take you {monthsToPayoff} months to pay off {cardBalance:C} if you only pay {monthlyPayment:C} per month.");
Console.WriteLine();


