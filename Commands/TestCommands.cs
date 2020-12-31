using System;
using Hammer.Attributes;

namespace Hammer.Commands
{
    [CommandGroup(Description = "Sample test commands")]
    public class TestCommands
    {
        [Command(Description = "Prints \"Hello World\" to the console")]
        public static void HelloWorld()
        {
            Console.Out.WriteLine("Hello World!");
        }
    }

    [CommandGroup(Description = "Sermple commands", AltName = "Sermple")]
    public class TestCommands2
    {
        [Command(Description = "Prints \"Fare thee well\" to the console", AltName = "Sayounara")]
        public static void Derp()
        {
            Console.Out.WriteLine("Hasta la Vista, Baby-san!");
        }

        [Command(Description = "Prints what you tell it", AltName = "Echo")]
        static void DerpDerp(
            [Parameter(AltName="Text", Description = "Text to echo back to you")]
            string FooBar)
        {
            Console.Out.WriteLine(FooBar);
        }

        enum Severity
        {
            Info,
            Warning,
            Error,
            Critical,
            OhDearGod,
        }

        [Command(Description = "Prints what you tell it with a default", AltName = "EchoDefault")]
        static void TestDefaultArgs(
            [Parameter(AltName="Text", Description = "Text to echo back to you", Default = "This is default!")]
            string thingthingthing,
            [Parameter(Default = 42, Description = "Never you mind!")]
            int Value,
            [Parameter(Optional = true)]
            string optionalString,
            Severity severity = Severity.OhDearGod)
        {
            Console.Out.WriteLine(thingthingthing);
        }
    }
}
