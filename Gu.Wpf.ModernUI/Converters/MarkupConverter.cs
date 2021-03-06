﻿namespace Gu.Wpf.ModernUI
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Windows.Data;
    using System.Windows.Markup;

    using Gu.Wpf.ModernUI.TypeConverters;

    /// <summary>
    /// Class implements a base for a typed value converter used as a markup extension. Override the Convert method in the inheriting class
    /// </summary>
    /// <typeparam name="TInput">Type of the expected input - value to be converted</typeparam>
    /// <typeparam name="TResult">Type of the result of the conversion</typeparam>
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    public abstract class MarkupConverter<TInput, TResult> : MarkupExtension, IValueConverter
    {
        private static readonly ITypeConverter<TInput> InputTypeConverter = TypeConverterFactory.Create<TInput>();
        private static readonly ITypeConverter<TResult> ResultTypeConverter = TypeConverterFactory.Create<TResult>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkupConverter{TInput, TResult}"/> class.
        /// </summary>
        // ReSharper disable once EmptyConstructor, think xaml needs it
        protected MarkupConverter()
        {
        }

        /// <inheritdoc/>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.VerifyValue(value, parameter);
            if (InputTypeConverter.IsValid(value))
            {
                var convertTo = InputTypeConverter.ConvertTo(value, culture);
                return this.Convert(convertTo, culture);
            }

            return this.ConvertDefault();
        }

        /// <inheritdoc/>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            this.VerifyValue(value, parameter);
            if (ResultTypeConverter.CanConvertTo(value, culture))
            {
                var convertTo = ResultTypeConverter.ConvertTo(value, culture);
                return this.ConvertBack(convertTo, culture);
            }

            return this.ConvertBackDefault();
        }

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        protected NotSupportedException ThrowConvertBackNotSupported()
        {
            return new NotSupportedException($"{this.GetType().Name} does not support use in bindings with Mode = TwoWay.");
        }

        protected abstract TResult Convert(TInput value, CultureInfo culture);

        protected virtual TResult ConvertDefault()
        {
            return default(TResult);
        }

        protected abstract TInput ConvertBack(TResult value, CultureInfo culture);

        protected virtual TInput ConvertBackDefault()
        {
            return default(TInput);
        }

        private void VerifyValue(object value, object parameter, [CallerMemberName] string caller = null)
        {
            if (Is.InDesignMode)
            {
                if (parameter != null)
                {
                    var message = $"ConverterParameter makes no sense for MarkupConverter. Parameter was: {parameter} for converter of type {this.GetType().Name}";
                    throw new ArgumentException(
                        message);
                }

                if (!InputTypeConverter.IsValid(value))
                {
                    var message = $"{caller} value: {value} is not valid for converter of type: {this.GetType().Name} from: {typeof(TInput).Name} to {typeof(TResult).Name}";
                    throw new ArgumentException(message, nameof(value));
                }
            }
        }
    }
}
