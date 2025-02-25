# AI-Powered Invoice Generator for Development Teams

## Overview

This open-source project is designed to help both .NET and Java teams implement basic AI integration and prompt engineering using OpenAI's ChatGPT API. It demonstrates how to process Git commit history and transform it into invoice-ready, non-technical time entries using AI-driven text generation.

## Features

- **ASP.NET Core MVC Infrastructure**  
  - Built using the Microsoft template for individual accounts  
  - Includes out-of-the-box Identity Provider for authentication and user management  

- **Integration with OpenAI ChatGPT API**  
  - Uses AI to process and refine commit messages  
  - Implements prompt engineering for structured output  

- **Processing CSV Files with Git Commit Data**  
  - Reads commit messages from CSV export  
  - Structures commit data for AI-assisted processing  

- **Invoice-Ready Time Entry Generation**  
  - Converts raw Git commit messages into non-technical, client-friendly descriptions  
  - Generates structured time entries for billing and invoicing  

- **Retrieval-Augmented Generation (RAG) for Contextual Message Output**  
  - Uses past data to improve the relevance and accuracy of AI responses  
  - Enhances prompt effectiveness based on historical context  

## Purpose

- **AI Adoption for Developers**  
  - Introduces AI-driven automation to software development teams  
  - Bridges the gap between raw development logs and client-friendly reporting  

- **Enhancing Prompt Engineering Skills**  
  - Demonstrates effective prompt construction and refinement  
  - Helps teams understand how AI models can be leveraged in their workflows  

## Getting Started

### Prerequisites

- .NET 8.0 SDK (or latest version)
- OpenAI API Key (for ChatGPT integration)
- SQL Server (or SQLite for local development)
- Git CLI (for exporting commit history)

### Installation

1. **Clone the Repository**  
   ```sh
   git clone https://github.com/yourusername/ai-invoice-generator.git
   cd ai-invoice-generator
   ```

2. **Setup Database**  
   ```sh
   dotnet ef database update
   ```

3. **Run the Application**  
   ```sh
   dotnet run
   ```

## Usage

1. **Export Git Commit History to CSV**  
   - Use `git log` or a tool like `git-quick-stats` to extract commit data  
   - Ensure the CSV format aligns with the expected input  

2. **Upload CSV & Generate Invoice Entries**  
   - Use the web interface to upload a CSV file  
   - AI processes commit messages into human-friendly descriptions  

3. **Review & Edit Entries**  
   - Modify descriptions if necessary  
   - Download structured invoice data  

## Contribution

Contributions are welcome! If you'd like to improve the AI prompts, enhance the MVC structure, or refine the RAG-based message processing, feel free to submit a pull request.

## License

This project is licensed under the MIT License.
