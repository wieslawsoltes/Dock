﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AvaloniaDemo.Views.Documents
{
    public class Document2View : UserControl
    {
        public Document2View()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
