using System;
using System.IO;
using log4net;

namespace Bastion.Client.Localization;

public sealed class LanguagePreferenceStore : ILanguagePreferenceStore
{
    private const string SettingsDirectoryName = "Bastion";
    private const string FileName = "language.txt";

    private static readonly ILog _logger = LogManager.GetLogger(typeof(LanguagePreferenceStore));

    private readonly string _directoryPath;
    private readonly string _filePath;

    public LanguagePreferenceStore()
    {
        _directoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            SettingsDirectoryName);
        _filePath = Path.Combine(_directoryPath, FileName);
    }

    public string LoadCultureName()
    {
        if (!File.Exists(_filePath))
        {
            return string.Empty;
        }

        string savedText = File.ReadAllText(_filePath);
        return savedText.Trim();
    }

    public void SaveCultureName(string cultureName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);

        try
        {
            Directory.CreateDirectory(_directoryPath);
            File.WriteAllText(_filePath, cultureName);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogSaveFailure(cultureName, ex);
        }
        catch (IOException ex)
        {
            LogSaveFailure(cultureName, ex);
        }
    }

    private void LogSaveFailure(string cultureName, Exception cause)
    {
        _logger.Error($"The chosen language could not be saved. Path={_filePath}; Language={cultureName}", cause);
    }
}
