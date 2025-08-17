namespace MoviesDatabaseChat.Entities
{
    public class Movie
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public string[] Genres { get; set; }
        public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public class User
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public HashSet<string> WatchedMovies { get; set; }
    }

    public class Rating
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string MovieId { get; set; }
        public double RatingValue { get; set; }
        public DateTime TimeStamp { get; set; }
    }

    public class MovieStat
    {
        public string MovieId { get; set; }
        public string Title { get; set; }
        public int Views { get; set; }
        public double RatingsSum { get; set; }
        public double AverageRating { get; set; }
        public string[] Genres { get; set; }
        public HashSet<string> Tags { get; set; }
    }

    public class MoviesSampleObject
    {
        public static MoviesSampleObject Instance = new()
        {
            Answer = "Answer to the user question",
            MoviesIds = ["The movies ids relevant to the query or response"],
            MoviesNames = ["The movies names relevant to the query or response"]
        };

        public string Answer;

        public List<string> MoviesIds { get; set; }
        public List<string> MoviesNames { get; set; }
    }

    public class RateToolSampleRequest
    {
        public static RateToolSampleRequest Instance = new()
        {
            MovieName = "The name of the movie the user wants to rate",
            RateValue = 4.5
        };

        public string MovieName { get; set; }
        public double RateValue { get; set; }
    }

    public class AddTagsSampleRequest
    {
        public static AddTagsSampleRequest Instance = new()
        {
            MovieName = "The name of the movie the user wants to rate",
            Tags = [ "Scary", "Disgusting" ]
        };

        public string MovieName { get; set; }
        public HashSet<string> Tags { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public class ChangeUserNameSampleRequest
    {
        public static ChangeUserNameSampleRequest Instance = new()
        {
            OldUserName = "James Parker",
            NewUserName = "James Smith"
        };

        public string NewUserName { get; set; }
        public string OldUserName { get; set; }
    }


    public class ActionToolResult
    {
        public bool IsSuccessful { get; set; }
        public string Answer { get; set; }
    }
}
