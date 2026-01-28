public enum GameMode
{
    MainMenu,
    Local,
    Bot,
    Online
}

public static class GameModeConfig
{
    public static GameMode Mode = GameMode.Local;
}

