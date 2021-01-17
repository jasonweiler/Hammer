# Hammer
A generic command line tool to use as an extension point for small productivity utilities

### Usage
* `Hammer.exe` - List all commands and command groups

* `Hammer.exe`  `group` - List all commands within the group

* `Hammer.exe`  `group.command` [**-named-parameters**] [**targets**]- Call this command with zero or more parameters

### Parameters
All command parameters fall into three distinct forms. 
1. `-switch` or `/switch` <br/>Named switch arguments are akin to a boolean values.

1. `-name=value` or `/name=value` <br/>Named arguments are always a name-value pair separated by an equals.

1. `target` <br/>Unnamed arguments after the comand name are considered command-targets.

Switches and arguments are mapped onto function arguments of the same name while targets are collected into a single array of values. Command line order is not maintained, so positional arguments should use name-value switches.

### But why?!
This tool is a reimplementation of one I had at a previous game studio job. If you've worked in the games industry - or perhaps any technical, content-heavy job, there are many situations where you don't need a new graphical tool. You just need a targeted command line that will fix your problem. That carries some baggage with it, however. First, discoverablility is usually a problem. There often isn't a good way to find that a command line tool exists at all, so users are unlikely to find them. Second, not all users are so comfortable with command line tools, so adoption is always a problem. Third, writing good command line tools that are easy to use and difficult to misuse is a lot harder than it might appear. A lot of time is spent on argument edge-cases and base-line functionality.

Hammer tries to address all of these issues. For users, it creates a single environment with multiple entry points so they know where to start. There are facilities to discover functionaltiy and get basic help. At a basic level, commands always follow the same calling convention to help with familiarity.

For programmers, Hammer provides a single extensible project where argument plumbing is a simple matter of some attribute markup and doing so automatically documents the functionality. Writing one-off fixes or lesser-used tools becomes trivial.

### Implementation
Commands are separated into command-groups. These groupings are arbitrary, but they're intended to represent some larger body of functionality. For example, Content fixes, Database-access, Cache maintenance. Adding a command group is as simple as adding a new class like this:

A command group is defined by a class with the CommandGroupAttribute
<pre>
[CommandGroup]
public class HelloCommands {}
</pre>

This defines a new command group called `Hello` based on the class name. The CommandGroup attribute has two optional fields:
* `Description` - A string used by the help system that describes this group's intended functionality.
* `AltName` - An alternate command group name to be used instead the one derived from the class name.

Next we can add a command to `Hello` like so:
<pre>
<b>[Command(Description="Prints a greeting to the console")]</b>
static void World(
    <b>[NamedParameter(Description="Optional name", Optional=true)]</b>
    string name="World")
{
    Console.Out.WriteLine($"Hello, {name}!");
}
</pre>

This will define a new entry point called `World` that takes one optional argument called `name`. Calling this from the command line looks like this:
<pre>
> Hammer.exe Hello.World /name=Earthlings
<b>Hello, Earthlings!</b>
</pre>

The `[Command]` attribute has the same optional fields as `[CommandGroup]` that allow for renaming and documentation.

### Parameters

All parameters to the entry point function must be mapped from the command line. Just as with commands, we use some custom attributes to help us. All parameters fall into two general categories: named and targets.

#### Named Parameters
A named parameter uses the `[NamedParameter]` attribute and maps command line arguments that use the `-` or `/` switch syntax to function arguments. An attempt will be made to convert the user-provided string to a strongly typed value with the C# type system, but the implementor is welcome to do their own conversion if they just accept the argument as a string. As one might expect, the switch-name should match the C# function parameter. As with other attributes, the `AltName` attribute field is available to rename arguments for the user.

The `[NamedParameter]` attribute has them as well in addition to some optional fields for default parameters.

* `Description` - A string used by the help system that describes that parameter's purpose.
* `AltName` - An alternate parameter name to be used instead the one defined in code.
* `Optional` - Marks the parameter as optional. If it isn't specified on the command line, the default argument is used. If there is no defined default argument, the type-default is used.
* `Default` - Defines the parameter's default value

Technically, the `[NamedParameter]` attribute is entirely optional. Any function parameter without some kind of parameter attribute will be assumed to be a named parameter where the name matches the code. If there is a code-defined default value, then the parameter is assumed to be optional.

#### Target Parameters

A target parameter is any parameter that doesn't use the `-` or `/` switch syntax. These are unnamed values that should be acted upon by the command - multiple file names to be processed, for example. Ultimately, however, it's up to the programmer how they're used. In the code, all of these values are mapped onto a single parameter with the `[TargetParameter]` attribute. Order is not guaranteed, so if that's important, you should use named parameters.

As with the others, `[TargetParamter]` has a few optional attribute fields:
* `Description` - A string used by the help system that describes this parameter's purpose.
* `MinCount` - Defines a minimum number of required targets (default: 0)
* `MaxCount` - Defines a maximum number of allowed targets (default: unlimited)
