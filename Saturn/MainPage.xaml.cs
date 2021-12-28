using Microsoft.Maui.Controls;
using Saturn.Data.Utils;
using System;

namespace Saturn
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Logger.Start();
        }
    }
}
