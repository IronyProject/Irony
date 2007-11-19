ReadMe file for Irony project
http://www.codeplex.com/irony

Prerequisites
  Windows XP or Vista; Visual Studio 2005, .NET Framework 3.0
  
Demo instructions
1. Open Irony_All.sln solution file in Visual Studio.
2. Right-click on the project "030.Irony.GrammarExplorer" and select "Set as StartUp project" from context menu.
3. Click Run button on toolbar (F5). Grammar Explorer Window opens.
4. Select grammar/language in top combo-box.
5. Browse form tabs to see grammar data. 
6. To parse source code sample, switch to "Parsing Test" tab. Click "Load..." button on top of the form. Open file dialog opens.
7. Navigate to <root>\Irony.Samples\SourceSamples folder. Select source file appropriate for grammar selected in step 4. 
8. Source file contents are loaded into text area in the form. Click Parse button on top of the form. 
9. The Output Syntax Tree control on the right (in Results tab in the right tab control) would contain a parsed syntax tree.
10. Alternatively you can paste or type your own sample program into source text area. 
11. Repeat steps 4-10 for other languages.

Notes
1. Grammar Explorer restores your last language selection and source sample after you close/restart the form. 
2. Note that sample grammars we provide are just samples - don't expect them to parse successfully any correct code file 
   in selected language.

  