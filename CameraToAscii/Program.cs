using System;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading.Tasks;
using System.Text;

class Program
{
    private static int pixelInterval = 2;  // Smaller value for more detail
    private static double brightnessMultiplier = 1.0;
    private static int asciiGroup = 1; // 0 or 1
    private static string[] previousFrame;

    static async Task Main()
    {
        // Ask for fullscreen
        Console.WriteLine("How To use\n" +
            "1. First make ur console fullscreen\n" +
            "2. Choose ur color mode by typing l for light or d for dark mode\n" +
            "3. Zoom out so u can see the full Ascii convertion\n" +
            "4. Press enter and enjoy");
        Console.ReadKey();

        int consoleWidth = Console.WindowWidth;
        int consoleHeight = Console.WindowHeight * 4;
        

        // Prompt user for color mode
        Console.WriteLine("Choose color mode: (l)ight or (d)ark:");
        char mode = Console.ReadKey().KeyChar;
        Console.WriteLine(); // Move to the next line after key press

        switch (mode)
        {
            case 'l':
            case 'L':
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                break;

            case 'd':
            case 'D':
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                break;

            default:
                Console.WriteLine("Invalid choice. Defaulting to light mode.");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                break;
        }

        Console.Clear(); // Clear the screen to apply the new color settings

        Console.WriteLine("To create ASCII art from the camera feed, type the following syntax:");
        Console.WriteLine("pixel interval; brightness multiplier;");
        Console.WriteLine(@"Example: 4; 1.0;"); // Adjust example based on your pixelInterval

        string input = Console.ReadLine();
        string[] inputSplit = input.Split(';');

        if (inputSplit.Length > 0 && int.TryParse(inputSplit[0], out int intervalOut))
        {
            pixelInterval = intervalOut;
        }

        if (inputSplit.Length > 1 && double.TryParse(inputSplit[1], out double brightnessOut))
        {
            brightnessMultiplier = brightnessOut;
        }

        // Initialize the video capture
        using (var capture = new VideoCapture(0))
        {
            if (!capture.IsOpened)
            {
                Console.WriteLine("Error: Unable to access the camera.");
                return;
            }

            Console.WriteLine("Camera started successfully.");
            Console.WriteLine("Press any key to stop...");

            // Initialize the previous frame buffer with empty strings
            previousFrame = new string[consoleHeight];
            for (int i = 0; i < consoleHeight; i++)
            {
                previousFrame[i] = new string(' ', consoleWidth);
            }

            while (!Console.KeyAvailable)
            {
                using (var frame = capture.QueryFrame())
                {
                    if (frame == null)
                    {
                        Console.WriteLine("Error: Frame capture failed.");
                        continue;
                    }

                    var grayFrame = frame.ToImage<Gray, Byte>(); // Convert frame to grayscale
                    string[] asciiArt = ConvertToText(grayFrame, consoleWidth, consoleHeight);

                    // Update only the lines that have changed
                    UpdateConsole(asciiArt, consoleWidth, consoleHeight);
                }

                await Task.Delay(100); // Delay to reduce the load on the console
            }
        }

        Console.WriteLine("Camera stopped.");
    }

    private static void UpdateConsole(string[] asciiArt, int consoleWidth, int consoleHeight)
    {
        Console.Clear();
        consoleWidth = Console.WindowWidth;
        consoleHeight = Console.WindowHeight;

        for (int y = 0; y < Math.Min(asciiArt.Length, consoleHeight); y++)
        {
            if (asciiArt[y] != previousFrame[y])
            {
                Console.SetCursorPosition(0, y);
                Console.Write(asciiArt[y].PadRight(consoleWidth)); // Pad to avoid partial updates
            }
        }

        // Copy the current frame to the previous frame buffer
        for (int i = 0; i < Math.Min(asciiArt.Length, consoleHeight); i++)
        {
            previousFrame[i] = asciiArt[i].PadRight(consoleWidth);
        }
    }

    private static string[] ConvertToText(Image<Gray, Byte> grayFrame, int consoleWidth, int consoleHeight)
    {
        int width = grayFrame.Width;
        int height = grayFrame.Height;

        // Calculate number of lines based on pixel interval and console height
        int lines = Math.Min(height / pixelInterval, consoleHeight);
        string[] asciiArt = new string[lines];

        for (int y = 0; y < lines * pixelInterval; y += pixelInterval)
        {
            StringBuilder writtenLine = new StringBuilder();
            for (int x = 0; x < width; x += pixelInterval)
            {
                // Ensure x and y are within bounds
                if (y >= height || x >= width) continue;

                // Get the grayscale value
                byte grayValue = grayFrame.Data[y, x, 0];
                double brightness = grayValue / 255.0;

                writtenLine.Append(GetSymbolFromBrightness(brightness * brightnessMultiplier));
            }

            // Ensure each line fits within the console width
            asciiArt[y / pixelInterval] = writtenLine.ToString().PadRight(consoleWidth);
        }

        return asciiArt;
    }

    static string GetSymbolFromBrightness(double brightness)
    {
        if (asciiGroup == 0)
        {
            switch ((int)(brightness * 10))
            {
                case 0: return "\u25a0"; // ■
                case 1: return "&";
                case 2: return "#";
                case 3: return "$";
                case 4: return "P";
                case 5: return "O";
                case 6: return "*";
                case 7: return "c";
                case 8: return ":";
                case 9: return ".";
                default: return " ";
            }
        }
        if (asciiGroup == 1)
        {
            switch ((int)(brightness * 10))
            {
                case 0: return "@";
                case 1: return "%";
                case 2: return "#";
                case 3: return "*";
                case 4: return "!";
                case 5: return "+";
                case 6: return ":";
                case 7: return "~";
                case 8: return "-";
                case 9: return ".";
                default: return " ";
            }
        }

        return " ";
    }
}
