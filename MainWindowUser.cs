﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace AutoMouseClicker
{
    public partial class MainWindow : Window
    {
        static int _x, _y;
        private int Cycles;
        private LowLevelKeyboardListener keyboardListener;
        private bool isIterating;

        public ObservableCollection<Point> MainIterationCoordinates { get; set; } = new ObservableCollection<Point>();
        public ObservableCollection<Point> InBetweenIterationCoordinates { get; set; } = new ObservableCollection<Point>();

        public void OnKeyboardInputReceived(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat)
            {
                return;
            }

            if (e.Key is Key.Escape)
            {
                isIterating = false;
                Debug.WriteLine("Esc is pressed, prepare to abort iteration");
            }

            if (e.Key is Key.LeftCtrl or Key.RightCtrl && e.IsDown && !isIterating && !(bool)RecordInBetweenCheckBox.IsChecked)
            {
                MainIterationCoordinates.Add(GetMousePosition());
                Debug.WriteLine("CTRL button is pressed, recording mouse position to main iteration");
            }
            else if (e.Key is Key.LeftCtrl or Key.RightCtrl && e.IsDown && !isIterating && (bool)RecordInBetweenCheckBox.IsChecked)
            {
                InBetweenIterationCoordinates.Add(GetMousePosition());
                Debug.WriteLine("CTRL button is pressed, recording mouse position to inbetween interation");
            }
        }

        /*private void OnKeyboardInputReceived(object sender, KeyPressedArgs e)
        {
            if(e.KeyPressed is Key.Escape)
            {
                isIterating = false;
                Debug.WriteLine("Esc is pressed, prepare to abort iteration");
            }

            if (e.KeyPressed is Key.LeftCtrl or Key.RightCtrl)
            {
                CapturedMousePos.Add(GetMousePosition());
                Debug.WriteLine("CTRL button is pressed, recording mouse position");
            }
        }*/

        private static Point GetMousePosition()
        {
            if (GetCursorPos(out POINT point) && point.X != _x && point.Y != _y)
            {
                _x = point.X;
                _y = point.Y;
            }
            return new Point(_x, _y);
        }

        private static void SetMousePosition(Point point)
        {
            SetCursorPos((int)point.X, (int)point.Y);
        }

        private static void SendMouseEvent(MouseEventFlags flag, Point point)
        {
            int x = (int)point.X;
            int y = (int)point.Y;
            mouse_event((int)flag, x, y, 0, 0);
        }

        private void IterateMouseEvent(int delay, int repeat, bool useInBetween, int inBetweenDelay, int executeInBetweenEvery)
        {
            foreach (Point i in MainIterationCoordinates)
            {
                if (!isIterating)
                {
                    Debug.WriteLine("Esc is pressed, aborting iteration");
                    Dispatcher.Invoke(() =>
                    {
                        _ = Activate();
                        NotifyError("Mouse iteration is canceled", "Warning");
                    });
                    return;
                }

                SetMousePosition(i);
                SendMouseEvent(MouseEventFlags.LeftDown, i);
                SendMouseEvent(MouseEventFlags.LeftUp, i);
                Thread.Sleep(delay);
            }

            Cycles += 1;

            if (useInBetween && InBetweenIterationCoordinates.Count > 0 && Cycles % executeInBetweenEvery == 0)
            {
                if (!isIterating)
                {
                    Debug.WriteLine("Esc is pressed, aborting iteration");
                    Dispatcher.Invoke(() =>
                    {
                        _ = Activate();
                        NotifyError("Mouse iteration is canceled", "Warning");
                    });
                    return;
                }

                foreach (Point i in InBetweenIterationCoordinates)
                {
                    SetMousePosition(i);
                    SendMouseEvent(MouseEventFlags.LeftDown, i);
                    SendMouseEvent(MouseEventFlags.LeftUp, i);
                    Thread.Sleep(inBetweenDelay);
                }
            }

            if (Cycles <= repeat)
            {
                IterateMouseEvent(delay, repeat, useInBetween, inBetweenDelay, executeInBetweenEvery);
            }
        }

        private void SetupKnowHowTextBlock()
        {
            KnowHowTextBlock.Text = "How to use: \n " +
                "When application is active, move your cursor/mouse into position then press CTRL to record that position." +
                "The position mouse list should be updated with that coordinate. Keep recording until your positions are all recorded then press Start." +
                "\n if something goes wrong, pres ESC to cancel iteration";
        }

        private bool VerifyUserInput()
        {
            if (MainIterationCoordinates.Count < 1)
            {
                NotifyError("You haven't record any mouse position", "Warning");
                return false;
            }

            if ((bool)UseInBetweenCheckBox.IsChecked && InBetweenIterationCoordinates.Count == 0)
            {
                NotifyError("You checked use inbetween checkbox but you haven't record any inbetween mouse position", "Warning");
                return false;
            }

            bool res = int.TryParse(DelayTextBox.Text, out _);
            if (!res)
            {
                DelayTextBox.Focus();
                NotifyError("Error parsing user input. Make sure you type a number", "Warning");
                return false;
            }

            res = int.TryParse(RepeatTextBox.Text, out _);
            if (!res)
            {
                _ = RepeatTextBox.Focus();
                NotifyError("Error parsing user input. Make sure you type a number", "Warning");
                return false;
            }

            res = int.TryParse(InBetweenDelayTextBox.Text, out _);
            if (!res)
            {
                _ = InBetweenDelayTextBox.Focus();
                NotifyError("Error parsing user input. Make sure you type a number", "Warning");
                return false;
            }

            res = int.TryParse(ExecuteInBetweenEveryTextBox.Text, out _);
            if (!res)
            {
                _ = ExecuteInBetweenEveryTextBox.Focus();
                NotifyError("Error parsing user input. Make sure you type a number", "Warning");
                return false;
            }


            return true;
        }

        private void NotifyError(string text, string caption)
        {
            MessageBox.Show(this, text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void NotifySuccess(string text)
        {
            MessageBox.Show(this, text, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags
        {
            LeftDown = 0x00000002,
            LeftUp = 0x00000004,
            MiddleDown = 0x00000020,
            MiddleUp = 0x00000040,
            Move = 0x00000001,
            Absolute = 0x00008000,
            RightDown = 0x00000008,
            RightUp = 0x00000010
        }
    }





    public struct POINT
    {
        public int X;
        public int Y;
    }
}
