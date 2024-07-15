using System;

namespace XcelerateGames.EditorTools
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AllowNestingAttribute : DrawerAttribute
    {
    }
}
