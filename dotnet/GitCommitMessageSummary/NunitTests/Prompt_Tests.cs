using System.Text;
using System.Text.Json;
using NunitTests.Models;
using OpenAI.Chat;

namespace NunitTests;

public class Prompt_Tests : TestBase
{



	[Test]
	public void Summarize_Messages_For_Invoice_Using_Embeddings()
	{
		// Load the embeddings from previous test
		var storedEmbeddings = LoadJustificationEmbeddings();
		Assert.That(storedEmbeddings, Is.Not.Null, "Embeddings should have been loaded successfully.");

		// Read commits from the same test file
		string jsonContent = File.ReadAllText(_testFilePath);
		List<GitCommit>? commits = JsonSerializer.Deserialize<List<GitCommit>>(jsonContent);

		// Function to calculate cosine similarity
		float CalculateCosineSimilarity(ReadOnlySpan<float> vec1, ReadOnlySpan<float> vec2)
		{
			float dotProduct = 0;
			float magnitude1 = 0;
			float magnitude2 = 0;

			for (int i = 0; i < vec1.Length; i++)
			{
				dotProduct += vec1[i] * vec2[i];
				magnitude1 += vec1[i] * vec1[i];
				magnitude2 += vec2[i] * vec2[i];
			}

			float magnitude = (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
			return magnitude == 0 ? 0 : dotProduct / magnitude;
		}

		// Function to find the most similar embedding
		string? GetMostSimilarEmbedding(ReadOnlyMemory<float> currentMessageEmbedding)
		{
			float maxSimilarity = float.MinValue;
			string? mostSimilarMessage = null;

			foreach (var storedEmbedding in storedEmbeddings)
			{
				var similarity = CalculateCosineSimilarity(storedEmbedding.Value.Span, currentMessageEmbedding.Span);
				if (similarity > maxSimilarity)
				{
					maxSimilarity = similarity;
					mostSimilarMessage = storedEmbedding.Key;
				}
			}

			return mostSimilarMessage;
		}

		// Prepare string builder for the prompt
		var sb = new StringBuilder();

		foreach (var commit in commits)
		{
			sb.AppendLine($"Date: {commit.Date}");
			sb.AppendLine($"Author: {commit.Author}");
			sb.AppendLine($"Commit Message: {commit.Message}");

			// Generate embedding for the commit message (assumed function for demonstration)
			var currentMessageEmbedding = GenerateEmbeddingFromText(commit.Message); // Implement this according to your embeddings source

			// Find the most similar stored embedding
			string? mostSimilar = GetMostSimilarEmbedding(currentMessageEmbedding);

			if (mostSimilar != null)
			{
				sb.AppendLine($"Embedding Insights: A similar past commit justification was found: \"{mostSimilar}\".");
				sb.AppendLine("Use this past work as a reference to explain the current work in a way that aligns with previous justifications.");
				sb.AppendLine("Ensure consistency in language and reasoning while adapting the justification to the specifics of the new commit.");
			}

			sb.AppendLine($"Files Changed: {string.Join(", ", commit.Files)}");
			sb.AppendLine("-----");
		}

		var commitPromptWithEmbeddings = sb.ToString();

		// Prepare chat messages with the updated prompt
		List<ChatMessage> messages = new()
		{
			new SystemChatMessage(
				"You are a highly skilled AI assistant trained in software development and project management communication. "
				+ "Your task is to transform raw Git commit messages into clear, non-technical time entries for invoicing purposes. "
				+ "Use embeddings insights to provide additional justification and business context for the recorded actions. "
				+ "These entries should be easy for project managers, product managers, and clients to understand. "
				+ "Avoid technical jargon and focus on explaining the work done in business terms."
			),
			new UserChatMessage(
				"I will provide you with a list of Git commit messages. Your task is to convert them into "
				+ "descriptive invoice time entries. Each entry should include:\n"
				+ "- A summary of the work done in non-technical language\n"
				+ "- An estimated time spent on the task (assume 2-4 hours per commit unless specified)\n"
				+ "- A justification of why the work was necessary, leveraging embeddings data where applicable\n\n"
				+ "Here are the commit messages:\n\n"
				+ commitPromptWithEmbeddings
			)
		};

		// Get AI-generated response and parse as invoice entries
		var jsonString = GetResponseFromChat(messages);

		var invoiceEntryOutput = JsonSerializer.Deserialize<InvoiceEntryList>(jsonString);

		Assert.That(invoiceEntryOutput.InvoiceEntries.Count > 0, "Should have returned multiple results");
		var dump = new
		{
			messages,
			recommendations = invoiceEntryOutput
		};

		var serializedRecommendations = GetIndentedJsonString(dump);
		WriteToConsoleAndFile("Invoice_Summary_Using_Embeddings", serializedRecommendations);

		Assert.Pass();
	}

	/// <summary>
	/// This test will take all commit history output and convert it into a entry that is
	/// fit to be added to an invoice for consumption by a product owner, manager, accounts payable personel
	/// or by a ceo
	/// </summary>
	[Test]
	public void Summarize_Messages_For_Invoice()
	{
		string jsonContent = File.ReadAllText(_testFilePath);
		List<GitCommit>? commits = JsonSerializer.Deserialize<List<GitCommit>>(jsonContent);

		var sb = new StringBuilder();
		foreach (var commit in commits)
		{
			sb.AppendLine($"Date: {commit.Date}");
			sb.AppendLine($"Author: {commit.Author}");
			sb.AppendLine($"Commit Message: {commit.Message}");
			sb.AppendLine($"Files Changed: {string.Join(", ", commit.Files)}");
			sb.AppendLine("-----");
		}

		var commitPrompt = sb.ToString();

		List<ChatMessage> messages = new()
		{
			new SystemChatMessage(
				"You are a highly skilled AI assistant trained in software development and project management communication. "
				+ "Your task is to transform raw Git commit messages into clear, non-technical time entries for invoicing purposes. "
				+ "These entries should be easy for project managers, product managers, and clients to understand. "
				+ "Avoid technical jargon and focus on explaining the work done in business terms."
			),
			new UserChatMessage(
				"I will provide you with a list of Git commit messages. Your task is to convert them into "
				+ "descriptive invoice time entries. Each entry should include:\n"
				+ "- A summary of the work done in non-technical language\n"
				+ "- An estimated time spent on the task (assume 2-4 hours per commit unless specified)\n"
				+ "- A justification of why the work was necessary\n\n"
				+ "Here are the commit messages:\n\n"
				+ commitPrompt
			)
		};

		var jsonString = GetResponseFromChat(messages);
		
        
		var invoiceEntryOutput = JsonSerializer.Deserialize<InvoiceEntryList>(jsonString);

		Assert.That(invoiceEntryOutput.InvoiceEntries.Count > 0, "Should have returned multiple results");
		var dump = new
		{
			messages, 
			recommendations = invoiceEntryOutput
		};
        
		var serializedRecommendations = GetIndentedJsonString(dump);
		WriteToConsoleAndFile("Invoice_Summary",serializedRecommendations);

		Assert.Pass();
	}
}