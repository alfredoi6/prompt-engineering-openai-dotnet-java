using System.Text.Json;
using NunitTests.Models;

namespace NunitTests;

public class Commit_Output_Tests : TestBase
{


	[Test]
	public void Read_File_For_Test()
	{
		// Act
		string jsonContent = File.ReadAllText(_testFilePath);

		// Assert
		Assert.That(!String.IsNullOrEmpty(jsonContent), "JSON content should not be null or empty");

	}

	[Test]
	public void Deserialize_File_To_Objects()
	{
		// Act
		string jsonContent = File.ReadAllText(_testFilePath);
		List<GitCommit> commits = JsonSerializer.Deserialize<List<GitCommit>>(jsonContent);

		// Assert
		Assert.That(!String.IsNullOrEmpty(jsonContent), "JSON content should not be null or empty");
		Assert.That(commits != null, "Deserialized object should not be null");
		Assert.That(commits.Count > 0, "Deserialized list should not be empty");

		// Validate the first commit structure
		var firstCommit = commits.FirstOrDefault();

		foreach (var commit in commits)
		{
			Assert.That(!string.IsNullOrEmpty(commit.Commit), "Commit hash should not be null or empty");
			Assert.That(!string.IsNullOrEmpty(commit.Date), "Commit date should not be null or empty");
			Assert.That(!string.IsNullOrEmpty(commit.Author), "Commit author should not be null or empty");
			Assert.That(!string.IsNullOrEmpty(commit.Email), "Commit email should not be null or empty");
			Assert.That(!string.IsNullOrEmpty(commit.Message), "Commit message should not be null or empty");
			Assert.That(commit.Files != null, "Commit files should not be null");
			Assert.That(commit.Files.Count > 0, "Commit files list should not be empty");

			// Ensure all file paths are valid (not null or empty)
			foreach (var file in commit.Files)
			{
				Assert.That(!string.IsNullOrEmpty(file), "File path in commit should not be null or empty");
			}
		}
	}
}