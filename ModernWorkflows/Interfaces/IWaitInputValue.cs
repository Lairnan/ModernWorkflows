namespace ModernWorkflows.Interfaces;

/// <summary>
/// Defines a base contract for halting execution to wait for user or system input.
/// Consumers must manually implement this interface to capture data from their specific input sources (e.g., Console, UI dialogs, or web requests).
/// </summary>
public interface IWaitInputValue
{
    /// <summary>
    /// Prompts for and waits until a string value is provided.
    /// </summary>
    /// <param name="titleKey">The text or localization key used to prompt the user for this input.</param>
    /// <returns>The string value provided by the input source.</returns>
    string WaitStringInput(string titleKey);
    
    /// <summary>
    /// Prompts for and waits until a valid integer value is provided.
    /// </summary>
    /// <param name="titleKey">The text or localization key used to prompt the user for this input.</param>
    /// <returns>The parsed integer value.</returns>
    int WaitIntInput(string titleKey);
    
    /// <summary>
    /// Prompts for and waits until a valid double-precision floating-point value is provided.
    /// </summary>
    /// <param name="titleKey">The text or localization key used to prompt the user for this input.</param>
    /// <returns>The parsed double value.</returns>
    double WaitDoubleInput(string titleKey);
    
    /// <summary>
    /// Prompts for and waits until a valid decimal value is provided, enforcing a specific decimal precision.
    /// </summary>
    /// <param name="titleKey">The text or localization key used to prompt the user for this input.</param>
    /// <param name="decimalPoint">The maximum number of decimal places expected or allowed for the input.</param>
    /// <returns>The parsed decimal value.</returns>
    decimal WaitDecimalInput(string titleKey, int decimalPoint);
    
    /// <summary>
    /// Prompts for and waits until a valid date and time value is provided based on a specific format.
    /// </summary>
    /// <param name="titleKey">The text or localization key used to prompt the user for this input.</param>
    /// <param name="dateFormat">The expected string format of the date (default is "dd.MM.yyyy").</param>
    /// <returns>The parsed <see cref="DateTime"/> value.</returns>
    DateTime WaitDateTimeInput(string titleKey, string dateFormat = "dd.MM.yyyy");
}