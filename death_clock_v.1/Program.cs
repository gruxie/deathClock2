Here is the commented code for `Program.cs`:

```c#
using System;
using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace LifeExpectancyApp
{
    // Class to hold configuration strings
    public class ConfigStrings
    {
        public string[] GreetingMessages { get; set; }
        public string[] LifeExpectancyResponses { get; set; }
    }

    // Class to hold user data
    public class UserData
    {
        public string Name { get; set; }
        public string Sex { get; set; }
        public string Birthday { get; set; }
        public string BirthDayOfWeek { get; set; }
        public double RawLifeExp { get; set; }
        public DateTime LifeExpectancyDate { get; set; }
        public string LifeExpectancyDayOfWeek { get; set; }
    }

    class Program
    {
        private const string CONFIG_FILE = "strings.json";
        private const string CUSTOMER_FILE = "cust_data.json";

        static async Task Main(string[] args)
        {
            // Ensure config file exists
            if (!File.Exists(CONFIG_FILE))
            {
                CreateDefaultConfigFile();
            }

            // Load configuration strings
            var configStrings = LoadConfigStrings();

            // Welcome and get name
            Console.WriteLine("Welcome to the Life Expectancy Calculator!");
            Console.Write("Please enter your name: ");
            string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Console.ReadLine().Trim());

            // Create UserData object
            UserData userData = new UserData { Name = name };

            // Select and display a greeting message
            string greetingMessage = GetRandomMessage(configStrings.GreetingMessages).Replace("{name}", name);
            Console.WriteLine(greetingMessage);

            // Get gender
            Console.WriteLine("Please select your gender:");
            Console.WriteLine("1. Male");
            Console.WriteLine("2. Female");
            Console.WriteLine("3. Not Specified");
            
            int genderChoice;
            while (!int.TryParse(Console.ReadLine(), out genderChoice) || genderChoice < 1 || genderChoice > 3)
            {
                Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
            }

            // Set user gender
            userData.Sex = genderChoice switch
            {
                1 => "male",
                2 => "female",
                _ => "not specified"
            };

            // Get birthday
            DateTime birthday;
            while (true)
            {
                Console.Write("Enter your birthday (mm/dd/yyyy): ");
                if (DateTime.TryParseExact(Console.ReadLine(), "MM/dd/yyyy", null, DateTimeStyles.None, out birthday))
                {
                    break;
                }
                Console.WriteLine("Invalid date format. Please use mm/dd/yyyy.");
            }
            
            // Store birthday and day of the week
            userData.Birthday = birthday.ToString("MM/dd/yyyy");
            userData.BirthDayOfWeek = birthday.ToString("dddd");
            Console.WriteLine($"You were born on a {userData.BirthDayOfWeek}!");

            // Calculate life expectancy
            userData.RawLifeExp = CalculateLifeExpectancy(userData.Sex);
            userData.LifeExpectancyDate = birthday.AddYears((int)Math.Floor(userData.RawLifeExp));

            // Adjust for weekend if needed
            if (userData.LifeExpectancyDate.DayOfWeek == DayOfWeek.Saturday)
            {
                userData.LifeExpectancyDate = userData.LifeExpectancyDate.AddDays(2);
            }
            else if (userData.LifeExpectancyDate.DayOfWeek == DayOfWeek.Sunday)
            {
                userData.LifeExpectancyDate = userData.LifeExpectancyDate.AddDays(1);
            }

            // Store life expectancy day of week
            userData.LifeExpectancyDayOfWeek = userData.LifeExpectancyDate.ToString("dddd");

            // Select life expectancy response
            string lifeExpResponse = GetRandomMessage(configStrings.LifeExpectancyResponses)
                .Replace("{date}", userData.LifeExpectancyDate.ToShortDateString())
                .Replace("{dayOfWeek}", userData.LifeExpectancyDayOfWeek);
            Console.WriteLine(lifeExpResponse);

            Console.WriteLine($"Your life is bookended by a {userData.BirthDayOfWeek} and a {userData.LifeExpectancyDayOfWeek}!");

            // Save user data
            SaveUserData(userData);

            // Start countdown
            await StartCountdownAsync(userData.LifeExpectancyDate);
        }

        // Starts the countdown to life expectancy date
        static async Task StartCountdownAsync(DateTime endDate)
        {
            Console.WriteLine("\nStarting Countdown to Life Expectancy...");
            Console.WriteLine("Press any key to stop the countdown.");

            // Use CancellationTokenSource to allow cancellation
            using (var cts = new CancellationTokenSource())
            {
                // Start a task to handle key press
                var keyTask = Task.Run(() =>
                {
                    Console.ReadKey(true);
                    cts.Cancel();
                });

                // Countdown loop
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        // Calculate remaining time
                        TimeSpan remainingTime = endDate - DateTime.Now;

                        if (remainingTime.TotalSeconds <= 0)
                        {
                            Console.WriteLine("\nLife expectancy date reached!");
                            break;
                        }

                        // Clear the current line
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);

                        // Display countdown
                        Console.Write($"Time Remaining: {FormatTimeSpan(remainingTime)}");

                        // Wait for a second
                        await Task.Delay(1000, cts.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("\nCountdown stopped by user.");
                }
            }
        }

        // Formats TimeSpan to a readable string
        static string FormatTimeSpan(TimeSpan timeSpan)
        {
            return $"{timeSpan.Days} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds";
        }

        // Calculates life expectancy based on gender
        static double CalculateLifeExpectancy(string sex)
        {
            return sex.ToLower() == "female" ? 80.2 : 74.8;
        }

        // Loads configuration strings from file
        static ConfigStrings LoadConfigStrings()
        {
            string jsonString = File.ReadAllText(CONFIG_FILE);
            return JsonSerializer.Deserialize<ConfigStrings>(jsonString);
        }

        // Creates default configuration file if it doesn't exist
        static void CreateDefaultConfigFile()
        {
            var defaultConfig = new ConfigStrings
            {
                GreetingMessages = new[] {
                    "Hey {name}, welcome to your life journey!",
                    "Nice to meet you, {name}! Let's explore your potential lifespan.",
                    "Greetings, {name}! Ready to peek into your future?"
                },
                LifeExpectancyResponses = new[] {
                    "This is your incept date. LOL! {date} ({dayOfWeek})",
                    "Your journey ends on {date}, a {dayOfWeek}. Buckle up!",
                    "Mark your calendar: {date} ({dayOfWeek}) is the big day!"
                }
            };

            string jsonString = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CONFIG_FILE, jsonString);
        }

        // Returns a random message from an array of messages
        static string GetRandomMessage(string[] messages)
        {
            Random random = new Random();
            return messages[random.Next(messages.Length)];
        }

        // Saves user data to a file
        static void SaveUserData(UserData userData)
        {
            string jsonString = JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CUSTOMER_FILE, jsonString);
        }
    }
}
```
