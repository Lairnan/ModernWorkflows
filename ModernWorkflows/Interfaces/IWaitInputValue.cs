namespace ModernWorkflows.Interfaces;

public interface IWaitInputValue
{
    string WaitStringInput(string titleKey);
    int WaitIntInput(string titleKey);
    double WaitDoubleInput(string titleKey);
    decimal WaitDecimalInput(string titleKey, int decimalPoint);
    DateTime WaitDateTimeInput(string titleKey, string dateFormat = "dd.MM.yyyy");
}