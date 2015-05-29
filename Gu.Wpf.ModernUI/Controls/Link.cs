namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;

    using Gu.Wpf.ModernUI.Navigation;

    /// <summary>
    /// A link that navigates to things
    /// </summary>
    public class Link : ButtonBase, ILink
    {
        /// <summary>
        /// Identifies the DisplayName property.
        /// </summary>
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register(
            "DisplayName",
            typeof(string),
            typeof(Link),
            new FrameworkPropertyMetadata(
                default(string),
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Identifies the SourceProperty property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(Uri),
            typeof(Link),
            new FrameworkPropertyMetadata(
                default(Uri),
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSourceChanged));

        internal static readonly DependencyPropertyKey IsNavigatedToPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsNavigatedTo",
            typeof(bool),
            typeof(Link),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsNavigatedToProperty = IsNavigatedToPropertyKey.DependencyProperty;

        internal static readonly DependencyPropertyKey CanNavigatePropertyKey = DependencyProperty.RegisterReadOnly(
            "CanNavigate",
            typeof(bool),
            typeof(Link),
            new PropertyMetadata(true));

        public static readonly DependencyProperty CanNavigateProperty = CanNavigatePropertyKey.DependencyProperty;

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty LinkNavigatorProperty = Modern.LinkNavigatorProperty.AddOwner(typeof(Link));

        static Link()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Link), new FrameworkPropertyMetadata(typeof(Link)));
        }

        public Link()
        {
            this.Command = LinkCommands.NavigateLink; // For some reason the command must be set in ctor for it to work with title links.
        }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the source uri.
        /// </summary>
        /// <value>The source.</value>
        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets if the current linknavigator can navigate to Source
        /// </summary>
        public bool CanNavigate
        {
            get { return (bool)GetValue(CanNavigateProperty); }
            protected set { SetValue(CanNavigatePropertyKey, value); }
        }

        /// <summary>
        /// LinkCommands updates this
        /// </summary>
        bool ILink.CanNavigate
        {
            get { return this.CanNavigate; }
            set { this.CanNavigate = value; }
        }

        /// <summary>
        /// Gets if the current navigationtarget Source == this.Source
        /// </summary>
        public bool IsNavigatedTo
        {
            get { return (bool)GetValue(IsNavigatedToProperty); }
            protected set { SetValue(IsNavigatedToPropertyKey, value); }
        }

        /// <summary>
        /// LinkCommands updates this
        /// </summary>
        bool ILink.IsNavigatedTo
        {
            get { return this.IsNavigatedTo; }
            set { this.IsNavigatedTo = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILinkNavigator LinkNavigator
        {
            get { return (ILinkNavigator)GetValue(LinkNavigatorProperty); }
            set { SetValue(LinkNavigatorProperty, value); }
        }

        public override string ToString()
        {
            return string.Format("{0}, DisplayName: {1}, Source: {2}, CanNavigate: {3}, IsNavigatedTo: {4}", GetType().Name, this.DisplayName, this.Source, this.CanNavigate, this.IsNavigatedTo);
        }

        protected virtual void OnSourceChanged(Uri oldSource, Uri newSource)
        {
            if (newSource != null && this.Command != LinkCommands.NavigateLink)
            {
                SetCurrentValue(CommandProperty, LinkCommands.NavigateLink);
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Link)d).OnSourceChanged((Uri)e.OldValue, (Uri)e.NewValue);
        }
    }
}