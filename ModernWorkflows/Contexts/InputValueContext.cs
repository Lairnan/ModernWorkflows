using ModernWorkflows.Models;

namespace ModernWorkflows.Contexts;

public class InputValueContext
{
    public string TitleKey { get; set; }
    public int DecimalPoint { get; set; }
    public string InputFormat { get; set; }
    public InputValue InputValueType { get; set; } = InputValue.String;
    public object? Value { get; set; }
    public string? MessageError { get; set; }
}