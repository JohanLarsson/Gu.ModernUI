﻿namespace Gu.Wpf.ModernUI
{
    using System.Security.Permissions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Threading;

    using Gu.Wpf.ModernUI.Internals;

    using ModernUi.Interfaces;

    /// <summary>
    /// A control for showing messages ribbon style
    /// </summary>
    public class ModernPopup : Control
    {
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(
            "ClickCommand",
            typeof(ICommand),
            typeof(ModernPopup),
            new PropertyMetadata(default(ICommand)));

        static ModernPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernPopup), new FrameworkPropertyMetadata(typeof(ModernPopup)));
        }

        public ModernPopup()
        {
            this.ClickCommand = new RelayCommand(OnClick, _ => true);
        }

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public DialogResult? Result { get; private set; }

        internal DialogResult RunDialog(ModernWindow owner, IDialogHandler dialogHandler, DialogViewModel viewModel)
        {
            this.Result = null;
            var decorator = owner.AdornerDecorator;
            if (decorator == null)
            {
                return ShowDialog(viewModel);
            }
            AdornerLayer adornerLayer = decorator.AdornerLayer;
            var uiElement = decorator.Child;
            if (adornerLayer == null || uiElement == null)
            {
                return ShowDialog(viewModel);
            }
            var adorner = new ContentAdorner(uiElement, this);
            adornerLayer.Add(adorner);

            while (this.Result == null)
            {
                DoEvents();
            }

            adornerLayer.Remove(adorner);
            return this.Result.Value;
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var dispatcherOperationCallback = new DispatcherOperationCallback(ExitFrame);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, dispatcherOperationCallback, frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }

        private void OnClick(object obj)
        {
            this.Result = (DialogResult)obj;
        }

        /// <summary>
        /// Showing a standard dialog as fallback.
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        private DialogResult ShowDialog(DialogViewModel vm)
        {
            var dialog = new ModernDialog
                                            {
                                                Title = vm.Title,
                                                DataContext = vm
                                            };
            dialog.ShowDialog();
            switch (dialog.MessageBoxResult)
            {
                case MessageBoxResult.OK:
                    return DialogResult.OK;
                case MessageBoxResult.Cancel:
                    return DialogResult.Cancel;
                case MessageBoxResult.Yes:
                    return DialogResult.Yes;
                case MessageBoxResult.No:
                    return DialogResult.No;
                case MessageBoxResult.None:
                default:
                    return DialogResult.None;
            }
        }
    }
}
