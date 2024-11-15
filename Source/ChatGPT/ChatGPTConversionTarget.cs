using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Exodus.Stride
{
    public class ChatGPTConversionTarget : IConversionTarget
    {
        string gameEngineName;

        private static readonly string OpenAiApiKey = "your-openai-api-key";  // Replace with your API Key
        private static readonly string OpenAiEndpoint = "https://api.openai.com/v1/completions";


        public ChatGPTConversionTarget(string gameEngineName)
        {
            this.gameEngineName = gameEngineName; 
        }

        public async Task<string> TransformCodeAsync(Document document)
        {
            var sourceText = await document.GetTextAsync();
            var source = sourceText.ToString();

            // Prepare the prompt to ask ChatGPT for Unity to Stride conversion
            string prompt = $"Convert the following Unity C# source code to {gameEngineName} game engine code:\n\n{source}";

            return await CallChatGptAsync(source);
        }


        // Function to call OpenAI API for GPT-4/ChatGPT completions
        private static async Task<string> CallChatGptAsync(string prompt)
        {
            using var client = new HttpClient();

            // Set up the HTTP request
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);

            var requestBody = new
            {
                model = "gpt-4",  // or "gpt-3.5-turbo", depending on your subscription
                prompt = prompt,
                max_tokens = 1500,
                temperature = 0.7
            };

            // Create the HTTP request content
            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                // Send the POST request
                var response = await client.PostAsync(OpenAiEndpoint, content);

                // Ensure a successful response
                response.EnsureSuccessStatusCode();

                // Read and return the result from the response
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // The response is a JSON object, and we need to extract the 'choices' section which contains the generated text
                var responseObject = System.Text.Json.JsonSerializer.Deserialize<OpenAiResponse>(jsonResponse);
                return responseObject?.Choices?[0].Text.Trim() ?? "No response from OpenAI.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling OpenAI API: {ex.Message}");
                return "Error occurred.";
            }
        }

        // Define a class to deserialize the OpenAI API response
        private class OpenAiResponse
        {
            public Choice[] Choices { get; set; }

            public class Choice
            {
                public string Text { get; set; }
            }
        }

    }
}
