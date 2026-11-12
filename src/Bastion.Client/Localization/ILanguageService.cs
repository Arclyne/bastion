using System;
using System.Collections.Generic;

namespace Bastion.Client.Localization;

/// <summary>
/// Keeps the language of the game: the languages it offers, the one on screen, and the one the player chose.
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Gets the languages the game offers, in the order the language picker lists them.
    /// </summary>
    IReadOnlyList<SupportedLanguage> Languages { get; }

    /// <summary>
    /// Gets the language the game shows now.
    /// </summary>
    SupportedLanguage CurrentLanguage { get; }

    /// <summary>
    /// Occurs after the game switches language, once the texts on screen are translated.
    /// </summary>
    event EventHandler? LanguageChanged;

    /// <summary>
    /// Loads the translated texts and shows the language the player saved. Without a saved one, it shows the language
    /// closest to the system's, or English when none is close.
    /// </summary>
    void LoadInitialLanguage();

    /// <summary>
    /// Switches the game to a language and saves it as the player's choice.
    /// </summary>
    /// <param name="language">The language to show, one of <see cref="Languages"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="language"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="language"/> is not one of <see cref="Languages"/>.</exception>
    void ChangeLanguage(SupportedLanguage language);
}
