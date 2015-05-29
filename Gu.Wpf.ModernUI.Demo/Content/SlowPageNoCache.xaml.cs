﻿namespace Gu.Wpf.ModernUI.Demo.Content
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for SlowPageNoCache.xaml
    /// </summary>
    public partial class SlowPageNoCache : UserControl
    {
        public SlowPageNoCache()
        {
            var sw = Stopwatch.StartNew();
            InitializeComponent();
            Thread.Sleep(TimeSpan.FromSeconds(2));
            this.Text.Text = string.Format("This page took {0} ms to load", sw.ElapsedMilliseconds);
        }
    }
}