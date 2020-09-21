namespace NBattleshipCodingContest.Players
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class IgnoreAttribute : Attribute
    {
    }
}
