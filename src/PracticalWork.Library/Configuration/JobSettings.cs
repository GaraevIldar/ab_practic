namespace PracticalWork.Library.Configuration;

/// <summary>
/// Настройки фоновых задач системы библиотеки
/// </summary>
public class JobSettings
{
    public Dictionary<string, JobConfiguration> Jobs { get; set; } = new();
}

/// <summary>
/// Конфигурация отдельной фоновой задачи
/// </summary>
public class JobConfiguration
{
    public string CronExpression { get; set; } = "0 0 6 * * ?";
    public int MaxRetries { get; set; } = 3;
    public int TimeoutMinutes { get; set; } = 30;
}
