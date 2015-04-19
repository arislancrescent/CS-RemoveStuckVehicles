using System;

namespace RemoveStuckTrains
{
    public sealed class Settings
    {
        private Settings()
        {
            Tag = "[ARIS] Remove Stuck Trains";
        }

        private static readonly Settings _Instance = new Settings();
        public static Settings Instance { get { return _Instance; } }

        public readonly string Tag;
    }
}