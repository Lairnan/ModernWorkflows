using System.Text;
using ModernWorkflows.Interfaces;

namespace TestAtmWorkflow.Implements;

public class ConsolePresenter : IMessagePresenter
{
    public void Show(string message, bool newLineAfter = false)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        var messageBuilder = new StringBuilder();
        messageBuilder.Append(message);
        if (newLineAfter)
            messageBuilder.AppendLine();
        Console.WriteLine(messageBuilder.ToString());
        Console.ForegroundColor = color;
    }
}