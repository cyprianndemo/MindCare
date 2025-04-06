using MindCare.Data;
using MindCare.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MindCare.Services
{
    public class ChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaudeAiService _claudeAiService;

        // Dictionary to track the state of each user
        private readonly Dictionary<string, bool> _isTrackingMood;

        public ChatbotService(ApplicationDbContext context, ClaudeAiService claudeAiService)
        {
            _context = context;
            _claudeAiService = claudeAiService;
            _isTrackingMood = new Dictionary<string, bool>();
        }

        public async Task<string> ProcessMessage(string message, string userId)
        {
            // Check if the user is currently tracking mood
            if (_isTrackingMood.TryGetValue(userId, out bool isTracking) && isTracking)
            {
                // Try to parse the message as an integer
                if (int.TryParse(message, out int score) && score >= 1 && score <= 5)
                {
                    _isTrackingMood[userId] = false; // Reset the tracking state
                    return await ProcessMoodResponse(score, userId);
                }
            }

            // Check for mood tracking command directly 
            if (message.ToLower().Contains("track mood") || message.ToLower().Contains("mood tracking"))
            {
                _isTrackingMood[userId] = true; // Set the tracking state
                return "Let's track your mood today. On a scale of 1-5 (1 being lowest, 5 being highest), how would you rate your mood?";
            }

            // For all other messages, pass to Claude AI
            var response = await _claudeAiService.GetResponseAsync(message, userId);
            return response;
        }

        public async Task<string> ProcessMoodResponse(int score, string userId)
        {
            var moodEntry = new MoodEntry
            {
                UserId = userId,
                MoodScore = score,
                EntryDate = DateTime.UtcNow
            };

            _context.MoodEntries.Add(moodEntry);
            await _context.SaveChangesAsync();

            // Get personalized response from Claude about the mood score
            var moodMessage = $"The user has just shared their mood score of {score} out of 5 (where 1 is lowest and 5 is highest). Please acknowledge their mood and provide appropriate suggestions or encouragement. Keep your response supportive and concise.";

            var response = await _claudeAiService.GetResponseAsync(moodMessage, userId);
            return response;
        }
    }
}