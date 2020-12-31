using System;

namespace Hammer
{
    public class CommandLineParser
    {
        public static CommandCall Parse(string[] args)
        {
            // the first arg is always the hive/command pair
            // subsequent args are always -flag, /flag, -switch=value, or /switch=value
            // args that don't start with - or / are assumed to be unadorned batch values - file names, for example

            var result = new CommandCall();

            if (args.Length > 0)
            {
                var hiveCmd = args[0];

                var cmdParts = hiveCmd.Split(new [] {'.' }, 2);

                result.GroupName = cmdParts[0];
                if (cmdParts.Length == 2)
                {
                    result.Name = cmdParts[1];
                }

                for (int i = 1; i < args.Length; ++i)
                {
                    if (args[i].StartsWith("-") || args[i].StartsWith("/"))
                    {
                        // is switch
                        var switchArg = ParseSwitch(args[i]);
                        if (switchArg.IsHammerSwitch())
                        {
                            result.HammerArguments.Add(switchArg);
                        }
                        else
                        {
                            result.CommandArguments.Add(switchArg);
                        }
                    }
                    else
                    {
                        result.TargetParameters.Add(args[i]);
                    }
                }
            }

            return result;
        }

        public static Argument ParseSwitch(string argText)
        {
            var result = new Argument();

            // 1. chop off the switch char
            argText = argText.Substring(1);

            // 2. split on any equals
            var argPair = argText.Split(new []{'='}, 2, StringSplitOptions.None);

            result.Name = argPair[0].Trim();
            if (argPair.Length == 2)
            {
                result.Value = argPair[1].Trim();
                result.HasValue = true;
            }

            return result;
        }
    }
}
