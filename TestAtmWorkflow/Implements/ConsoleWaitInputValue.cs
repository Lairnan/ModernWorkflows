using System.Globalization;
using ModernWorkflows.Interfaces;

namespace TestAtmWorkflow.Implements;

public class ConsoleWaitInputValue : IWaitInputValue
{
    public string WaitStringInput(string titleKey)
    {
        string? input;
        do
        {
            Console.Write(titleKey);
            input = Console.ReadLine();
        } while (string.IsNullOrWhiteSpace(input));
        return input;
    }

    public int WaitIntInput(string titleKey)
    {
        do
        {
            Console.Write(titleKey);
            var input = Console.ReadLine();
            if (!int.TryParse(input, out var i))
                Console.WriteLine("Wrong input. Wait integer...");
            else return i;
        } while (true);
    }

    public double WaitDoubleInput(string titleKey)
    {
        do
        {
            Console.Write(titleKey);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            input = input.Replace(',', '.');
            if (!double.TryParse(input, out var i))
                Console.WriteLine("Wrong input. Wait double(x.xx)");
            else return i;
        } while (true);
    }

    public decimal WaitDecimalInput(string titleKey, int decimalPoint)
    {
        do
        {
            Console.Write(titleKey);
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            input = input.Replace(',', '.');
            var indexPoint = input.IndexOf('.');

            input = input[..(indexPoint + decimalPoint + 1)];
            if (!decimal.TryParse(input, out var i))
                Console.WriteLine($"Wrong input. Wait decimal(x.{new string('x', decimalPoint)})");
            else return i;
        } while (true);
    }

    public DateTime WaitDateTimeInput(string titleKey, string dateFormat = "dd.MM.yyyy")
    {
        do
        {
            Console.Write(titleKey);
            var input = Console.ReadLine();
            if (!DateTime.TryParseExact(input, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var i))
                Console.WriteLine("Wrong input. Wait dateTime...");
            else return i;
        } while (true);
    }
}