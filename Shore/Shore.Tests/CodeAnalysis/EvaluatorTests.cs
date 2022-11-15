using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;
using Xunit;

namespace Shore.Tests.CodeAnalysis
{
    public class EvaluatorTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("~1", -2)]
        [InlineData("(1)", 1)]
        [InlineData("13 + 14", 27)]
        [InlineData("96 - 3", 93)]
        [InlineData("9 * 10", 90)]
        [InlineData("18 / 9", 2)]
        [InlineData("(1 + 2) * 3", 9)]
        [InlineData("3 < 4", true)]
        [InlineData("5 < 4", false)]
        [InlineData("4 <= 4", true)]
        [InlineData("4 <= 5", true)]
        [InlineData("3 > 4", false)]
        [InlineData("5 > 4", true)]
        [InlineData("4 >= 4", true)]
        [InlineData("4 >= 5", false)]
        [InlineData("1 == 2", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 4", true)]
        [InlineData("13 != 13", false)]
        [InlineData("1 | 2", 3)]
        [InlineData("1 | 0", 1)]
        [InlineData("1 & 3", 1)]
        [InlineData("1 & 0", 0)]
        [InlineData("1 ^ 0", 1)]
        [InlineData("1 ^ 3", 2)]
        [InlineData("-5 >> 1", -3)]
        [InlineData("-2 << 1", -4)]
        [InlineData("false == true", false)]
        [InlineData("false == false", true)]
        [InlineData("true == true", true)]
        [InlineData("true != true", false)]
        [InlineData("true != false", true)]
        [InlineData("false != false", false)]
        [InlineData("false | false", false)]
        [InlineData("false | true", true)]
        [InlineData("true | true", true)]
        [InlineData("false & false", false)]
        [InlineData("false & true", false)]
        [InlineData("true & true", true)]
        [InlineData("false ^ false", false)]
        [InlineData("true ^ false", true)]
        [InlineData("true ^ true", false)]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("\"test\"", "test")]
        [InlineData("\"te\"\"st\"", "te\"st")]
        [InlineData("\"test\" == \"test\"", true)]
        [InlineData("\"test\" != \"test\"", false)]
        [InlineData("\"test\" == \"abc\"", false)]
        [InlineData("\"test\" != \"abc\"", true)]
        [InlineData("int a = 10", 10)]
        [InlineData("{ int b = 0 (b = 10) * b}", 100)]
        [InlineData("{ int a = 0 if a == 0 a = 10 a }", 10)]
        [InlineData("{ int a = 0 if a == 4 a = 10 a }", 0)]
        [InlineData("{ int a = 0 if a == 0 a = 10 else a = 5 a }", 10)]
        [InlineData("{ int a = 0 if a == 4 a = 10 else a = 5 a }", 5)]
        [InlineData("{ int i = 10 int result = 0 while i > 0 { result = result + i i = i -1 } result }", 55)]
        [InlineData("{ int result = 0 for i = 1 until 10 { result = result + i } result }", 55)]
        [InlineData("{ int result = 0 for i = 1 until 10 { if i == 5 break result = result + i } result }", 10)]
        [InlineData("{ int result = 0 for i = 1 until 10 { if result == 1 { result = 2 continue } result = result + i } result }", 54)]
        [InlineData("{ int a = 10 for i = 1 until (a = a - 1) { } a }", 9)]
        [InlineData("function void tester(){ 1 } tester()", 1)]
        [InlineData("function void tester(int num, int numTwo){ num + numTwo } tester(1,1)", 2)]
        [InlineData("function string tester(){ return \"hello\" } tester()", "hello")]
        [InlineData("function string tester(string str){ return str } tester(\"hi\")", "hi")]
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
        {
            AssertValue(text, expectedValue);
        }

        [Fact]
        public void Evaluator_Reports_Void_Return()
        {
            var text = @"
                function void tester() {return [1]}
            ";

            var diagnostics = @"
                Since the Function 'tester' is of Type 'void' 'return' cannot be followed by an expression.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_Reports_Function_TypeMismatch()
        {
            var text = @"
                function string tester() {return [1]}
            ";

            var diagnostics = @"
                Cannot convert Type 'int32' to 'string'. An explicit conversion exists (are you missing a cast?).
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_Reports_Function_Argument_TypeMismatch()
        {
            var text = @"
                function string tester(int arg) {return [arg]}
            ";

            var diagnostics = @"
                Cannot convert Type 'int32' to 'string'. An explicit conversion exists (are you missing a cast?).
            ";
            
            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_InvokeFunctionArguments_NoInfiniteLoop()
        {
            var text = @"
                print(""Hi""[[=]][)]
            ";

            var diagnostics = @"
                Unexpected Token EqualsToken, CloseParenToken was expected.
                Unexpected Token EqualsToken, IdentifierToken was expected.
                Unexpected Token CloseParenToken, IdentifierToken was expected.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_FunctionParameters_NoInfiniteLoop()
        {
            var text = @"
                function void hi(string name[[[=]]][)]
                {
                    print(""Hi "" + name + ""!"" )
                }[]
            ";

            var diagnostics = @"
                Unexpected Token EqualsToken, CloseParenToken was expected.
                Unexpected Token EqualsToken, OpenBraceToken was expected.
                Unexpected Token EqualsToken, IdentifierToken was expected.
                Unexpected Token CloseParenToken, IdentifierToken was expected.
                Unexpected Token EndOfFileToken, CloseBraceToken was expected.
            ";

            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_No_InfiniteLoop()
        {
            var text = @"
                {
                [)][]
            ";

            var diagnostics = @"
                Unexpected Token CloseParenToken, IdentifierToken was expected.
                Unexpected Token EndOfFileToken, CloseBraceToken was expected.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_NameExpression_Reports_NoError_ForTokenFabrication()
        {
            var text = @"1 + []";

            var diagnostics = @"
                Unexpected Token EndOfFileToken, IdentifierToken was expected.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_For_Statement_Reports_CannotConvert_LowerBound()
        {
            var text = @"
                {
                    int result = 0
                    for i = [false] until 10
                        result = result + i
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'bool' to Type 'int32'.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_For_Statement_Reports_CannotConvert_UpperBound()
        {
            var text = @"
                {
                    int result = 0
                    for i = 1 until [true]
                        result = result + i
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'bool' to Type 'int32'.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_While_Statement_Reports_CannotConvert()
        {
            var text = @"
                {
                    int x = 0
                    while [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'int32' to Type 'bool'.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_If_Statement_Reports_CannotConvert()
        {
            var text = @"
                {
                    int x = 0
                    if [10]
                        x = 10
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'int32' to Type 'bool'.
            ";
            
            AssertDiagnostics(text, diagnostics);
        }
        
        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
                {
                    int x = 10
                    int y = 100
                    {
                        int x = 10
                    }
                    int [x] = 5
                }
            ";

            var diagnostics = @"
                Variable 'x' has already been declared in this Scope.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Name_Reports_Undefined()
        {
            var text = @"[x] * 10";

            var diagnostics = @"
                Variable 'x' is not defined.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Assigned_Reports_Undefined()
        {
            var text = @"[x] = 10";

            var diagnostics = @"
                Variable 'x' is not defined.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Assigned_Reports_CannotAssign()
        {
            var text = @"
                {
                    readonly x = 10
                    x [=] 0
                }
            ";

            var diagnostics = @"
                Variable 'x' is read-only and cannot be re-assigned.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Assigned_Reports_CannotConvert()
        {
            var text = @"
                {
                    int x = 10
                    x = [true]
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'bool' to Type 'int32'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Unary_Reports_Undefined()
        {
            var text = @"[+]true";

            var diagnostics = @"
                Unary Operator '+' is not defined for Type 'bool'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Binary_Reports_Undefined()
        {
            var text = @"10 [*] false";

            var diagnostics = @"
                Binary Operator '*' is not defined for Types 'int32' and 'bool'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            var nodeTree = NodeTree.Parse(text);
            var compilation = new Compilation(nodeTree);
            var variables = new Dictionary<VariableSymbol?, object>();
            var result = compilation.Evaluate(variables);
            
            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }
        
        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = NodeTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol?, object>());

            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics");

            Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

            for (var i = 0; i < expectedDiagnostics.Length; i++)
            {
                var expectedMessage = expectedDiagnostics[i];
                var actualMessage = result.Diagnostics[i].Message;
                Assert.Equal(expectedMessage, actualMessage);

                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = result.Diagnostics[i].Location.Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}