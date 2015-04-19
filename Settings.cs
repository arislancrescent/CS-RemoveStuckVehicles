using System;

namespace RemoveStuckVehicles
{
    public sealed class Settings
    {
        private Settings()
        {
            Tag = "[ARIS] Remove Stuck Vehicles";
        }

        private static readonly Settings _Instance = new Settings();
        public static Settings Instance { get { return _Instance; } }

        public readonly string Tag;
    }
}