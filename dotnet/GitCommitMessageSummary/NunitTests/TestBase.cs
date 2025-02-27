using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Schema.Generation;
using NunitTests.Models;
using OpenAI;
using OpenAI.Chat;

namespace NunitTests
{
    public abstract class TestBase
    {
        protected IConfiguration _configuration { get; private set; }
        protected string _testFilePath;
        protected string _embeddingsFilePath = "";
        protected TestBase()
        {
            LoadConfigurationWithUserSecrets();
        }

        /// <summary>
        /// Loads configuration from appsettings.json and applies user secrets.
        /// </summary>
        private void LoadConfigurationWithUserSecrets()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddUserSecrets<TestBase>();
			
            _configuration = builder.Build();

         
        }

        [SetUp]
        public void Setup()
        {
	        // Path to the test JSON file (adjust as needed)
	        _testFilePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Assets", "sample_git_commit_history_output.json");

	        // Ensure the test file exists before running tests
	        if (!File.Exists(_testFilePath))
		        throw new FileNotFoundException($"Test JSON file not found: {_testFilePath}");
            
            
            var embeddingsOutputPath = GetIdeVisibleOutputDirectoryPath("test_embeddings");
            // Ensure the output directory exists
            Directory.CreateDirectory(embeddingsOutputPath);
            var fileName = "justification_embeddings.json";
            _embeddingsFilePath = Path.Combine(embeddingsOutputPath, fileName);
        }

        protected string GetIdeVisibleOutputDirectoryPath(string folderName)
        {
            // Get the file path where this unit test class is saved
            var filePath = new StackTrace(true).GetFrame(0)!.GetFileName();
            var folderPath = Path.GetDirectoryName(filePath);

            // Define the output directory inside the test folder
            var outputPath = Path.Combine(folderPath!, folderName);
            return outputPath;
        }
        
        /// <summary>
        /// Writes content to the console and a JSON file inside the 'test_output' directory.
        /// </summary>
        /// <param name="testName">The name of the test method.</param>
        /// <param name="content">The content to log.</param>
        protected void WriteToConsoleAndFile(string testName, string content)
        {
            Console.WriteLine(content);
            // Define the output directory inside the test folder
            var outputPath = GetIdeVisibleOutputDirectoryPath("test_output");
            
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var fileName = $"{testName}_{timestamp}.json";
            var fullPath = Path.Combine(outputPath, fileName);

            // Ensure the output directory exists
            Directory.CreateDirectory(outputPath);

            // Write content to file
            File.WriteAllText(fullPath, content);
        }

        /// <summary>
        /// Serializes an object to an indented JSON string.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <param name="objectToSerialize">Object to serialize.</param>
        /// <returns>Indented JSON string.</returns>
        protected string GetIndentedJsonString<T>(T objectToSerialize)
        {
            return JsonSerializer.Serialize(objectToSerialize, new JsonSerializerOptions { WriteIndented = true });
        }

        protected ReadOnlyMemory<float> GenerateEmbeddingFromText(string textInput)
        {
            var apiKey = _configuration["OPENAI_API_KEY"];
            var _openAIClient = new OpenAIClient(apiKey);
            var embeddingClient = _openAIClient.GetEmbeddingClient("text-embedding-ada-002");
            var embeddingResult = embeddingClient.GenerateEmbedding(textInput);
            var embedding = embeddingResult.Value;
            return embedding.ToFloats();
        }
        
        /// <summary>
        /// Calls OpenAI's chat model and returns a response.
        /// </summary>
        /// <param name="messages">List of chat messages.</param>
        /// <param name="temperature">Model's creativity setting.</param>
        /// <param name="presencePenalty">Presence penalty setting.</param>
        /// <param name="frequencyPenalty">Frequency penalty setting.</param>
        /// <param name="model">OpenAI model to use.</param>
        /// <returns>Response from OpenAI.</returns>
        protected string GetResponseFromChat(
            List<ChatMessage> messages, 
            float temperature = 0.4f, 
            float presencePenalty = 0.0f, 
            float frequencyPenalty = 0.0f, 
            string model = "gpt-4o-2024-08-06")
        {
            // Retrieve API key from user secrets
            string apiKey = _configuration["OPENAI_API_KEY"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is missing. Ensure it's set in user secrets.");
            }

            // Get the schema of the class for the structured response
            ChatClient client = new(model: model, apiKey: apiKey);
            var generator = new JSchemaGenerator();
            var jsonSchema = generator.Generate(typeof(InvoiceEntryList)).ToString();

            var responseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "invoiceentryoutput",
                BinaryData.FromString(jsonSchema));

            ChatCompletionOptions options = new()
            {
                ResponseFormat = responseFormat,
                Temperature = temperature,
                MaxOutputTokenCount = 12000,
                PresencePenalty = presencePenalty,
                FrequencyPenalty = frequencyPenalty,
            };

            ChatCompletion completion = client.CompleteChat(messages, options);

            return completion.Content[0].Text;
        }
        
        
        public Dictionary<string, ReadOnlyMemory<float>>? LoadJustificationEmbeddings()
        {
            if (!File.Exists(_embeddingsFilePath))
            {
                return new Dictionary<string, ReadOnlyMemory<float>>(); // Return empty if file doesn't exist
            }

            var json = File.ReadAllText(_embeddingsFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<float>>>(json);
        }
    }
}
