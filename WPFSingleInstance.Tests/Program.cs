using System;

namespace WPFSingleInstance.Tests
{
    public class Program
    {
        /// <summary>
        /// The unique guid of this application
        /// Required to identify the current application
        /// </summary>
        private const string MyAppUniqueGuid = "75F9C9D7-00B6-4675-8286-0BDA7C2792C8";

        /// <summary>
        /// The entry point of this App
        /// </summary>
        [STAThread]
        public static void Main()
        {
            // Check if the current instance is the first
            if (!SingleInstance<App>.InitializeAsFirstInstance(MyAppUniqueGuid)) return;
            // Load the app if the instance is the first
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
