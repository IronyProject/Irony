using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Irony.Interpreter;
using Irony.Samples;

namespace Irony.Samples.ConsoleCalculator {
  class Program {

    static void Main(string[] args) {
      var grammar = new ExpressionEvaluatorGrammar();
      var interpreter = new ScriptInterpreter(grammar);
      interpreter.RethrowExceptions = false; 
      
      Console.Title = "Calculator";
      Console.CancelKeyPress += Console_CancelKeyPress;
      Console.WriteLine("Command line calculator based on Irony.Samples.ExpressionEvaluatorGrammar.");
      Console.WriteLine("Type 'exit' or press Ctr^C to exit the program.");
      Console.WriteLine(); 
      
      string input;
      string prompt = ">";
      while(true) {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(prompt);
        interpreter.EvaluationContext.OutputBuffer.Length = 0;
        input = Console.ReadLine();
        if (input == "exit") return;
        var result = interpreter.Evaluate(input);
        switch (result) {
          case InterpreterState.Success:
            Console.WriteLine(interpreter.EvaluationContext.OutputBuffer);
            break; 
          case InterpreterState.SyntaxError:
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var err in interpreter.LastParseTree.Errors) {
              Console.WriteLine(string.Empty.PadRight(prompt.Length + err.Location.Column) + "^");
              Console.WriteLine(err.Message);
              //Console.WriteLine("(column " + (err.Location.Column + 1) + ")");
            }
            break; 
          case InterpreterState.RuntimeError:
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(interpreter.LastException.Message); 
            break;
          default: break;
        }//switch
      } 
    }

    static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e) {
      //e.Cancel = false;
    }
  }
}
