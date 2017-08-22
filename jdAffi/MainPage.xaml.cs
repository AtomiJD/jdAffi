using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media.SpeechSynthesis;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.UI.Core;

namespace jdAffi
{
    public sealed partial class MainPage : Page
    {
        MediaElement mediaElement;
        private SpeechSynthesizer synth;
        private SpeechRecognizer spcLissener;
        private CoreDispatcher dispatcher;
        private jdAffiController affC;
        private DispatcherTimer timer;
        private int overrule = 0;
        private int time_count = 0;

        public MainPage()
        {
            this.InitializeComponent();

            this.dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            InitializeSpeechRecognizer();

            mediaElement = this.media;
            synth = new Windows.Media.SpeechSynthesis.SpeechSynthesizer();
            foreach (var voice in SpeechSynthesizer.AllVoices)
            {
                RenderStatus(voice.DisplayName);
            }
            using (var speaker = new SpeechSynthesizer())
            {
                speaker.Voice = (SpeechSynthesizer.AllVoices.First(x => x.Gender == VoiceGender.Female));
                synth.Voice = speaker.Voice;
            }

            affC = new jdAffiController();

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private async void InitializeSpeechRecognizer()
        {
            this.spcLissener = new SpeechRecognizer();
            spcLissener.StateChanged += spcLissener_StateChanged;
            spcLissener.ContinuousRecognitionSession.ResultGenerated += spcLissener_ResultGenerated;

            StorageFile GrammarContentFile = await Package.Current.InstalledLocation.GetFileAsync(@"jdAffiGrammar.xml");

            SpeechRecognitionGrammarFileConstraint GrammarConstraint = new SpeechRecognitionGrammarFileConstraint(GrammarContentFile);
            spcLissener.Constraints.Add(GrammarConstraint);

            SpeechRecognitionCompilationResult CompilationResult = await spcLissener.CompileConstraintsAsync();

            RenderStatus("Sprach Status: " + CompilationResult.Status.ToString());

            if (CompilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await spcLissener.ContinuousRecognitionSession.StartAsync();
            }
        }

        private void spcLissener_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string strStatus = "";
            RenderStatus(args.Result.Text);
            strStatus = affC.Values;
            switch (args.Result.Text)
            {
                case "hello taffy":
                    Say("Hello master");
                    timer.Start();
                    break;
            }
            if (time_count > 0)
                switch (args.Result.Text)
                { 
                    case "begin watering":
                        if (strStatus.Substring(2,1) != "3" && overrule == 0)
                        {
                            Say("My can is empty, pleas fill my can!");
                        }
                        else { 
                            Say("I'm starting watering your plant!");
                            affC.StartWatering();
                        }
                        overrule = 0;
                        break;
                    case "stop watering":
                    case "end watering":
                        Say("Stopping. Was watering OK?");
                        affC.StopWatering();
                        overrule = 0;
                        break;
                    case "fill can":
                        Say("I'm filling your can, please wait!");
                        affC.FillCan();
                        break;
                    case "ficksau":
                    case "asshole":
                        Say("Eat my shorts!");
                        break;
                    case "overrule":
                        Say("Master, you activated my over rule mode, be careful!");
                        overrule = 1;
                        break;
                    default:
                        Say("speak clearly!");
                        break;
                }
        }

        private void spcLissener_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            RenderStatus(args.State.ToString());
        }


        public async System.Threading.Tasks.Task playTextAsync(string strText)
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                SpeechSynthesisStream stream = await synth.SynthesizeTextToStreamAsync(strText);
                mediaElement.SetSource(stream, stream.ContentType);
                mediaElement.Play();
            });
        }

        private void btnTalk_Click(object sender, RoutedEventArgs e)
        {
            Say(this.txtTalkToMe.Text);
        }

        private async void Say(string rstrText)
        {
            await playTextAsync(rstrText);
        }

        private async void RenderStatus(string rstrText )
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.txtDebug.Text += rstrText + "\n";
            });
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //jdtodo
            time_count += 1;
            if (time_count > 10)
            {
                time_count = 0;
                timer.Stop();
            }
        }
    }

}
