using System.Windows;
using MacroRecorder.Gui.ViewModels;

namespace MacroRecorder.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
