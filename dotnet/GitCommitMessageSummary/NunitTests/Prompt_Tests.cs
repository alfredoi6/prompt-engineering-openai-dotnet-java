using System.Text;
using System.Text.Json;
using NunitTests.Models;
using OpenAI.Chat;

namespace NunitTests;

public class Prompt_Tests : TestBase
{
	/// <summary>
	/// Test to validate the generation of descriptive invoice time entries from Git commit messages 
	/// using embeddings for deeper contextual understanding. 
	/// 
	/// This test evaluates the following:
	/// - Proper loading of stored embeddings.
	/// - Cosine similarity calculation for determining relatedness between current and past commit messages.
	/// - Matching the most similar embedding for each commit and providing context-based insights.
	/// - Conversion of commit messages into clear, non-technical, business-focused invoice entries.
	/// - Verification of the output to ensure correct formatting, non-empty results, and valid justification insights.
	/// 
	/// The process includes leveraging embeddings to align new commit justifications with past ones, 
	/// ensuring consistency and providing additional context for invoicing purposes.
	/// </summary>
	[Test]
	public void Summarize_Messages_For_Invoice_Using_Embeddings()
	{
		// Load the embeddings from previous test
		var storedEmbeddings = LoadJustificationEmbeddings();
		Assert.That(storedEmbeddings, Is.Not.Null, "Embeddings should have been loaded successfully.");

		// Read commits from the same test file
		string jsonContent = File.ReadAllText(_testFilePath);
		List<GitCommit>? commits = JsonSerializer.Deserialize<List<GitCommit>>(jsonContent);

		// Computes the cosine similarity between two embedding vectors.
		// Cosine similarity measures how similar two vectors are based on their directional alignment.
		// It ranges from -1 (completely opposite) to 1 (identical). A higher value means greater similarity.
		float CalculateCosineSimilarity(ReadOnlySpan<float> vec1, ReadOnlySpan<float> vec2)
		{
			float dotProduct = 0; // Accumulates the sum of element-wise multiplication of vec1 and vec2
			float magnitude1 = 0; // Sum of squared values for vec1 (used to compute magnitude)
			float magnitude2 = 0; // Sum of squared values for vec2 (used to compute magnitude)

			// Compute dot product and vector magnitudes
			for (int i = 0; i < vec1.Length; i++)
			{
				dotProduct += vec1[i] * vec2[i]; // Multiply corresponding elements and sum
				magnitude1 += vec1[i] * vec1[i]; // Sum of squares for vec1
				magnitude2 += vec2[i] * vec2[i]; // Sum of squares for vec2
			}

			// Compute the magnitude of each vector
			float magnitude = (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));

			// Return the cosine similarity (dot product divided by magnitudes)
			// If magnitude is zero (to avoid division by zero), return 0 similarity
			return magnitude == 0 ? 0 : dotProduct / magnitude;
		}

		// Finds the most similar embedding from the stored embeddings by computing cosine similarity.
		// It iterates through all stored embeddings and selects the one with the highest similarity score.
		string? GetMostSimilarEmbedding(ReadOnlyMemory<float> currentMessageEmbedding)
		{
			float maxSimilarity = float.MinValue; // Initialize similarity score to the lowest possible value
			string? mostSimilarMessage = null; // Store the closest matching commit message

			// Iterate through stored embeddings and compute similarity with the current message embedding
			foreach (var storedEmbedding in storedEmbeddings)
			{
				var similarity = CalculateCosineSimilarity(storedEmbedding.Value.Span, currentMessageEmbedding.Span);

				// Update if a higher similarity score is found
				if (similarity > maxSimilarity)
				{
					maxSimilarity = similarity;
					mostSimilarMessage = storedEmbedding.Key; // Store the corresponding commit message
				}
			}

			// Return the most similar commit message, or null if no close match is found
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
			var currentMessageEmbedding =
				GenerateEmbeddingFromText(commit.Message); // Implement this according to your embeddings source

			// Find the most similar stored embedding
			string? mostSimilar = GetMostSimilarEmbedding(currentMessageEmbedding);

			if (mostSimilar != null)
			{
				sb.AppendLine($"Embedding Insights: A similar past commit justification was found: \"{mostSimilar}\".");
				sb.AppendLine(
					"Use this past work as a reference to explain the current work in a way that aligns with previous justifications.");
				sb.AppendLine(
					"Ensure consistency in language and reasoning while adapting the justification to the specifics of the new commit.");
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