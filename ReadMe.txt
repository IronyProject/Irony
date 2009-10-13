ReadMe file for Irony project
http://www.codeplex.com/irony

Prerequisites
  Windows XP or Vista; Visual Studio 2008, .NET Framework 3.5
  
Demo instructions
To run Grammar Explorer
* Open Irony_All.sln solution file in Visual Studio.
* Right-click on the project "030.Irony.GrammarExplorer" and select "Set as StartUp project" from context menu.
* Click Run button on toolbar (F5). Grammar Explorer Window opens.
* Important: if you are launching the Grammar Explorer for the first time after downloading Irony and 
    if you see that Grammars combobox is not empty, then make sure you clear: click on the button next to the combobox and select "Remove all"
* If Grammars combobox on top is empty, click on the button next to it (or right-click the combobox) and select 
   "Add grammar" command. In the file-open window that appears, navigate to (root)\Irony.Samples\bin\debug folder and 
   select Irony.Samples.dll. Application will popup a small window with a list of grammars in selected assembly. 
   Leave all lines checked and click "OK". The newly added grammars will appear in the grammar combobox.  
* Select grammar/language in top combo-box.
* Browse form tabs to see grammar data. 
* To parse source code sample, switch to "Test" tab. Click "Load..." button on top of the form. Open file dialog opens.
* Navigate to <root>\Irony.Samples\SourceSamples folder. Select source file appropriate for the selected grammar. 
* Source file contents are loaded in the text area in the form. Click Parse button to parse the sample. 
* The Parse Tree control on the right will show the parse tree for the sample.
* If the button "Run" enabled, click it to execute/evaluate the code. The results are shown in the Output window at the bottom of the form.
  For Expression Evaluator grammar, the output is the result of the last expression or assignment. Interpreter for Scheme
  can execute more complex programs found in SourceSamples\Scheme subfolder.  
* Alternatively you can paste or type your own sample program into source text area. 
* Repeat for other selections in the Grammar combobox.
Note that Grammar Explorer restores your last language selection and source sample after you close/restart the form. 

  