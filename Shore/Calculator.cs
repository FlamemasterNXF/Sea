using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shore
{
    public class Calculator
    {
        internal static double Calculate(string calc)
        {
            // remove 2+ chained spaces
            string FixSpaces(string text) =>
                !Regex.IsMatch(text, @"(\s){2,}") ? text : Regex.Replace(text, @"(\s){2,}", " ");

            // equal opening and closing ()
            if (calc.Count(c => c == '(') != calc.Count(c => c == ')')) return double.NaN;
            var rawCalc = calc.Replace(" ", "");

            // allow for ()() => ()*()
            rawCalc = rawCalc.Replace(")(", ")*(");

            // allow for n() => n * ()
            foreach (var match in Regex.Matches(rawCalc, @"\d\(").Select(m => m.Value))
                rawCalc = rawCalc.Replace(match, match.Replace("(", "*("));

            // allow for ()n => () * n
            foreach (var match in Regex.Matches(rawCalc, @"\)\d").Select(m => m.Value))
                rawCalc = rawCalc.Replace(match, match.Replace(")", ")*"));

            var realCalc = rawCalc.Replace("--", "+").Replace("(", " ( ").Replace(")", " ) ").Replace("+", " + ")
                .Replace("-", " - ")
                .Replace("*", " * ").Replace("/", " / ").Replace("^", " ^ ").Replace("%", " % ").Replace("!", " ! ");

            realCalc = FixSpaces(realCalc);
            //Console.WriteLine($"Solving: {realCalc}");
            var step = 1;
            var same = "";
            var sameCount = 0;

            while (Regex.IsMatch(realCalc, @"\([^\(]*?\)"))
            {
                var match = Regex.Match(realCalc, @"\([^\(]*?\)").Value;
                var indx = realCalc.IndexOf(match, StringComparison.Ordinal);
                var before = realCalc.Replace(" ", "");
                if (same == before) sameCount++;
                else sameCount = 0;
                same = before;
                realCalc = realCalc.Remove(indx, match.Length);

                if (Regex.IsMatch(match, @"(\( (\d+?) \))"))
                {
                    realCalc = realCalc.Insert(indx, Regex.Match(match, @"\( (\d+?) \)").Groups[1].Value);
                    //Console.WriteLine($"Step {step++}: {before} => {realCalc.Replace(" ", "")}");
                }
                else
                    realCalc = realCalc.Insert(indx,
                        $"{Solve(Regex.Match(match, @"\( (.*?) \)").Groups[1].Value, ref step)}");

                realCalc = FixSpaces(realCalc).Replace("--", "+");
                if (sameCount >= 25) throw new Exception("Terminated due to repetition");
            }

            return Solve(realCalc, ref step);
        }

        private static double Solve(string calc, ref int step)
        {
            var equation = calc.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();

            void OperatorSolve(string op, Func<double, double, double> operation)
            {
                var indx = equation.IndexOf(op);
                string hold;
                switch (op)
                {
                    case "-" when string.Join("", equation).Contains("--"):
                        var together = string.Join(" ", equation);
                        together = together.Replace("- -", "+");
                        equation = together.Split(" ").ToList();
                        return;
                    case "-" when equation.Count < 3:
                        equation = new List<string> { string.Join("", equation) };
                        return;
                    case "-" when indx == 0 || !Regex.IsMatch(equation[indx - 1], @"\d*"):
                        hold = equation[indx + 1];
                        equation.RemoveRange(indx, 2);
                        equation.Insert(indx, $"-{hold}");
                        return;
                    case "+" when equation.Count < 3:
                        equation.RemoveAt(0);
                        return;
                    case "+" when indx == 0 || !Regex.IsMatch(equation[indx - 1], @"\d*"):
                        hold = equation[indx + 1];
                        equation.RemoveRange(indx, 2);
                        equation.Insert(indx, hold);
                        return;
                    case "+" when equation[indx + 1] == "-":
                        equation[indx + 1] = string.Join("", equation.GetRange(indx + 1, 2));
                        equation.RemoveAt(indx + 2);
                        break;
                    case "*" when equation[indx + 1] == "-":
                        equation[indx + 1] = string.Join("", equation.GetRange(indx + 1, 2));
                        equation.RemoveAt(indx + 2);
                        break;
                    case "/" when equation[indx + 1] == "-":
                        equation[indx + 1] = string.Join("", equation.GetRange(indx + 1, 2));
                        equation.RemoveAt(indx + 2);
                        break;
                    case "%" when equation[indx + 1] == "-":
                        equation[indx + 1] = string.Join("", equation.GetRange(indx + 1, 2));
                        equation.RemoveAt(indx + 2);
                        break;
                    default:
                        try
                        {
                            var n = operation.Invoke(double.Parse(equation[indx - 1]),
                                double.Parse(equation[indx + 1]));
                            equation.RemoveRange(indx - 1, 3);
                            equation.Insert(indx - 1, $"{n}");
                        }
                        catch
                        {
                            //Console.WriteLine($"Either {equation[indx - 1]} or {equation[indx + 1]} is incorrect");
                            throw new Exception("kek");
                        }

                        break;
                }
            }

            var same = "";
            var sameCount = 0;

            while (equation.Count != 1)
            {
                var before = string.Join("", equation);
                if (same == before) sameCount++;
                else sameCount = 0;
                same = before;

                if (equation.Contains("!"))
                {
                    var indx = equation.IndexOf("!");
                    var n = Factorial(int.Parse(equation[indx - 1]));
                    equation.RemoveRange(indx - 1, 2);
                    equation.Insert(indx - 1, $"{n}");
                }
                else if (equation.Contains("^")) OperatorSolve("^", Math.Pow);
                else if (equation.Contains("*") || equation.Contains("/") || equation.Contains("%"))
                {
                    if ((equation.IndexOf("*") < equation.IndexOf("/") || equation.IndexOf("/") == -1) &&
                        equation.Contains("*")) OperatorSolve("*", (d1, d2) => d1 * d2);
                    else if (equation.Contains("/")) OperatorSolve("/", (d1, d2) => d1 / d2);
                    else if (equation.Contains("%")) OperatorSolve("%", (d1, d2) => d1 % d2);
                }
                else if (equation.Contains("+") || equation.Contains("-"))
                {
                    if ((equation.IndexOf("+") < equation.IndexOf("-") || equation.IndexOf("-") == -1) &&
                        equation.Contains("+")) OperatorSolve("+", (d1, d2) => d1 + d2);
                    else if (equation.Contains("-")) OperatorSolve("-", (d1, d2) => d1 - d2);
                }
                else if (equation.Count == 2) equation = new List<string> { $"{equation.Select(double.Parse).Sum()}" };

                equation = equation.Select(s => s.Replace("--", "")).ToList();

                //Console.WriteLine($"Step {step++}. {before} => {string.Join("", equation)}");
                if (sameCount >= 25) throw new Exception("Terminated due to repetition");
            }

            return double.Parse(equation[0]);
        }

        private static double Factorial(int i)
        {
            switch (i)
            {
                case < 0:
                    return double.NaN;
                case < 2:
                    return 1;
            }

            var val = 2d;
            for (var j = 3; j <= i; j++) val *= j;
            return val;
        }

        private static string SubVar(string eq, char var, double sub) => eq.Replace($"{var}", $"({sub})");
        private static double CalSubVar(string eq, char var, double sub) => Calculate(SubVar(eq, var, sub));

        private static double PartialSum(string eq, long start, long end)
        {
            var a1 = CalSubVar(eq, 'n', start);
            var an = CalSubVar(eq, 'n', end);
            return Calculate($"({end - start + 1}/2)({a1}+{an})");
        }

        private static double PartialSumGeo(double a1, double ratio, int end) =>
            Calculate($"({a1}(1-{ratio}^{end}))/(1-{ratio})");

        private static double CommonDifference(double[] series)
        {
            var pattern = new double[series.Length - 1];
            for (var i = 1; i < series.Length; i++) pattern[i - 1] = series[i] - series[i - 1];
            //Program.Write($"[{string.Join(",", pattern)}]");
            pattern = pattern.Union(pattern).ToArray();
            return pattern.Length == 1 ? pattern[0] : double.NaN;
        }

        private static double CommonRatio(double[] series)
        {
            var pattern = new double[series.Length - 1];
            for (var i = 1; i < series.Length; i++) pattern[i - 1] = series[i] / series[i - 1];
            //Program.Write($"[{string.Join(",", pattern)}]");
            pattern = pattern.Union(pattern).ToArray();
            return pattern.Length == 1 ? pattern[0] : double.NaN;
        }
    }
}