using System;

namespace Maul
{
    public class CommandLineParser
    {
        public static CommandCall Parse(string[] args)
        {
            // the first arg is always the hive/command pair
            // subsequent args are always -flag, /flag, -switch=value, or /switch=value
            // args that don't start with - or / are assumed to be unadorned batch values - file names, for example

            var result = new CommandCall();

            foreach(var arg in args)
            {
                if (arg.StartsWith("-") || arg.StartsWith("/"))
                {
                    // is a switch
                    var switchArg = ParseSwitch(arg);
                    if (switchArg.IsMaulSwitch())
                    {
                        result.MaulArguments.Add(switchArg);
                    }
                    else
                    {
                        // TODO: don't allow command arguments until after we have a command
                        result.CommandArguments.Add(switchArg);
                    }
                }
                else
                {
                    if (result.GroupName == null)
                    {
                        var hiveCmd = arg;

                        var cmdParts = hiveCmd.Split(new [] {'.' }, 2);

                        result.GroupName = cmdParts[0];
                        if (cmdParts.Length == 2)
                        {
                            result.Name = cmdParts[1];
                        }
                    }
                    else
                    {
                        // unnamed target parameter
                        result.TargetParameters.Add(new TargetArgument(arg));
                    }
                }
            }

            return result;
        }

        public static NamedArgument ParseSwitch(string argText)
        {
            var result = new NamedArgument();

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
