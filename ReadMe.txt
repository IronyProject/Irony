ReadMe file for Irony project
http://www.codeplex.com/irony

Prerequisites
  Windows XP or Vista; Visual Studio 2005, .NET Framework 3.0
  
Demo instructions
To run Grammar Explorer
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
Note that Grammar Explorer restores your last language selection and source sample after you close/restart the form. 

To run unit tests
1. Prerequisite: you should have NUnit installed; Irony download contains core NUnit assembly in Irony.Tests\Lib folder, but 
it is only to allow the test project to compile. Goto www.nunit.org, download and install NUnit 
2. Open Irony_All.sln solution file in Visual Studio.
3. Right-click on the project "040.Irony.Tests" and select "Set as StartUp project" from context menu.
4. In properties dialog for this project, Debug page, in "Start Action" option group, select option "Start external program";
enter/browse the path to the nunit GUI - should be something like "C:\Program Files\NUnit 2.4.3\bin\nunit.exe"
5. Click Run (F5)
6. In NUnit GUI app select File/Open Project menu item, select Irony.Tests.dll in the bin subfolder of the project. 
7. Click Run buton in GUI to execute the unit tests.  


  