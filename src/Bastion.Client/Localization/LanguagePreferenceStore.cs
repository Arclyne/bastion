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

    public string? Load()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            return File.ReadAllText(_filePath).Trim();
        }
        catch (UnauthorizedAccessException ex)
        {
            LogReadFailure(ex);
            return null;
        }
        catch (IOException ex)
        {
            LogReadFailure(ex);
            return null;
        }
    }

    public void Save(string cultureName)
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

    private void LogReadFailure(Exception ex)
    {
        _logger.Warn($"The saved language could not be read. Path={_filePath}", ex);
    }

    private void LogSaveFailure(string cultureName, Exception ex)
    {
        _logger.Warn($"The chosen language could not be saved. Path={_filePath}; Language={cultureName}", ex);
    }
}
