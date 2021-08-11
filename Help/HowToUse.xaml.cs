using System.Windows;

namespace AutoMouseClicker.Help
{
    /// <summary>
    /// Interaction logic for HowToUse.xaml
    /// </summary>
    public partial class HowToUse : Window
    {
        public HowToUse()
        {
            InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HowToUseText.Text = "How to use: \n " +
                "When application is active, move your cursor/mouse into position then press CTRL to record that position." +
                "The position mouse list should be updated with that coordinate. Keep recording until your positions are all recorded then press Start." +
                "\n if something goes wrong, pres ESC to cancel iteration";
        }
    }
}
