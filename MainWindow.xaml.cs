using AutoMouseClicker.Help;

using Microsoft.Win32;

using Newtonsoft.Json;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace AutoMouseClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!VerifyUserInput())
            {
                return;
            }
            
            Cycles = 0;
            isIterating = true;
            int delay = int.Parse(DelayTextBox.Text);
            int repeat = int.Parse(RepeatTextBox.Text);
            bool useInBetween = (bool)UseInBetweenCheckBox.IsChecked;
            int inBetweenDelay = int.Parse(InBetweenDelayTextBox.Text);
            int executeInBetweenEvery = int.Parse(ExecuteInBetweenEveryTextBox.Text);

            await Task.Run(() => IterateMouseEvent(delay, repeat, useInBetween, inBetweenDelay, executeInBetweenEvery));

            _ = Activate();
            if (isIterating) NotifySuccess("Mouse iteration is done");
            isIterating = false;
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            MainIterationCoordinates.Clear();
            InBetweenIterationCoordinates.Clear();
        }

        private void SavePosBtn_Click(object sender, RoutedEventArgs e)
        {

            SaveData saveData = new(MainIterationCoordinates, InBetweenIterationCoordinates);
            string jsonString = JsonConvert.SerializeObject(saveData);

            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Text Files(*.txt)|*.txt"
            };
            if ((bool)saveFileDialog.ShowDialog() && saveFileDialog.FileName != string.Empty)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, jsonString);
                    NotifySuccess("Success in saving file");
                }
                catch (Exception a)
                {
                    NotifyError("Error in saving mouse position file. Make sure that you input the file name and you have right to write into that folder", "Warning");
                    Debug.WriteLine(a.Message);
                }
            }
        }

        private void LoadPosBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            if ((bool)openFileDialog.ShowDialog())
            {
                try
                {
                    using StreamReader streamReader = new(openFileDialog.FileName);
                    //StreamReader streamReader = new(openFileDialog.FileName);
                    SaveData saveData = JsonConvert.DeserializeObject<SaveData>(streamReader.ReadToEnd());
                    MainIterationCoordinates.Clear();
                    foreach(Point pos in saveData.MainIterationCoordinates)
                    {
                        MainIterationCoordinates.Add(pos);
                    }

                    InBetweenIterationCoordinates.Clear();
                    foreach (Point pos in saveData.InBetweenIterationCoordinates)
                    {
                        InBetweenIterationCoordinates.Add(pos);
                    }
                    //streamReader.Close();
                    NotifySuccess("Success in loading file");
                    return;
                }
                catch (Exception a)
                {
                    Debug.WriteLine("User might be using old save file");
                    Debug.WriteLine(a.Message);
                }

                try
                {
                    // check if user still use the old file
                    using StreamReader streamReader = new(openFileDialog.FileName);
                    ObservableCollection<Point> newMousePos = JsonConvert.DeserializeObject<ObservableCollection<Point>>(streamReader.ReadToEnd());
                    MainIterationCoordinates.Clear();
                    foreach (Point pos in newMousePos)
                    {
                        MainIterationCoordinates.Add(pos);
                    }
                    NotifySuccess("You are using the old save file but don't worry we just succesfully imported it. Save it again to convert it into new savedata format");
                }
                catch (Exception a)
                {
                    NotifyError("Error in loading mouse position file. Are you sure this is the right file?", "Warning");
                    Debug.WriteLine(a.Message);
                }

            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.AddKeyDownHandler(this, new KeyEventHandler(OnKeyboardInputReceived));
            SetupGuideTextBlock();
            keyboardListener = new LowLevelKeyboardListener();
            keyboardListener.OnKeyPressed += OnKeyboardInputReceived;
            keyboardListener.HookKeyboard(this);
            //EventManager.RegisterClassHandler(typeof(Window), Window.PreviewKeyDownEvent, new KeyEventHandler(Test));
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            keyboardListener.UnHookKeyboard();
        }

        private void MenuItem_CloseProgram(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to quit?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void MenuItem_Documentation(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("We are hosting our documentation in SourceForge.net. Do you want to visit the documentation?", "Confirmation", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                string url = "https://sourceforge.net/p/automouseclicker/wiki/Home/";
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
            }
        }

        private void MenuItem_HowToUse(object sender, RoutedEventArgs e)
        {
            HowToUse howToUseWindow = new HowToUse();
            howToUseWindow.Show();
        }
    }
}