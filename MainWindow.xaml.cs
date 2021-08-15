// MIT License (MIT)
//
// Copyright (c) 2021 Deta Novian Anantika Putra (detanaputra)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the “Software”), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using AutoMouseClicker.Help;

using Microsoft.Win32;

using Newtonsoft.Json;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
            WindowState = WindowState.Minimized;

            await Task.Run(() => IterateMouseEvent(delay, repeat, useInBetween, inBetweenDelay, executeInBetweenEvery));

            WindowState = WindowState.Normal;
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
                    SaveData saveData = JsonConvert.DeserializeObject<SaveData>(streamReader.ReadToEnd());
                    MainIterationCoordinates.Clear();
                    foreach (Point pos in saveData.MainIterationCoordinates)
                    {
                        MainIterationCoordinates.Add(pos);
                    }

                    InBetweenIterationCoordinates.Clear();
                    foreach (Point pos in saveData.InBetweenIterationCoordinates)
                    {
                        InBetweenIterationCoordinates.Add(pos);
                    }
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

        private void DonateBtn_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://ko-fi.com/detanaputra";
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}"));
        }

        private void SplitClearBtn_Click(object sender, RoutedEventArgs e)
        {
            SplitClearContextMenu.PlacementTarget = SplitContextTarget;
            SplitClearContextMenu.IsOpen = true;
        }

        private void SplitClearMain_Click(object sender, RoutedEventArgs e)
        {
            MainIterationCoordinates.Clear();
        }

        private void SplitClearInBetween_Click(object sender, RoutedEventArgs e)
        {
            InBetweenIterationCoordinates.Clear();
        }
    }
}