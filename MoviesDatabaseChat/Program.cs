using MoviesDatabaseChat;
using MoviesDatabaseChat.Entities;
using Raven.Client.Documents;

class Program
{
    public static async Task Main()
    {
        using var store = new DocumentStore
        {
            Urls = new[] { "http://localhost:8080" },
            Database = "MoviesDB2"
        }.Initialize();

        if (await DatabaseBootstrapper.CreateDatabaseAsync(store, log: Console.WriteLine, smallDb: true))
        {
            Console.WriteLine($"Database '{store.Database}' is ready on your local server, run again for chat");
            return;
        }

        // db is already exists -> start conversation
        await StartConversationAsync(store);
    }

    private static async Task StartConversationAsync(IDocumentStore store)
    {
        var (chatId, userId) = GetChatIdAndUserId();

        var conversation = new Conversation(store, chatId, userId);

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("Conversation started (write something to the agent): ");
        Console.ForegroundColor = ConsoleColor.White;
        var consoleReader = new HistoryConsoleReader();

        while (true)
        {
            var input = consoleReader.ReadLine(); // same as 'Console.ReadLine()' but with history - can use arrow up/down keys to get previous inputs
            
            if (string.IsNullOrEmpty(input))
            {
                PrintEmptyAnswer();
                continue;
            }

            if (await ShouldEndChatAsync(store, chatId, input))
                break;

            var answer = await conversation.TalkAsync(input);
            PrintAnswer(answer);
        }

        Console.WriteLine("Goodbye!");
    }

    private static (string chatId, string userId) GetChatIdAndUserId()
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Enter ChatId: ");
        Console.ForegroundColor = ConsoleColor.White;
        string chatId = Console.ReadLine().Trim();
        if (chatId == string.Empty)
        {
            chatId = "Chats/";
            Console.SetCursorPosition(14, Console.CursorTop - 1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(chatId);
            Console.ForegroundColor = ConsoleColor.White;
        }
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Enter UserId: ");
        Console.ForegroundColor = ConsoleColor.White;
        string userId = Console.ReadLine().Trim();
        if (userId == string.Empty)
        {
            userId = "Users/1";
            Console.SetCursorPosition(14, Console.CursorTop - 1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(userId);
            Console.ForegroundColor = ConsoleColor.White;
        }

        return (chatId, userId);
    }

    private static async Task<bool> ShouldEndChatAsync(IDocumentStore store, string chatId, string input)
    {
        if (input?.ToLower().Trim() == "exit")
            return true;

        if (input?.ToLower().Trim() == "exit and remove chat")
        {
            using var session = store.OpenAsyncSession();
            session.Delete(chatId);
            await session.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private static void PrintAnswer(MoviesSampleObject movie)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Answer: {movie.Answer}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        if (movie.MoviesIds.Count > 0)
            Console.WriteLine($"Movies ids: [{string.Join(", ", movie.MoviesIds)}]");

        if (movie.MoviesNames.Count > 0)
            Console.WriteLine($"Movies names: [{string.Join(", ", movie.MoviesNames)}]");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private static void PrintEmptyAnswer()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.SetCursorPosition(0, Console.CursorTop - 1);
        Console.WriteLine($"Prompt cannot be empty, try again");
        Console.ForegroundColor = ConsoleColor.White;
    }

}