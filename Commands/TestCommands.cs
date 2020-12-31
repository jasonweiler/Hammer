using System;
using Hammer.Attributes;

namespace Hammer.Commands
{
    [HammerCommandGroup(Description = "Sample test commands")]
    public class TestCommands
    {
        [HammerCommand(Description = "Prints \"Hello World\" to the console")]
        public static void HelloWorld()
        {
            Console.Out.WriteLine("Hello World!");
        }
    }

    [HammerCommandGroup(Description = "Sermple commands", AltName = "Sermple")]
    public class TestCommands2
    {
        [HammerCommand(Description = "Prints \"Fare thee well\" to the console", AltName = "Sayounara")]
        public static void Derp()
        {
            Console.Out.WriteLine("Hasta la Vista, Baby-san!");
        }

        [HammerCommand(Description = "Prints what you tell it", AltName = "Echo")]
        static void DerpDerp(
            [HammerParameter(AltName="Text", Description = "Text to echo back to you")]
            string FooBar)
        {
            Console.Out.WriteLine(FooBar);
        }
    }
}
