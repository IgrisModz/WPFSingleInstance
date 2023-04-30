using System;

namespace WPFSingleInstance.Tests
{
    public class Program
    {
        private const string MyAppUniqueGuid = "75F9C9D7-00B6-4675-8286-0BDA7C2792C8";

        [STAThread]
        public static void Main()
        {
            if (!SingleInstance<App>.InitializeAsFirstInstance(MyAppUniqueGuid)) return;
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
