using System.Collections.ObjectModel;
using System.Windows;

namespace AutoMouseClicker
{
    public class SaveData
    {
        public ObservableCollection<Point> MainIterationCoordinates = new();
        public ObservableCollection<Point> InBetweenIterationCoordinates = new();

        public SaveData(ObservableCollection<Point> mainIterationCoordinates, ObservableCollection<Point> inBetweenIterationCoordinates)
        {
            MainIterationCoordinates = mainIterationCoordinates;
            InBetweenIterationCoordinates = inBetweenIterationCoordinates;
        }
    }
}
