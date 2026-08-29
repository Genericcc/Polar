namespace _Scripts
{
    /// <summary>
    /// Shared placement for the project's own entries in the Project window's Create menu.
    /// Every [CreateAssetMenu] under the "Polar" root should use <see cref="Order"/> - a submenu
    /// is positioned by its items' priorities, so mixed values scatter the group.
    /// </summary>
    public static class PolarAssetMenu
    {
        public const string Root = "Polar/";

        // Lower sorts earlier; negative beats Unity's built-in entries. Raise towards 0 to sink
        // the group further down the menu.
        public const int Order = -1000;
    }
}
