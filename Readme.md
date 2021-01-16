# Hammer
A generic command line tool to use as an extension point for small productivity tools

## Usage
* `Hammer.exe` - List all commands and command groups
* `Hammer.exe`  `group` - *List all commands within the group*
* `Hammer.exe`  `group.command` [**parameters**]- Call this command with zero or more parameters

### Parameters
All command parameters fall into three distinct forms. 
1. Named switches: `-switch` or `/switch` which is akin to a boolean argument
1. Named arguments: `-name=value` or `/name=value` passes the value 
1. Targets: **value**

Switches and arguments are mapped onto function arguments of the same name while targets are collected into a single array of values. Command line order is not maintained, so positional arguments should use name-value switches.

### Background
I had a general tool like this at a previous game studio job, so I decided to write my own on a lark. Basically, Hammer is a small command line tool framework with a git-like usage pattern. It has some nice upsides in a production environment for one-off tools like single-use data

*For developers*, it provides some command line handling and a way to document commands. Adding new command-groups and command entry points is easy because most of the drudgery is taken care of.

*For users*, it lowered the cognitive barrier to running fix-it tool. 
It lowered the barrier for creating simple command line tools by putting them all in one place and providing a means for users to 


+ Low overhead to extend for developers
+ Putting many tools in one place lowers the barrier to use the tool - even for non-technical people
+ The help system means commands are easily discoverable by users



Implementation:
A command group is defined by a class with the CommandGroupAttribute
<pre>[CommandGroup]
public class HelloCommands {}
</pre>

This defines a new command group called `Hello`. There are two optional fields on the attribute:
* `Description` - A string used by the help system that describes that this group's functionality.
* `AltName` - An alternate command group name to be used instead the class name.

<pre>[Command(Description = "Prints a greeting to the console")]
static void World(
    [Parameter(Description = "A name to print"), Optional=true]
    string name="World")
{
    Console.Out.WriteLine($"Hello, {Name}");
}
</pre>

Defines a new entry point in the `Hello` command group called `World` that takes one optional argument.

The Command and Parameter attributes have the same optional fields as the CommandGroup.

The Parameter attribute has two additional optional fields:
* `Optional` - Marks the parameter as optional. If it isn't specified on the command line, the default argument is used. If there is no defined default argument, the type-default is used.
* `Default` - Defines the parameter default value

