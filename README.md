# Exodus

**Exodus** allows developers interested in moving from Unity to other game engines
to automate some parts of their transition.

Exodus aims to be a focal point for all aspects of this transition. Initally we are
focusing on source code translation. However next steps include asset and scene management.

** Source Code transformation **

Exodus uses Roslyn to read the unity solution and applies transformations
to the source code to help ease the transition to the new game engine.

If hand crafting a converter, Roslyn's syntax tree and Semantic model are available to
help determine context.

A simple text "find and replace" can also be used.


There is also an option to make web service calls to ChatGPT to automate the process. 

Rate and Token limits are likely to apply to this.

To use, modify the Program.

await SolutionConverter.ConvertSolutionAsync(solution path, destination path , Conversion target);

The software 

* reads the solution
* determines which projects are unity projects
* gathers all of the CS files in those projects
* applies a conversion to those files using the conversion target object.


Currently there are two conversion types

StrideConversionTarget
ChatGPTConversionTarget

The ChatGPTConversionTarget takes the name of the game engine you'd like to convert to as a parameter.

If Using ChatGPT, you'll need to supply your OpenAI API Key.





