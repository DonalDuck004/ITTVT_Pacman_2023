namespace PacManWPF.Game.PGs.Enums
{
    public enum GhostColors
    {
        Red,
        Pink,
        Cyan,
        Orange
    }

    public enum GhostTickTypes
    {
        Died = 0,
        Scaried = 1,
        Alive = 2,
    }

    public enum FoodTypes
    {
        PacDot = 10,
        PowerPellet = 50,
        Cherry = 100,
        Strawberry = 300,
        Peach = 500,
        Apple = 700,
        Melon = 1000,
        Galaxian = 2000,
        Bell = 3000,
        Key = 5000,
    }

    public enum GhostEngines
    {
        CachedAutoMover,
        Cyclic,
        NextToBack,
        NoCachedAutoMover,
        OneTime,
        Fixed,

        _NULL
    }
}
