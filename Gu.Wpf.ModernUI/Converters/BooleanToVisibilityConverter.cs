﻿namespace Gu.Wpf.ModernUI
{
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    /// <summary>Converts <see cref="bool"/> to <see cref="Visibility"/>.</summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Used from xaml")]
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    [ValueConversion(typeof(bool?), typeof(Visibility))]
    public class BooleanToVisibilityConverter : MarkupConverter<bool?, Visibility>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToVisibilityConverter"/> class.
        /// </summary>
        // ReSharper disable once EmptyConstructor, think xaml needs it
        public BooleanToVisibilityConverter()
        {
        }

        /// <summary>
        /// Gets or sets the value to be returned when the converted value is true
        /// </summary>
        public Visibility WhenTrue { get; set; } = Visibility.Visible;

        /// <summary>
        /// Gets or sets the value to be returned when the converted value is false
        /// </summary>
        public Visibility WhenFalse { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Gets or sets the value to be returned when the converted value is null
        /// </summary>
        public Visibility WhenNull { get; set; } = Visibility.Collapsed;

        /// <inheritdoc />
        protected override Visibility Convert(bool? value, CultureInfo culture)
        {
            if (value == null)
            {
                return this.WhenNull;
            }

            return value == true ? this.WhenTrue : this.WhenFalse;
        }

        /// <inheritdoc />
        protected override bool? ConvertBack(Visibility value, CultureInfo culture)
        {
            if (value == this.WhenTrue)
            {
                return true;
            }

            if (value == this.WhenFalse)
            {
                return false;
            }

            return null;
        }
    }
}
