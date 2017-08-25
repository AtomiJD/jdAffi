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
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

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
        private DispatcherTimer chart_timer;
        private int overrule = 0;
        private int time_count = 0;
        private int line_count = 0;
        private int amount_count = 0;

        public class Humidity
        {
            public string Name { get; set; }
            public int Amount { get; set; }
        }

        List<Humidity> liste01 = new List<Humidity>();
        List<Humidity> liste02 = new List<Humidity>();
        List<Humidity> liste03 = new List<Humidity>();

        private void LoadChartContents(int a1, int a2, int a3)
        {
            Random rand = new Random();

            liste01.Add(new Humidity() { Name = string.Format("{0}", amount_count), Amount = a1 });
            liste02.Add(new Humidity() { Name = string.Format("{0}", amount_count), Amount = a2 });
            liste03.Add(new Humidity() { Name = string.Format("{0}", amount_count), Amount = a3 });
            if (liste01.Count > 10) liste01.RemoveAt(0);
            if (liste02.Count > 10) liste02.RemoveAt(0);
            if (liste03.Count > 10) liste03.RemoveAt(0);
            (LineChart.Series[0] as LineSeries).Refresh();
            (LineChart.Series[1] as LineSeries).Refresh();
            (LineChart.Series[2] as LineSeries).Refresh();
        }

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

            this.timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);

            this.chart_timer = new DispatcherTimer();
            chart_timer.Tick += chart_timer_Tick;
            chart_timer.Interval = new TimeSpan(0, 0, 1);

            (LineChart.Series[0] as LineSeries).ItemsSource = liste01;
            (LineChart.Series[1] as LineSeries).ItemsSource = liste02;
            (LineChart.Series[2] as LineSeries).ItemsSource = liste03;

            chart_timer.Start();

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
            RenderStatus(rstrText);
            time_count = 0;
        }
        
        private async void RenderStatus(string rstrText )
        {
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.txtDebug.Text += rstrText + "\n";
                line_count++;
                if (line_count > 40)
                {
                    this.txtDebug.Text = "";
                    line_count = 0;
                }
            });
        }

        void timer_Tick(object sender, object e)
        {
            time_count += 1;
            if (time_count > 10)
            {
                time_count = 0;
                timer.Stop();
            }
        }

        void chart_timer_Tick(object sender, object e)
        {
            amount_count++;
            string strStatus = "";
            strStatus = affC.Values;
            LoadChartContents(Int32.Parse(strStatus.Substring(2, 1)), Int32.Parse(strStatus.Substring(3, 1)), Int32.Parse(strStatus.Substring(4, 1)));
        }
    }

}
