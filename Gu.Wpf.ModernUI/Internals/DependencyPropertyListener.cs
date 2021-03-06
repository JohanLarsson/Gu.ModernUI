namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    internal sealed class DependencyPropertyListener : DependencyObject, IDisposable
    {
        private static readonly DependencyProperty DummyProperty = DependencyProperty.Register(
            "Dummy",
            typeof(object),
            typeof(DependencyPropertyListener),
            new PropertyMetadata(null, OnDummyChanged));

        public DependencyPropertyListener(DependencyObject source, DependencyProperty property)
            : this(source, new PropertyPath(property))
        {
        }

        public DependencyPropertyListener(DependencyObject source, string property)
            : this(source, new PropertyPath(property))
        {
        }

        public DependencyPropertyListener(DependencyObject source, PropertyPath property)
        {
            this.Binding = new Binding
            {
                Source = source,
                Path = property,
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            this.BindingExpression = (BindingExpression)BindingOperations.SetBinding(this, DummyProperty, this.Binding);
        }

        public event EventHandler<DependencyPropertyChangedEventArgs> Changed;

        public BindingExpression BindingExpression { get; private set; }

        public Binding Binding { get; }

        public DependencyObject Source => (DependencyObject)this.Binding.Source;

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, DummyProperty);
        }

        private static void OnDummyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DependencyPropertyListener)d).OnChanged(e);
        }

        private void OnChanged(DependencyPropertyChangedEventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }
    }
}