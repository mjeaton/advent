namespace CreditCardPayoff.Main;

public class CreditCardPayoffCalculator
{
    public double GetMonthsUntilPayoff(double balance, double payment, double apr)
    {
        // the formula, flattened for easier understanding
        // n = -(1/30) * log(1 + b/p * (1 - (1 + i)^30)) / log(1 + i)
        var dailyRate = apr / 365 / 100;
       
        return Math.Ceiling(-(1.0 / 30.0) * Math.Log(1 + balance / payment * (1 - Math.Pow(1 + dailyRate, 30))) / Math.Log(1 + dailyRate));
    } 
}