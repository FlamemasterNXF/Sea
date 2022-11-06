using Shore.CodeAnalysis;
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
        [InlineData("(1)", 1)]
        [InlineData("13 + 14", 27)]
        [InlineData("96 - 3", 93)]
        [InlineData("9 * 10", 90)]
        [InlineData("18 / 9", 2)]
        [InlineData("(1 + 2) * 3", 9)]
        [InlineData("1 == 2", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 4", true)]
        [InlineData("13 != 13", false)]
        [InlineData("false == true", false)]
        [InlineData("false == false", true)]
        [InlineData("true == true", true)]
        [InlineData("true != true", false)]
        [InlineData("true != false", true)]
        [InlineData("false != false", false)]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("let a = 10", 10)]
        [InlineData("{ let b = 0 (b = 10) * b}", 100)]
        public void Evaluator_Computes_CorrectValues(string text, object expectedValue)
        {
            AssertValue(text, expectedValue);
        }
        
        [Fact]
        public void Evaluator_VariableDeclaration_Reports_Redeclaration()
        {
            var text = @"
                {
                    let x = 10
                    let y = 100
                    {
                        let x = 10
                    }
                    let [x] = 5
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
                    let x = 10
                    x = [true]
                }
            ";

            var diagnostics = @"
                Cannot Convert Type 'Boolean' to Type 'Int32'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Unary_Reports_Undefined()
        {
            var text = @"[+]true";

            var diagnostics = @"
                Unary Operator '+' is not defined for Type 'Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_Binary_Reports_Undefined()
        {
            var text = @"10 [*] false";

            var diagnostics = @"
                Binary Operator '*' is not defined for Types 'Int32' and 'Boolean'.
            ";

            AssertDiagnostics(text, diagnostics);
        }

        private static void AssertValue(string text, object expectedValue)
        {
            var nodeTree = NodeTree.Parse(text);
            var compilation = new Compilation(nodeTree);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = compilation.Evaluate(variables);
            
            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }
        
        private void AssertDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);
            var syntaxTree = NodeTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

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
                var actualSpan = result.Diagnostics[i].Span;
                Assert.Equal(expectedSpan, actualSpan);
            }
        }
    }
}