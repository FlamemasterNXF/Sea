using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using Shore.IO;

namespace Shore
{
    internal abstract class Repl
    {
        private readonly List<Command> _commands = new();
        private readonly List<string> _history = new();
        private int _historyIndex;
        private bool _done;

        protected Repl() => InitializeCommands();

        private void InitializeCommands()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                               BindingFlags.Instance | BindingFlags.FlattenHierarchy);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;

                var command = new Command(attribute.Name, attribute.Description, method);
                _commands.Add(command);
            }
        }

        public void Run()
        {
            while (true)
            {
                var text = EditSubmission();
                if (string.IsNullOrEmpty(text)) return;

                if (!text.Contains(Environment.NewLine) && text.StartsWith("#")) EvaluateCommand(text);
                else EvaluateSubmission(text);
                
                _history.Add(text);
                _historyIndex = 0;
            }
        }
        
        private void EvaluateCommand(string input)
        {
            var args = new List<string>();
            var inQuotes = false;
            var position = 1;
            var sb = new StringBuilder();
            while (position < input.Length)
            {
                var c = input[position];
                var l = position + 1>= input.Length ? '\0' : input[position + 1];

                if (char.IsWhiteSpace(c))
                {
                    if (!inQuotes) CommitPendingArgument();
                    else sb.Append(c);
                }
                else if (c == '\"')
                {
                    if (!inQuotes) inQuotes = true;
                    else if (l == '\"')
                    {
                        sb.Append(c);
                        position++;
                    }
                    else inQuotes = false;
                }
                else sb.Append(c);

                position++;
            }

            CommitPendingArgument();

            void CommitPendingArgument()
            {
                var arg = sb.ToString();
                if (!string.IsNullOrWhiteSpace(arg)) args.Add(arg);
                sb.Clear();
            }

            var commandName = args.FirstOrDefault();
            if (args.Count > 0) args.RemoveAt(0);

            var command = _commands.SingleOrDefault(mc => mc.Name == commandName);
            if (command == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Invalid command {input}.");
                Console.ResetColor();
                return;
            }

            var parameters = command.Method.GetParameters();

            if (args.Count != parameters.Length)
            {
                var parameterNames = string.Join(" ", parameters.Select(p => $"<{p.Name}>"));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: invalid number of arguments");
                Console.WriteLine($"usage: #{command.Name} {parameterNames}");
                Console.ResetColor();
                return;
            }

            var instance = command.Method.IsStatic ? null : this;
            command.Method.Invoke(instance, args.ToArray());
        }
        
        private sealed class View
        {
            private readonly Action<string> _lineRenderer;
            private readonly ObservableCollection<string> _document;
            private int _cursorTop;
            private int _renderedLineCount;
            private int _currentLine;
            private int _currentCharacter;

            public View(Action<string> lineRenderer, ObservableCollection<string> document)
            {
                _lineRenderer = lineRenderer;
                _document = document;
                _document.CollectionChanged += DocumentChanged;
                _cursorTop = Console.CursorTop;
                Render();
            }

            private void DocumentChanged(object? sender, NotifyCollectionChangedEventArgs e) => Render();

            private void Render()
            {
                Console.CursorVisible = false;
                var lineCount = 0;

                foreach (var line in _document)
                {
                    Console.SetCursorPosition(0, Math.Min(_cursorTop + lineCount, Console.BufferHeight - 1));
                    Console.ForegroundColor = ConsoleColor.Green;

                    Console.Write(lineCount == 0 ? "» " : "· ");
                    Console.ResetColor();
                    _lineRenderer(line);
                    Console.Write(new string(' ', Console.WindowWidth - line.Length - 2));
                    lineCount++;
                }
                
                var numberOfBlankLines = _renderedLineCount - lineCount;
                if (numberOfBlankLines > 0)
                {
                    var blankLine = new string(' ', Console.WindowWidth);
                    for (var i = 0; i < numberOfBlankLines; i++)
                    {
                        Console.SetCursorPosition(0, _cursorTop + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }

                _renderedLineCount = lineCount;
                Console.CursorVisible = true;
                UpdateCursorPosition();
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = Math.Min(_cursorTop + _currentLine, Console.BufferHeight - 1);
                Console.CursorLeft = 2 + _currentCharacter;
            }

            public int CurrentLine
            {
                get => _currentLine;
                set
                {
                    if (_currentLine == value) return;
                    _currentLine = value;
                    _currentCharacter = Math.Min(_document[_currentLine].Length, _currentCharacter);

                    UpdateCursorPosition();
                }
            }

            public int CurrentCharacter
            {
                get => _currentCharacter;
                set
                {
                    if (_currentCharacter != value)
                    {
                        _currentCharacter = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }
        
        private string EditSubmission()
        {
            _done = false;

            var document = new ObservableCollection<string>() { "" };
            var view = new View(RenderLine, document);

            while (!_done)
            {
                var key = Console.ReadKey(true);
                HandleKey(key, document, view);
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
            Console.WriteLine();

            return string.Join(Environment.NewLine, document);       
        }

        private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, View view)
        {
            if (key.Modifiers == default(ConsoleModifiers))
            {
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        HandleEscape(document, view);
                        break;
                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, view);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, view);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;
                    case ConsoleKey.Backspace:
                        HandleBackspace(document, view);
                        break;
                    case ConsoleKey.Delete:
                        HandleDelete(document, view);
                        break;
                    case ConsoleKey.Home:
                        HandleHome(document, view);
                        break;
                    case ConsoleKey.F2:
                        HandleEnd(document, view);
                        break;
                    case ConsoleKey.Tab:
                        HandleTab(document, view);
                        break;
                    case ConsoleKey.PageUp:
                        HandlePageUp(document, view);
                        break;
                    case ConsoleKey.PageDown:
                        HandlePageDown(document, view);
                        break;
                }
            }
            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }
            }

            if (key.KeyChar >= ' ') HandleTyping(document, view, key.KeyChar.ToString());
        }

        private void HandleEscape(ObservableCollection<string> document, View view)
        {
            document[view.CurrentLine] = string.Empty;
            view.CurrentCharacter = 0;
        }

        private void HandleEnter(ObservableCollection<string> document, View view)
        {
            var submissionText = string.Join(Environment.NewLine, document);
            if (submissionText.StartsWith("#") || IsCompleteSubmission(submissionText))
            {
                _done = true;
                return;
            }

            InsertLine(document, view);
        }

        private void HandleControlEnter(ObservableCollection<string> document, View view) => InsertLine(document, view);

        private static void InsertLine(ObservableCollection<string> document, View view)
        {
            var remainder = document[view.CurrentLine].Substring(view.CurrentCharacter);
            document[view.CurrentLine] = document[view.CurrentLine].Substring(0, view.CurrentCharacter);

            var lineIndex = view.CurrentLine + 1;
            document.Insert(lineIndex, remainder);
            view.CurrentCharacter = 0;
            view.CurrentLine = lineIndex;
        }

        private void HandleLeftArrow(ObservableCollection<string> document, View view)
        {
            if (view.CurrentCharacter > 0) view.CurrentCharacter--;
        }

        private void HandleRightArrow(ObservableCollection<string> document, View view)
        {
            var line = document[view.CurrentLine];
            if (view.CurrentCharacter <= line.Length - 1) view.CurrentCharacter++;
        }

        private void HandleUpArrow(ObservableCollection<string> document, View view)
        {
            if (view.CurrentLine > 0) view.CurrentLine--;
        }

        private void HandleDownArrow(ObservableCollection<string> document, View view)
        {
            if (view.CurrentLine < document.Count - 1) view.CurrentLine++;
        }

        private void HandleBackspace(ObservableCollection<string> document, View view)
        {
            var start = view.CurrentCharacter;
            if (start == 0)
            {
                if (view.CurrentLine == 0) return;

                var currentLine = document[view.CurrentLine];
                var previousLine = document[view.CurrentLine - 1];
                document.RemoveAt(view.CurrentLine);
                view.CurrentLine--;
                document[view.CurrentLine] = previousLine + currentLine;
                view.CurrentCharacter = previousLine.Length;
            }
            else
            {
                var lineIndex = view.CurrentLine;
                var line = document[lineIndex];
                var before = line.Substring(0, start - 1);
                var after = line.Substring(start);            
                document[lineIndex] = before + after;
                view.CurrentCharacter--;
            }
        }

        private void HandleDelete(ObservableCollection<string> document, View view)
        {
            var lineIndex = view.CurrentLine;
            var line = document[lineIndex];
            var start = view.CurrentCharacter;
            if (start >= line.Length)
            {
                if (view.CurrentLine == document.Count - 1) return;

                var nextLine = document[view.CurrentLine + 1];
                document[view.CurrentLine] += nextLine;
                document.RemoveAt(view.CurrentLine + 1);
                return;
            }

            var before = line.Substring(0, start);
            var after = line.Substring(start + 1);            
            document[lineIndex] = before + after;
        }

        private void HandleHome(ObservableCollection<string> document, View view) => view.CurrentCharacter = 0;

        private void HandleEnd(ObservableCollection<string> document, View view) => Environment.Exit(1);

        private void HandleTab(ObservableCollection<string> document, View view)
        {
            const int TabWidth = 4;
            var start = view.CurrentCharacter;
            var remainingSpaces = TabWidth - start % TabWidth;
            var line = document[view.CurrentLine];
            document[view.CurrentLine] = line.Insert(start, new string(' ', remainingSpaces));
            view.CurrentCharacter += remainingSpaces;
        }

        private void HandlePageUp(ObservableCollection<string> document, View view)
        {
            _historyIndex--;
            if (_historyIndex < 0) _historyIndex = _history.Count - 1;
            UpdateDocumentFromHistory(document, view);
        }

        private void HandlePageDown(ObservableCollection<string> document, View view)
        {
            _historyIndex++;
            if (_historyIndex > _history.Count -1) _historyIndex = 0;
            UpdateDocumentFromHistory(document, view);
        }

        private void UpdateDocumentFromHistory(ObservableCollection<string> document, View view)
        {
            if (_history.Count == 0) return;
            document.Clear();

            var historyItem = _history[_historyIndex];
            var lines = historyItem.Split(Environment.NewLine);
            foreach (var line in lines)
                document.Add(line);

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
        }

        private void HandleTyping(ObservableCollection<string> document, View view, string text)
        {
            var lineIndex = view.CurrentLine;
            var start = view.CurrentCharacter;
            document[lineIndex] = document[lineIndex].Insert(start, text);
            view.CurrentCharacter += text.Length;
        }

        protected void ClearHistory() => _history.Clear();

        protected virtual void RenderLine(string line) => Console.Write(line);

        protected abstract bool IsCompleteSubmission(string text);

        protected abstract void EvaluateSubmission(string text);
        
        [AttributeUsage(AttributeTargets.Method, AllowMultiple=false)]
        protected sealed class CommandAttribute : Attribute
        {
            public CommandAttribute(string name, string description)
            {
                Name = name;
                Description = description;
            }

            public string Name { get; }
            public string Description { get; }
        }

        private sealed class Command
        {
            public Command(string name, string description, MethodInfo method)
            {
                Name = name;
                Description = description;
                Method = method;
            }

            public string Name { get; }
            public string Description { get; }
            public MethodInfo Method { get; }
        }

        [Command("help", "Shows help")]
        protected void EvaluateHelp()
        {
            var maxNameLength = _commands.Max(c => c.Name.Length);

            foreach (var command in _commands.OrderBy(c => c.Name))
            {
                var commandParams = command.Method.GetParameters();
                if (commandParams.Length == 0)
                {
                    var paddedName = command.Name.PadRight(maxNameLength);

                    Console.Out.WritePunctuation("#");
                    Console.Out.WriteIdentifier(paddedName);
                }
                else
                {
                    Console.Out.WritePunctuation("#");
                    Console.Out.WriteIdentifier(command.Name);
                    foreach (var pi in commandParams)
                    {
                        Console.Out.Write(" ");
                        Console.Out.WritePunctuation("<");
                        Console.Out.WriteIdentifier(pi.Name);
                        Console.Out.WritePunctuation(">");
                    }

                    Console.Out.WriteLine();
                    Console.Out.Write(" ");
                    for (int _ = 0; _ < maxNameLength; _++) Console.Out.Write(" ");
                }
                
                Console.Out.Write(" ");
                Console.Out.Write(" ");
                Console.Out.Write(" ");
                Console.Out.SetForeground(ConsoleColor.DarkCyan);
                Console.Out.Write(command.Description);
                Console.Out.ResetForeground();
                Console.Out.WriteLine();
            }
        }
    }
}