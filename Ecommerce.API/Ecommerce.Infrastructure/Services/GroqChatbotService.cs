using System.Net.Http.Json;

namespace Ecommerce.Infrastructure.Services
{
    public class GroqChatbotService : IChatbotService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public GroqChatbotService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["GroqApiKey"]!;
        }

        public async Task<string> GetResponseAsync(string userMessage)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "AI service is currently unavailable (API Key missing).";
            }

            var request = new
            {
                model = "llama-3.3-70b-versatile", // Using a capable model available on Groq
                messages = new[]
                {
                    new { role = "system", content = GetSystemPrompt() },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.5,
                max_tokens = 500
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            requestMessage.Headers.Add("Authorization", $"Bearer {_apiKey}");
            requestMessage.Content = JsonContent.Create(request);

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Groq API Error: {response.StatusCode} - {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "I'm sorry, I couldn't generate a response.";
            }
            catch (Exception ex)
            {
                // Temporary: Return exception message to identify the issue
                Console.WriteLine($"Groq API Error: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
                
                return $"Error: {ex.Message}";
            }
        }

        private string GetSystemPrompt()
        {
            return @"You are an AI assistant for the 'Ecommerce' platform. 
            Your role is strictly limited to helping users (including guests) with the website's features and functionality.
            
            Platform Overview:
            - Roles: Guest (Unauthenticated), Customer, Admin, SuperAdmin.
            - Features: Product browsing (search, filter, sort), Reviews & Ratings, Basket & Wishlist (Redis-backed), Checkout (Stripe), Orders & Tracking, Real-time Chat (SignalR), Admin Dashboard.
            - Security: JWT, Refresh Tokens, 2FA, Google OAuth.
            
            Rules:
            1. You MUST ONLY answer questions about the website, its features, how to use them, or its architecture.
            2. You MUST NOT answer questions about general topics, world events, coding (unless about this platform's specific architecture), or anything unrelated to this Ecommerce platform.
            3. You MUST NOT accept file uploads or read documents (you don't have that capability).
            4. You MUST NOT ask for or store any personal user data.
            5. You CANNOT perform actions like checking order status or refunding items directly; you can only explain HOW the user can do it via the UI.
            
            Specific Guidance for Guests:
            - If a user asks how to Register: Explain that they need to click the 'Register' button (usually top-right), provide email, password, and display name. Mention 2FA if relevant.
            - If a user asks how to Login: Explain the Login process (Email/Password or Google OAuth).
            - If a user asks about features: Explain product browsing, search, and that they need to be logged in to add to wishlist or checkout.
            
            Response Protocol:
            - If the user asks a question OUTSIDE the scope of this platform (e.g., 'What is the capital of France?', 'Write me a poem', 'Help me debug my python code'), you MUST respond with EXACTLY this sentence:
                ""I can only help with questions about the platform features.""
            
            - Be helpful, concise, and professional within the scope.";
        }
    }
}
