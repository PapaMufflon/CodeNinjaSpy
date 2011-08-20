// Guids.cs
// MUST match guids.h
using System;

namespace MufflonoSoft.CodeNinjaSpy
{
    static class GuidList
    {
        public const string guidCodeNinjaSpyPkgString = "8524ed92-ffa6-476e-9caa-b30c4d2f8d17";
        public const string guidCodeNinjaSpyCmdSetString = "4d6482a4-fe61-4538-a171-dec281ff5b30";
        public const string guidToolWindowPersistanceString = "62fc0bfd-d05f-44c1-8170-8c5a1f1a5b3e";

        public static readonly Guid guidCodeNinjaSpyCmdSet = new Guid(guidCodeNinjaSpyCmdSetString);
    };
}