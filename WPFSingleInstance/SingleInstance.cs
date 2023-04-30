//-----------------------------------------------------------------------
// <copyright file="SingleInstance.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     This class checks to make sure that only one instance of 
//     this application is running at a time.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using TinyIpc.Messaging;

namespace WPFSingleInstance
{
    /// <summary>
    /// This class checks to make sure that only one instance of 
    /// this application is running at a time.
    /// </summary>
    /// <remarks>
    /// Note: this class should be used with some caution, because it does no
    /// security checking. For example, if one instance of an app that uses this class
    /// is running as Administrator, any other instance, even if it is not
    /// running as Administrator, can activate it with command line arguments.
    /// For most apps, this will not be much of an issue.
    /// </remarks>
    public static class SingleInstance<TApplication> where TApplication : Application, ISingleInstance
    {
        private const string channelNameSufflix = ":SingeInstanceIPCChannel";
        //For detecting if mutex is locked (first instance is already up then)
        private static Mutex singleMutex;
        //Message bus for communication between instances
        private static TinyMessageBus messageBus;

        /// <summary>
        /// Intended to be on app startup
        /// Initializes service if the call is from first instance.
        /// Signals the first instance if it already exists
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="uniqueName">A unique name for IPC channel</param>
        /// <returns>Whether the call is from application's first instance</returns>
        public static bool InitializeAsFirstInstance(string uniqueName)
        {
            var commandLineArgs = GetCommandLineArgs(uniqueName);
            var applicationIdentifier = uniqueName + Environment.UserName;
            var channelName = $"{applicationIdentifier}{channelNameSufflix}";
            singleMutex = new Mutex(true, applicationIdentifier, out var firstInstance);

            if (firstInstance)
                CreateRemoteService(channelName);
            else
                SignalFirstInstance(channelName, commandLineArgs);

            return firstInstance;
        }

        private static void SignalFirstInstance(string channelName, IList<string> commandLineArgs)
        {
            var bus = GetTinyMessageBus(channelName);
            var serializedArgs = commandLineArgs.Serialize();
            bus?.PublishAsync(serializedArgs).Wait();
            WaitTillMessageGetsPublished(bus);
        }

        private static TinyMessageBus GetTinyMessageBus(string channelName, int tryCount = 50)
        {
            var tries = 0;
            var minMessageAge = TimeSpan.FromSeconds(30);
            while (tries++ < tryCount)
            {
                try
                {
                    var bus = new TinyMessageBus(channelName, minMessageAge);
                    return bus;
                }
                catch (Exception) { }
            }
            return default;
        }

        private static void WaitTillMessageGetsPublished(ITinyMessageBus bus)
        {
            if (bus == null)
                return;

            while (bus.MessagesPublished != 1)
            {
                Thread.Sleep(10);
            }
        }

        private static void CreateRemoteService(string channelName)
        {
            messageBus = new TinyMessageBus(channelName);
            messageBus.MessageReceived += (_, e) =>
            {
                (Application.Current as TApplication)?.SignalExternalCommandLineArgs(e.Message.ToArray().Deserialize<string[]>());
            };
        }

        private static string[] GetCommandLineArgs(string uniqueApplicationName)
        {
            var args = Environment.GetCommandLineArgs();
            if (args == null)
            {
                // Try getting commandline arguments from shared location in case of ClickOnce deployed application  
                var appFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);
                var cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if (File.Exists(cmdLinePath))
                {
                    try
                    {
                        using var reader = new StreamReader(cmdLinePath, Encoding.Unicode);
                        args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
                        File.Delete(cmdLinePath);
                    }
                    catch (IOException) { }
                }
            }
            return args ?? Array.Empty<string>();
        }

        public static void Cleanup()
        {
            if (messageBus != null)
            {
                messageBus.Dispose();
                messageBus = null;
            }
            if (singleMutex != null)
            {
                singleMutex.Close();
                singleMutex = null;
            }
        }
    }
}
