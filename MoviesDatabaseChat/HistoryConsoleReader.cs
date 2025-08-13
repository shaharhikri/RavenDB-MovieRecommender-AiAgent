using System.Text;

namespace MoviesDatabaseChat
{
    internal class HistoryConsoleReader
    {
        private List<string> history = new List<string>();
        private int historyIndex = -1;

        public string ReadLine()
        {
            var inputBuffer = new StringBuilder();
            historyIndex = history.Count; // reset index to after last entry

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    var finalInput = inputBuffer.ToString();
                    if (!string.IsNullOrWhiteSpace(finalInput))
                        history.Add(finalInput);
                    return finalInput;
                }
                else if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (inputBuffer.Length > 0)
                    {
                        inputBuffer.Length--;
                        Console.Write("\b \b");
                    }
                }
                else if (keyInfo.Key == ConsoleKey.UpArrow)
                {
                    if (history.Count > 0)
                    {
                        historyIndex = Math.Max(0, historyIndex - 1);
                        ReplaceCurrentLine(inputBuffer, history[historyIndex]);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.DownArrow)
                {
                    if (history.Count > 0)
                    {
                        historyIndex = Math.Min(history.Count, historyIndex + 1);
                        if (historyIndex >= history.Count)
                            ReplaceCurrentLine(inputBuffer, "");
                        else
                            ReplaceCurrentLine(inputBuffer, history[historyIndex]);
                    }
                }
                else
                {
                    inputBuffer.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }

        private void ReplaceCurrentLine(StringBuilder buffer, string newText)
        {
            // Clear current text
            while (buffer.Length > 0)
            {
                Console.Write("\b \b");
                buffer.Length--;
            }
            // Write new text
            Console.Write(newText);
            buffer.Append(newText);
        }

    }
}
