using ICities;

namespace RemoveStuckTrains
{
    public class Identity : IUserMod
    {
        public string Name
        {
            get { return Settings.Instance.Tag; }
        }

        public string Description
        {
            get { return "Detects and removes all trains that are stuck."; }
        }
    }
}