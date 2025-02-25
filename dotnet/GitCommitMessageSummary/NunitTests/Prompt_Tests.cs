using System.Text;
using System.Text.Json;
using GitCommitMessageSummaryTests.Models;

using OpenAI.Chat;

namespace GitCommitMessageSummaryTests;

public class Prompt_Tests : TestBase
{

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