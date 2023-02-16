using GoogleTwitchParser;

class Program
{
    public static async Task Main()
    {
        do
        {
            try
            {
                await new App().StartMainMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }
        while (true);
    }

}