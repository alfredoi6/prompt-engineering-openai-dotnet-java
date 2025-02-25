using System.Text.Json.Serialization;

namespace GitCommitMessageSummaryTests.Models;

public class GitCommit
{
	[JsonPropertyName("date")]
	public string Date { get; set; }

	[JsonPropertyName("commit")]
	public string Commit { get; set; }

	[JsonPropertyName("author")]
	public string Author { get; set; }

	[JsonPropertyName("email")]
	public string Email { get; set; }

	[JsonPropertyName("message")]
	public string Message { get; set; }

	[JsonPropertyName("files")]
	public List<string> Files { get; set; }
    
	public override string ToString()
	{
		return $"Commit: {Commit}\nDate: {Date}\nAuthor: {Author} ({Email})\nMessage: {Message}\nFiles: {string.Join(", ", Files)}\n";
	}
}