# RavenDB Movie Agent

An AI-powered **console application** for movie search and recommendations (Ai Chat), 
built with **RavenDB's AI Agent feature** and **.NET Core**.  

---

## 🚀 Features

- **RavenDB AI Agent Integration**
  - Leverages the built-in AI Agent feature in RavenDB to answer free-text queries.
  
- **Advanced Search**
  - Search movies by title, tags, or genres.
  - Apply quality filters (minimum views / minimum or maximum rating).
  
- **Vector Similarity Search**
  - Use semantic similarity on tags and genres for highly relevant recommendations.
  
- **Flexible Sorting**
  - Sort results by average rating and view count.

---

## 🛠 Requirements

1. **Local RavenDB Server**
   - Run RavenDB locally at `http://localhost:8080`.
   - [Download RavenDB](https://ravendb.net/download) if you don't have it installed.

2. **OpenAI API Key**
   - Set your OpenAI API key as an environment variable: ```RAVEN_AI_INTEGRATION_OPENAI_API_KEY```

---

## ▶️ How It Works

1. **First Run**
   - The app will create the required RavenDB database automatically on your local server.
   - ⚠ **Do not stop the application during the first run** — let it finish setting up the database.

2. **Subsequent Runs**
   - The program will start directly in the chat mode with the AI Agent.

---