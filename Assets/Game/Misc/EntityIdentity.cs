namespace USquad
{
    public enum EIdentity
    {
        
    }

    public static class EntityIdentity
    {
        public static int Get(EIdentity identity)
        {
            return (int)identity;
        }
    }

}