using MindCare.Data;
using MindCare.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindCare.Services
{
    public class ChatbotService
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<string, List<string>> _moodQuestions;
        private readonly Dictionary<string, string> _mentalHealthResources;

        // Dictionary to track the state of each user
        private readonly Dictionary<string, bool> _isTrackingMood;

        public ChatbotService(ApplicationDbContext context)
        {
            _context = context;
            _moodQuestions = new Dictionary<string, List<string>>
            {
                { "initial", new List<string> { "How are you feeling today? (1-5)", "Have you experienced any significant stress today? (Yes/No)", "How well did you sleep last night? (1-5)" } }
            };

            _mentalHealthResources = new Dictionary<string, string>
            {
                { "depression", "For depression, consider visiting [NIMH](https://www.nimh.nih.gov/health/statistics), where you can find statistics, treatment options, and support groups." },
                { "anxiety", "For anxiety, check out [Anxiety and Depression Association of America](https://adaa.org/), which offers resources and coping strategies." },
                { "stress", "For stress management, visit [American Psychological Association](https://www.apa.org/topics/stress), which provides various tips and resources." },
                { "therapy", "Therapy can be a great resource. Visit [Psychology Today](https://www.psychologytoday.com/us/therapists) to find a therapist near you." },
                { "self-care", "Self-care is crucial. Here are some tips: exercise regularly, eat healthily, and engage in hobbies you enjoy." }
            };

            _isTrackingMood = new Dictionary<string, bool>();
        }

        public async Task<string> ProcessMessage(string message, string userId)
        {
            message = message.ToLower();

            if (message.Contains("help") || message.Contains("guide"))
            {
                return GetNavigationGuide();
            }

            if (message.Contains("track mood") || message.Contains("mood tracking"))
            {
                _isTrackingMood[userId] = true; // Set the tracking state
                return StartMoodTracking();
            }

            // Check if the user is currently tracking mood
            if (_isTrackingMood.TryGetValue(userId, out bool isTracking) && isTracking)
            {
                // Try to parse the message as an integer
                if (int.TryParse(message, out int score) && score >= 1 && score <= 5)
                {
                    _isTrackingMood[userId] = false; // Reset the tracking state
                    return await ProcessMoodResponse(score, userId);
                }
                else
                {
                    return "Please enter a valid mood score (1-5).";
                }
            }

            if (message.Contains("speak to professional") || message.Contains("talk to admin"))
            {
                return InitiateLiveChat();
            }

            // Check for specific mental health topics
            foreach (var resource in _mentalHealthResources)
            {
                if (message.Contains(resource.Key))
                {
                    return resource.Value;
                }
            }

            if (message.Contains("relax") || message.Contains("relaxation"))
            {
                return GetRelaxationTechniques();
            }

            if (message.Contains("chat") || message.Contains("talk"))
            {
                return GetInteractiveResponses();
            }

            return "I can help you with:\n1. System navigation (type 'help')\n2. Mood tracking (type 'track mood')\n3. Speaking with a professional (type 'speak to professional')\n4. Mental health resources (type a topic such as 'depression', 'anxiety', 'stress', or 'self-care')\n5. Relaxation techniques (type 'relax')\n6. Chat or talk (type 'chat' or 'talk').";
        }

        private string GetNavigationGuide()
        {
            return @"Here's how to navigate our system:
1. Dashboard: View your mood history and analytics
2. Appointments: Schedule meetings with health professionals
3. Resources: Access mental health resources
4. Profile: Update your personal information
What would you like to know more about?";
        }

        private string StartMoodTracking()
        {
            return "Let's track your mood today. On a scale of 1-5 (1 being lowest, 5 being highest), how would you rate your mood?";
        }

        public async Task<string> ProcessMoodResponse(int score, string userId)
        {
            var moodEntry = new MoodEntry
            {
                UserId = userId,
                /*MoodScore = score,
                Date = DateTime.UtcNow*/
            };

            _context.MoodEntries.Add(moodEntry);
            await _context.SaveChangesAsync();

            // Provide feedback based on the user's mood score
            if (score < 5)
            {
                var suggestions = GetMoodImprovementSuggestions(score);
                return $"Thank you for sharing your mood rating of {score}.\n" +
                       "It's important to acknowledge our feelings. Here are some suggestions to improve your mood:\n" +
                       suggestions +
                       "\nWhat specific things are making your mood low? (Please share any specific issues or situations.)";
            }
            else
            {
                return $"Great to hear you're feeling good with a rating of {score}! Keep up the positive mindset!";
            }
        }

        private string GetMoodImprovementSuggestions(int score)
        {
            // Suggest strategies based on mood score
            if (score == 1)
            {
                return "Consider reaching out to a friend or family member for support. Engage in a comforting activity like watching a favorite movie or going for a walk.";
            }
            else if (score == 2)
            {
                return "Try some light exercise, practice deep breathing, or meditate for a few minutes to help elevate your mood.";
            }
            else if (score == 3)
            {
                return "Listen to your favorite music, or consider talking to someone about what's on your mind.";
            }
            else if (score == 4)
            {
                return "Keep engaging in activities you enjoy! Consider trying something new to boost your mood even more.";
            }
            return "It's great that you're feeling good! Maintain your positive habits.";
        }

        private string InitiateLiveChat()
        {
            return "I'll connect you with a health professional. Please wait while I check for available professionals...";
        }

        private string GetRelaxationTechniques()
        {
            return "Here are some relaxation techniques you can try:\n" +
                   "1. Deep Breathing: Inhale slowly through your nose for a count of four, hold for a count of four, and exhale through your mouth for a count of four. Repeat several times.\n" +
                   "2. Progressive Muscle Relaxation: Tense each muscle group in your body for a few seconds, then relax. Start from your toes and work your way up to your head.\n" +
                   "3. Visualization: Close your eyes and imagine yourself in a peaceful place, such as a beach or a forest. Focus on the sights, sounds, and smells of your surroundings.\n" +
                   "4. Mindfulness Meditation: Sit quietly and focus on your breath. Notice the sensation of each inhale and exhale. If your mind wanders, gently bring your attention back to your breath.";
        }

        private string GetInteractiveResponses()
        {
            return "Sure, let's chat! Tell me more about your day. How are you feeling?";
        }
    }
}
