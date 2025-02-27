using System.Text.Json;
using OpenAI;
using OpenAI.Embeddings;

namespace NunitTests;

public class Embedding_Storage_Tests : TestBase
{
    
    
    private List<string> _justificationDocuments;

    [SetUp]
    public void Setup()
    {


        _justificationDocuments = new List<string>
        {
            "Justification: Implemented user authentication to enhance application security and enable personalized user experiences. This feature is critical for compliance and user data protection.",
            "Justification: Resolved a critical data corruption bug that was causing data loss and system instability. This fix ensures data integrity and prevents potential business disruptions.",
            "Justification: Optimized database queries to improve application performance and reduce response times. This optimization enhances user experience and reduces server load.",
            "Justification: Refactored existing code to improve code readability, maintainability, and reduce technical debt. This refactoring will facilitate future development and reduce the risk of introducing new bugs.",
            "Justification: Integrated a third-party payment gateway to enable secure online transactions. This integration expands the application's functionality and provides users with a convenient payment option.",
            "Justification: Implemented responsive design improvements to ensure a consistent and user-friendly experience across different devices. This enhancement improves accessibility and user satisfaction.",
            "Justification: Developed and executed unit and integration tests to ensure code quality and prevent regressions. This testing is essential for maintaining a stable and reliable application.",
            "Justification: Updated API documentation to provide clear and accurate information for developers integrating with the application. This documentation is crucial for external developers and ensures smooth integration.",
            "Justification: Applied a security patch to address a known vulnerability and protect the application from potential attacks. This patch is essential for maintaining application security and preventing data breaches.",
            "Justification: Upgraded a critical dependency library to benefit from performance improvements, security fixes, and new features. This upgrade ensures the application remains up-to-date and secure."
        };
    }



    
    [Test]
    public async Task Store_Justification_Embeddings()
    {
        var justificationEmbeddings = new Dictionary<string, ReadOnlyMemory<float>>();
        
        foreach (var document in _justificationDocuments)
        {
            var embeddingVectors = GenerateEmbeddingFromText(document);
            justificationEmbeddings[document] = embeddingVectors;
        }

        // Serialize and save to disk
        var json = JsonSerializer.Serialize(justificationEmbeddings);
        File.WriteAllText(_embeddingsFilePath, json);

        Assert.That(File.Exists(_embeddingsFilePath));
    }

    [Test]
    public void LoadJustificationEmbeddings_FromDisk_DeserializationTest()
    {
        // Act: Use the existing file on disk and call LoadJustificationEmbeddings
        var loadedEmbeddings = LoadJustificationEmbeddings();

        // Assert: Ensure the deserialization is successful and not null
        Assert.That(loadedEmbeddings, Is.Not.Null, "Deserialized embeddings are null!");
        Assert.That(loadedEmbeddings.Count, Is.GreaterThan(0),
            "No embeddings were deserialized! The file might be empty or invalid.");

        // Validate one or more expected entries in the deserialized dictionary
        var expectedKey = _justificationDocuments.FirstOrDefault();
            
        Assert.That(loadedEmbeddings.ContainsKey(expectedKey), Is.True,
            $"Key '{expectedKey}' was not found in the deserialized data!");
    }
    
 
}