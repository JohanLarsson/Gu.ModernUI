#pragma warning disable SA1124, SA1201 // cleaning up this mess later. Can hopefully remove it
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.
namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    /// <summary>
    /// Represents a control with a single piece of content and when that content
    /// changes performs a transition animation.
    /// </summary>
    /// <QualityBand>Experimental</QualityBand>
    /// <remarks>The API for this control will change considerably in the future.</remarks>
    // [TemplateVisualState(GroupName = PresentationGroup, Name = NormalState)]
    // [TemplateVisualState(GroupName = PresentationGroup, Name = DefaultTransitionState)]
    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    public class TransitioningContentControl : ContentControl
    {
        /// <summary>
        /// The name of the group that holds the presentation states.
        /// </summary>
        private const string PresentationGroup = "PresentationStates";

        /// <summary>
        /// The name of the state that represents a normal situation where no
        /// transition is currently being used.
        /// </summary>
        private const string NormalState = "Normal";

        /// <summary>
        /// The name of the state that represents the default transition.
        /// </summary>
        public const string DefaultTransitionState = "DefaultTransition";

        /// <summary>
        /// The name of the control that will display the previous content.
        /// </summary>
        internal const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";

        /// <summary>
        /// The name of the control that will display the current content.
        /// </summary>
        internal const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

        /// <summary>
        /// Gets or sets the current content presentation site.
        /// </summary>
        /// <value>The current content presentation site.</value>
        private ContentPresenter CurrentContentPresentationSite { get; set; }

        /// <summary>
        /// Gets or sets the previous content presentation site.
        /// </summary>
        /// <value>The previous content presentation site.</value>
        private ContentPresenter PreviousContentPresentationSite { get; set; }

        /// <summary>
        /// Occurs when the IsTransitioning value has changed.
        /// </summary>
        public event EventHandler IsTransitioningChanged;

        /// <summary>
        /// Indicates whether the control allows writing IsTransitioning.
        /// </summary>
        private bool allowIsTransitioningWrite;

        /// <summary>
        /// Gets a value indicating whether this instance is currently performing
        /// a transition.
        /// </summary>
        public bool IsTransitioning
        {
            get => (bool)this.GetValue(IsTransitioningProperty);

            private set
            {
                this.allowIsTransitioningWrite = true;
                this.SetValue(IsTransitioningProperty, value);
                this.allowIsTransitioningWrite = false;
                this.IsTransitioningChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>Identifies the <see cref="IsTransitioning"/> dependency property.</summary>
        public static readonly DependencyProperty IsTransitioningProperty = DependencyProperty.Register(
                nameof(IsTransitioning),
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(OnIsTransitioningChanged));

        /// <summary>
        /// IsTransitioningProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningContentControl that changed its IsTransitioning.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsTransitioningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;

            if (!source.allowIsTransitioningWrite)
            {
                source.SetCurrentValue(IsTransitioningProperty, (bool)e.OldValue);
                throw new InvalidOperationException("IsTransitioning property is read-only.");
            }
        }

        /// <summary>
        /// The storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard currentTransition;

        /// <summary>
        /// Gets or sets the storyboard that is used to transition old and new content.
        /// </summary>
        private Storyboard CurrentTransition
        {
            set
            {
                if (this.currentTransition != null)
                {
                    this.currentTransition.Completed -= this.OnTransitionCompleted;
                }

                this.currentTransition = value;
                if (this.currentTransition != null)
                {
                    this.currentTransition.Completed += this.OnTransitionCompleted;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the transition to use. These correspond
        /// directly to the VisualStates inside the PresentationStates group.
        /// </summary>
        public string Transition
        {
            get => this.GetValue(TransitionProperty) as string;
            set => this.SetValue(TransitionProperty, value);
        }

        /// <summary>Identifies the <see cref="Transition"/> dependency property.</summary>
        public static readonly DependencyProperty TransitionProperty =
            DependencyProperty.Register(
                nameof(Transition),
                typeof(string),
                typeof(TransitioningContentControl),
                new PropertyMetadata(DefaultTransitionState, OnTransitionChanged));

        /// <summary>
        /// TransitionProperty property changed handler.
        /// </summary>
        /// <param name="d">TransitioningContentControl that changed its Transition.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnTransitionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = (TransitioningContentControl)d;
            var oldTransition = e.NewValue as string;
            var newTransition = e.NewValue as string;

            if (source.IsTransitioning)
            {
                source.AbortTransition();
            }

            // find new transition
            var newStoryboard = source.GetStoryboard(newTransition);

            // unable to find the transition.
            if (newStoryboard == null)
            {
                // could be during initialization of xaml that presentationgroups was not yet defined
                if (source.TryGetVisualStateGroup(PresentationGroup) == null)
                {
                    // will delay check
                    source.CurrentTransition = null;
                }
                else
                {
                    // revert to old value
                    source.SetValue(TransitionProperty, oldTransition);

                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, "Transition '{0}' was not defined.", newTransition));
                }
            }
            else
            {
                source.CurrentTransition = newStoryboard;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current transition
        /// will be aborted when setting new content during a transition.
        /// </summary>
        public bool RestartTransitionOnContentChange
        {
            get => (bool)this.GetValue(RestartTransitionOnContentChangeProperty);
            set => this.SetValue(RestartTransitionOnContentChangeProperty, value);
        }

        /// <summary>Identifies the <see cref="RestartTransitionOnContentChange"/> dependency property.</summary>
        public static readonly DependencyProperty RestartTransitionOnContentChangeProperty = DependencyProperty.Register(
                nameof(RestartTransitionOnContentChange),
                typeof(bool),
                typeof(TransitioningContentControl),
                new PropertyMetadata(false, (d, e) => ((TransitioningContentControl)d).OnRestartTransitionOnContentChangeChanged((bool)e.OldValue, (bool)e.NewValue)));

        /// <summary>
        /// Called when the RestartTransitionOnContentChangeProperty changes.
        /// </summary>
        /// <param name="oldValue">The old value of RestartTransitionOnContentChange.</param>
        /// <param name="newValue">The new value of RestartTransitionOnContentChange.</param>
        protected virtual void OnRestartTransitionOnContentChangeChanged(bool oldValue, bool newValue)
        {
        }

        /// <summary>
        /// Occurs when the current transition has completed.
        /// </summary>
        public event RoutedEventHandler TransitionCompleted;

        /// <summary>
        /// Initializes static members of the <see cref="TransitioningContentControl"/> class.
        /// </summary>
        static TransitioningContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningContentControl), new FrameworkPropertyMetadata(typeof(TransitioningContentControl)));
        }

        /// <summary>
        /// Builds the visual tree for the TransitioningContentControl control
        /// when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this.IsTransitioning)
            {
                this.AbortTransition();
            }

            base.OnApplyTemplate();

            this.PreviousContentPresentationSite = this.GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
            this.CurrentContentPresentationSite = this.GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

            this.CurrentContentPresentationSite?.SetCurrentValue(ContentPresenter.ContentProperty, this.Content);

            // hookup currenttransition
            var transition = this.GetStoryboard(this.Transition);
            this.CurrentTransition = transition;
            if (transition == null)
            {
                var invalidTransition = this.Transition;

                // revert to default
                this.SetCurrentValue(TransitionProperty, DefaultTransitionState);

                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Transition '{0}' was not defined.", invalidTransition));
            }

            VisualStateManager.GoToState(this, NormalState, useTransitions: false);
        }

        /// <summary>
        /// Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        /// <param name="newContent">The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            this.StartTransition(oldContent, newContent);
        }

        /// <summary>
        /// Starts the transition.
        /// </summary>
        /// <param name="oldContent">The old content.</param>
        /// <param name="newContent">The new content.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newContent", Justification = "Should be used in the future.")]
        private void StartTransition(object oldContent, object newContent)
        {
            // both presenters must be available, otherwise a transition is useless.
            if (this.CurrentContentPresentationSite != null && this.PreviousContentPresentationSite != null)
            {
                this.CurrentContentPresentationSite.Content = newContent;

                this.PreviousContentPresentationSite.Content = oldContent;

                // and start a new transition
                if (!this.IsTransitioning || this.RestartTransitionOnContentChange)
                {
                    this.SetCurrentValue(IsTransitioningProperty, true);
                    VisualStateManager.GoToState(this, NormalState, useTransitions: false);
                    VisualStateManager.GoToState(this, this.Transition, useTransitions: true);
                }
            }
        }

        /// <summary>
        /// Handles the Completed event of the transition storyboard.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            this.AbortTransition();
            this.TransitionCompleted?.Invoke(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Aborts the transition and releases the previous content.
        /// </summary>
        public void AbortTransition()
        {
            // go to normal state and release our hold on the old content.
            VisualStateManager.GoToState(this, NormalState, useTransitions: false);
            this.IsTransitioning = false;
            if (this.PreviousContentPresentationSite != null)
            {
                this.PreviousContentPresentationSite.Content = null;
            }
        }

        /// <summary>
        /// Attempts to find a storyboard that matches the newTransition name.
        /// </summary>
        /// <param name="newTransition">The new transition.</param>
        /// <returns>A storyboard or null, if no storyboard was found.</returns>
        private Storyboard GetStoryboard(string newTransition)
        {
            var presentationGroup = this.TryGetVisualStateGroup(PresentationGroup);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
            {
                newStoryboard = presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == newTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            }

            return newStoryboard;
        }
    }
}
