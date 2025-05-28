using System.Net;
using System.Text;
using Amazon.Lambda.Core;
using LambaExamenMDC.Models;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambaExamenMDC;

public class Function
{
    const string API_KEY = "5tMpg9KcwHw57Kx9qUUNILITRJ1WK3yFQuOw3Cd9HQ8VY6vYC7oPJQQJ99BEACHYHv6XJ3w3AAAAACOGOZVY";
    const string ENDPOINT = "https://paco-mb6srmq2-eastus2.cognitiveservices.azure.com/openai/deployments/gpt-4.1/chat/completions?api-version=2025-01-01-preview";


    public async Task<Question> FunctionHandler(Question input, ILambdaContext context)
    {
        Question data = new Question
        {
            Ques = await AskQuestion(input.Ques)
        };
        return data;
    }

    static async Task<string> AskQuestion(string question)
    {
        var payload = new
        {
            messages = new object[]
            {
                new {
                    role = "system",
                    content = new object[] {
                        new {
                            type = "text",
                            text = "You are an AI assistant that helps people find information."
                        }
                    }
                },
                new {
                    role = "user",
                    content = new object[] {
                        new {
                            type = "text",
                            text = question
                        }
                    }
                }
            },
            temperature = 0.7,
            top_p = 0.95,
            max_tokens = 800,
            stream = false
        };
        return await SendRequest(payload);
    }

    static async Task<string> SendRequest(object payload)
    {
        using (var httpClient = new HttpClient())
        {
            httpClient.DefaultRequestHeaders.Add("api-key", API_KEY);

            var response = await httpClient.PostAsync(ENDPOINT, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                var responseData = jsonObject?.choices?[0]?.message?.content;
                var completionTokens = jsonObject?.usage.completion_tokens;
                var prompt_tokens = jsonObject?.usage.prompt_tokens;
                var total_tokens = jsonObject?.usage.total_tokens;

                if (responseData != null)
                {
                    return responseData;
                    
                }
                else
                {
                    return  "Response data is null.";
                }
            }
            else
            {
                return $"Error: {response.StatusCode}, {response.ReasonPhrase}";
            }
        }
    }
}
