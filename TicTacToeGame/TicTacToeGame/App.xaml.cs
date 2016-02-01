using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TicTacToeGame
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session);
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }

#endif
           

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            try
            {
                System.Diagnostics.Debug.WriteLine("Loading voice commands");
                //get the file and register
                StorageFile vcdFile = await Package.Current.InstalledLocation.GetFileAsync(@"voiceCommands.xml");
                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.
                    InstallCommandDefinitionsFromStorageFileAsync(vcdFile);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("VCD didn't load: " + ex);
                throw;
            }
        }
        //add the onActivated event handler (override)
        protected async override void OnActivated(IActivatedEventArgs args)
        {

            base.OnActivated(args);
            if (args.Kind == ActivationKind.VoiceCommand)
                return;
            //now get the voice parameters
            VoiceCommandActivatedEventArgs commandArgs = args as VoiceCommandActivatedEventArgs;
            SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

            //use this for debugging
            MessageDialog msgDialog = new MessageDialog("");

            string voiceCommandName = speechRecognitionResult.RulePath[0];
            string textSpoken = speechRecognitionResult.Text;
            IReadOnlyList<string> recognisedVoiceCmdPhrases; //phrases Im looking for

            msgDialog.Content = ("Parameters so far: " + System.Environment.NewLine + voiceCommandName +
                System.Environment.NewLine + textSpoken);

            await msgDialog.ShowAsync();
            //decide which command - newGame or makeMove
            switch (voiceCommandName)
            {
                case "newGame":
                    msgDialog.Content = "New Game command";
                    break;

                case "makeMove":
                    msgDialog.Content = "Make a move command";
                    String xValue = "XDefault", yValue = "YDefault";

                    if (speechRecognitionResult.SemanticInterpretation.Properties.
                                            TryGetValue("xPositon", out recognisedVoiceCmdPhrases)) {

                        xValue = recognisedVoiceCmdPhrases.First();
                    }
                     //y value
                    if (speechRecognitionResult.SemanticInterpretation.Properties.
                                            TryGetValue("yPositon", out recognisedVoiceCmdPhrases))
                    {

                        yValue = recognisedVoiceCmdPhrases.First();
                    }
                    msgDialog.Content = "Move to [" + xValue + "], [" + yValue + "]";

                    break;
                default :
                    msgDialog.Content = "unknown command.";

                    break;
            }
        }


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
