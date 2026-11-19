using System;
using Bastion.Domain.Matches;
using Microsoft.Xna.Framework;

namespace Bastion.Client.Theme;

public static class Palette
{
    private const byte FocusHaloAlpha = 0x29;
    private const byte ErrorHaloAlpha = 0x1A;
    private const byte ReachableSquareAlpha = 0xC8;
    private const byte HeroIconFillAlpha = 0x29;

    private static readonly Color _pawnBlue = new Color(0x2F, 0x6F, 0xB8);
    private static readonly Color _pawnRed = new Color(0xC2, 0x3B, 0x2E);
    private static readonly Color _pawnGreen = new Color(0x2E, 0x8B, 0x57);
    private static readonly Color _pawnYellow = new Color(0xD9, 0xA4, 0x1C);

    public static Color Background { get; } = new Color(0xF6, 0xF1, 0xE6);

    public static Color Card { get; } = new Color(0xFF, 0xFD, 0xF8);

    public static Color White { get; } = new Color(0xFF, 0xFF, 0xFF);

    public static Color Ink { get; } = new Color(0x2A, 0x1F, 0x1A);

    public static Color Body { get; } = new Color(0x5A, 0x4A, 0x40);

    public static Color Muted { get; } = new Color(0x8A, 0x7A, 0x6E);

    public static Color Placeholder { get; } = new Color(0xA8, 0x99, 0x8C);

    public static Color Accent { get; } = new Color(0x9C, 0x4F, 0x3E);

    public static Color AccentLight { get; } = new Color(0xB3, 0x5A, 0x45);

    public static Color AccentDark { get; } = new Color(0x73, 0x36, 0x28);

    public static Color AccentSoft { get; } = new Color(0xF0, 0xC4, 0x88);

    public static Color Border { get; } = new Color(0xE6, 0xDA, 0xCA);

    public static Color BorderStrong { get; } = new Color(0xD8, 0xC8, 0xB3);

    public static Color ButtonEdge { get; } = new Color(0xE2, 0xD4, 0xC1);

    public static Color Error { get; } = new Color(0xB3, 0x26, 0x1E);

    public static Color Shadow { get; } = new Color(0x3C, 0x23, 0x14, 0x48);

    public static Color BorderHover { get; } = new Color(0xC9, 0xB7, 0x9F);

    public static Color AccentHover { get; } = new Color(0xBF, 0x65, 0x4F);

    public static Color AccentHoverEnd { get; } = new Color(0xA8, 0x56, 0x43);

    public static Color SurfaceHover { get; } = new Color(0xFB, 0xF7, 0xF0);

    public static Color Selected { get; } = new Color(0xF6, 0xEE, 0xE4);

    public static Color Disabled { get; } = new Color(0xF4, 0xEE, 0xE5);

    public static Color FocusHalo { get; } = new Color(Accent, FocusHaloAlpha);

    public static Color ErrorHalo { get; } = new Color(Error, ErrorHaloAlpha);

    public static Color ErrorSoft { get; } = new Color(0xFD, 0xEC, 0xEA);

    public static Color Warning { get; } = new Color(0x8A, 0x5A, 0x00);

    public static Color WarningSoft { get; } = new Color(0xFF, 0xF4, 0xDC);

    public static Color InfoSoft { get; } = new Color(0xEA, 0xF2, 0xFA);

    public static Color Success { get; } = new Color(0x11, 0x70, 0x4F);

    public static Color SuccessSoft { get; } = new Color(0xE6, 0xF4, 0xEE);

    public static Color Overlay { get; } = new Color(0x1E, 0x12, 0x0A, 0x73);

    public static Color IdentityAccent { get; } = new Color(0xD8, 0x5A, 0x30);

    public static Color ReachableSquare { get; } = new Color(AccentSoft, ReachableSquareAlpha);

    public static Color Wall { get; } = new Color(0x8B, 0x5A, 0x3C);

    public static Color PendingWall { get; } = new Color(0xC9, 0x8B, 0x4E);

    public static Color ValidWallGhost { get; } = new Color(0x3F, 0xA3, 0x6B);

    public static Color InvalidWallGhost { get; } = new Color(0xC2, 0x3B, 0x2E);

    public static Color AvatarFill { get; } = new Color(0xFB, 0xEC, 0xDA);

    public static Color HeroIconFill { get; } = new Color(White, HeroIconFillAlpha);

    public static Color HeroDetail { get; } = new Color(0xFF, 0xF2, 0xEC);

    public static Color GetPawnColor(PawnSymbol symbol)
    {
        switch (symbol)
        {
            case PawnSymbol.Blue:
                return _pawnBlue;
            case PawnSymbol.Red:
                return _pawnRed;
            case PawnSymbol.Green:
                return _pawnGreen;
            case PawnSymbol.Yellow:
                return _pawnYellow;
            default:
                throw new ArgumentOutOfRangeException(nameof(symbol), symbol, "Unsupported pawn symbol.");
        }
    }
}
