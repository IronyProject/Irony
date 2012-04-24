using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Irony.Parsing {
  // Should be implemented by Grammar class to be able to run samples in Grammar Explorer.
  public interface ICanRunSample {
    string RunSample(RunSampleArgs args);
  }

  public class RunSampleArgs {
    public LanguageData Language;
    public string Sample;
    public ParseTree ParsedSample;
    public IConsoleAdaptor Console;
    public RunSampleArgs(LanguageData language, string sample, ParseTree parsedSample, IConsoleAdaptor console = null) {
      Language = language;
      Sample = sample;
      ParsedSample = parsedSample;
      Console = console;
    }
  }

  //An abstraction of a Console.
  public interface IConsoleAdaptor {
    bool Canceled { get; set; }
    void Write(string text);
    void WriteLine(string text);
    void SetTextStyle(ConsoleTextStyle style);
    int Read(); //reads a key
    string ReadLine(); //reads a line; returns null if Ctrl-C is pressed
    void SetTitle(string title);
  }

  public enum ConsoleTextStyle {
    Normal,
    Error,
  }
}
