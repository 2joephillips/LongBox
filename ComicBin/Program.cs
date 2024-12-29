internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {

        // Start the WPF application on a dedicated STA thread
        Thread uiThread = new Thread(() =>
        {
            //Initialize the WPF App from UIDesktopApp1
            var wpfApp = new ComicBin.Desktop.App();
            wpfApp.InitializeComponent(); // Load App.xaml
            wpfApp.Run(); // Start the WPF app
        })
        {
            IsBackground = false, // Ensure the UI thread keeps running
            Name = "WPF_UI_Thread"
        };

        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();

        // Separate high-priority thread for background tasks
        Thread highPriorityThread = new Thread(PerformHighPriorityTasks)
        {
            IsBackground = true, // Allows the process to exit when the main thread exits
            Priority = ThreadPriority.Highest
        };

        highPriorityThread.Start();
    }

    private static void PerformHighPriorityTasks(object? obj)
    {
        while (true)
        {
            Console.WriteLine("Performing high-priority tasks...");
            Thread.Sleep(500); // Simulate work
        }
    }
}