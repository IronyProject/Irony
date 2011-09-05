#region License
/* **********************************************************************************
 * Copyright (c) Roman Ivantsov
 * This source code is subject to terms and conditions of the MIT License
 * for Irony. A copy of the license can be found in the License.txt file
 * at the root of this distribution. 
 * By using this source code in any fashion, you are agreeing to be bound by the terms of the 
 * MIT License.
 * You must not remove this notice from this software.
 * **********************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Irony.Interpreter.Evaluator;

namespace Irony.Samples {
  //The purpose of this class is pure convenience - to make expression evaluator grammar (which is in Irony.Interpreter assembly) to appear 
  // with other sample grammars. 
  public class SampleExpressionEvaluatorGrammar : ExpressionEvaluatorGrammar {
    public SampleExpressionEvaluatorGrammar() { 
    }

    public override Interpreter.LanguageRuntime CreateRuntime(Parsing.LanguageData language) {
      var runtime = base.CreateRuntime(language);
      return runtime; 
    }

  }//class

}//namespace


