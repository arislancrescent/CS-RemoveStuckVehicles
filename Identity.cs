using ICities;

namespace RemoveStuckVehicles
{
    public class Identity : IUserMod
    {
        public string Name
        {
            get { return Settings.Instance.Tag; }
        }

        public string Description
        {
            get { return "Detects and removes all vehicles that are confused or blocked."; }
        }
    }
}