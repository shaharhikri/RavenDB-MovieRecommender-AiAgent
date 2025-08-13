using System.Text.Json;
using MoviesDatabaseChat.Entities;
using Raven.Client.Documents;
using Raven.Client.Documents.AI;
using Raven.Client.Documents.Operations.AI.Agents;

namespace MoviesDatabaseChat
{
    internal class Conversation
    {
        private readonly IDocumentStore _store;
        private readonly string _userId;
        private readonly IAiConversationOperations _chat;

        public Conversation(IDocumentStore store, string chatId, string userId)
        {
            _store = store;
            _userId = userId;
            _chat = store.AI.Conversation(DatabaseBootstrapper.AiAgentIdentifier, chatId,
                creationOptions: new AiConversationCreationOptions().AddParameter("userId", userId));
        }

        public async Task<MoviesSampleObject> TalkAsync(string prompt)
        {
            _chat.SetUserPrompt(prompt);

            var agentResult = await _chat.RunAsync<MoviesSampleObject>(CancellationToken.None);

            return await HandleActionToolRequestsAsync(_store, _chat, _userId, agentResult);
        }

        private static async Task<MoviesSampleObject> HandleActionToolRequestsAsync(IDocumentStore store,
            IAiConversationOperations chat, string userId, AiAnswer<MoviesSampleObject> agentResult)
        {
            while (agentResult.Status == AiConversationResult.ActionRequired)
            {
                foreach (var request in chat.RequiredActions())
                {
                    await HandleSingleActionRequest(store, chat, userId, request);
                }

                agentResult = await chat.RunAsync<MoviesSampleObject>(CancellationToken.None);
            }

            return agentResult.Answer;
        }

        private static async Task HandleSingleActionRequest(IDocumentStore store, IAiConversationOperations chat, string userId, AiAgentActionRequest request)
        {
            try
            {
                switch (request.Name)
                {
                    case "RateMovie":
                        await RateMovieAsync(store, chat, userId, request);
                        break;
                    case "AddTags":
                        await AddTagsAsync(store, chat, userId, request);
                        break;
                    case "ChangeUserName":
                        await ChangeUserNameAsync(store, chat, userId, request);
                        break;
                    default:
                        chat.AddActionResponse(request.ToolId, new ActionToolResult
                        {
                            IsSuccessful = false,
                            FailureReason = $"Tool '{request.Name}' is Unrecognized"
                        });
                        break;
                }
            }
            catch (Exception e)
            {
                chat.AddActionResponse(request.ToolId, new ActionToolResult
                {
                    IsSuccessful = false,
                    FailureReason = "database error (server error 500): " + e.Message.Substring(0, 100)
                });
            }
        }

        private static async Task RateMovieAsync(IDocumentStore store, IAiConversationOperations chat, string userId,
            AiAgentActionRequest request)
        {
            var req = JsonSerializer.Deserialize<RateToolSampleObject>(request.Arguments);
            if (req.RateValue < 0 || req.RateValue > 5)
            {
                chat.AddActionResponse(request.ToolId, new ActionToolResult
                {
                    IsSuccessful = false,
                    FailureReason = $"Cant rate \"{req.MovieName}\" with the rate value {req.RateValue} - rate value has to be between 0 to 5"
                });
                return;
            }

            using (var session = store.OpenAsyncSession())
            {
                var movies = await session
                    .Advanced
                    .AsyncRawQuery<Movie>("from Movies where Title = $name")
                    .AddParameter("name", req.MovieName.ToLower())
                    .ToListAsync();

                if (movies == null || movies.Count == 0)
                {
                    chat.AddActionResponse(request.ToolId, new ActionToolResult
                    {
                        IsSuccessful = false,
                        FailureReason = $"Movie with the name \"{req.MovieName}\" doesn't exist on the database"
                    });
                    return;
                }

                var user = await session.LoadAsync<User>(userId);

                foreach (var m in movies)
                {
                    user.WatchedMovies.Add(m.Id);
                    await session.StoreAsync(new Rating()
                    {
                        Id = "Ratings/",
                        MovieId = m.Id,
                        UserId = userId,
                        RatingValue = req.RateValue,
                        TimeStamp = DateTime.Now
                    });
                }

                await session.SaveChangesAsync();

                chat.AddActionResponse(request.ToolId, new ActionToolResult
                {
                    IsSuccessful = true,
                    FailureReason =
                        $"Found {movies.Count} movies with the name '{req.MovieName}' and rated them by score '{req.RateValue}'"
                });
            }
        }

        private static async Task AddTagsAsync(IDocumentStore store, IAiConversationOperations chat, string userId,
            AiAgentActionRequest request)
        {

            var req = JsonSerializer.Deserialize<AddTagSampleObject>(request.Arguments);
            using (var session = store.OpenAsyncSession())
            {
                var movies = await session
                    .Advanced
                    .AsyncRawQuery<Movie>("from Movies where Title = $name")
                    .AddParameter("name", req.MovieName.ToLower())
                    .ToListAsync();

                if (movies == null || movies.Count == 0)
                {
                    chat.AddActionResponse(request.ToolId, new ActionToolResult
                    {
                        IsSuccessful = false,
                        FailureReason =
                            $"Movie with the name \"{req.MovieName}\" doesn't exist on the database"
                    });
                    return;
                }

                foreach (var m in movies)
                {
                    foreach (var t in req.Tags)
                    {
                        m.Tags.Add(t);
                    }
                }

                await session.SaveChangesAsync();

                chat.AddActionResponse(request.ToolId, new ActionToolResult
                {
                    IsSuccessful = true,
                    FailureReason =
                        $"Found {movies.Count} movies with the name '{req.MovieName}' and added them by tags [{string.Join(", ", req.Tags)}]"
                });
            }
        }

        private static async Task ChangeUserNameAsync(IDocumentStore store, IAiConversationOperations chat,
            string userId, AiAgentActionRequest request)
        {
            var req = JsonSerializer.Deserialize<ChangeUserNameObject>(request.Arguments);
            using (var session = store.OpenAsyncSession())
            {
                var user = await session.LoadAsync<User>(userId);
                if (user.Name.ToLower() != req.OldUserName.ToLower())
                {
                    chat.AddActionResponse(request.ToolId, new ActionToolResult
                    {
                        IsSuccessful = false,
                        FailureReason = $"Your old name isn't '{req.OldUserName}'"
                    });
                    return;
                }

                user.Name = req.NewUserName;
                await session.SaveChangesAsync();

                chat.AddActionResponse(request.ToolId, new ActionToolResult
                {
                    IsSuccessful = true,
                    FailureReason = $"Name of user '{user.Id}' changed from '{req.OldUserName}' to '{req.NewUserName}'"
                });
            }
        }
    }
}
