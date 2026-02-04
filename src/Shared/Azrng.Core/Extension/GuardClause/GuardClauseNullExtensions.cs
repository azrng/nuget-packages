using Azrng.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Azrng.Core.Extension.GuardClause
{
    /// <summary>
    /// null值校验的扩展方法类
    /// </summary>
    /// <example>
    /// Guard.Against.Null(input, nameof(input));
    /// </example>
    public static class GuardClauseNullExtensions
    {
        /// <summary>
        /// 如果输入的值为null，抛出自定义异常
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="parameterName"></param>
        /// <param name="message">Optional. Custom error message</param>
        /// <param name="exceptionCreator"></param>
        /// <returns><paramref name="input" /> if the value is not null.</returns>
        /// <exception cref="Exception"></exception>
        public static T Null<T>(this IGuardClause guardClause,
                                [NotNull] T input,
                                string? message = null,
                                [CallerArgumentExpression("input")] string? parameterName = null,
                                Func<Exception>? exceptionCreator = null)
        {
            if (input is not null)
            {
                return input;
            }

            var exception = exceptionCreator?.Invoke();
            if (string.IsNullOrEmpty(message))
            {
                throw exception ?? new ArgumentNullException(parameterName);
            }

            throw exception ?? new ArgumentNullException(parameterName, message);
        }

        /// <summary>
        /// 如果输入的值为null，抛出自定义异常
        /// </summary>
        /// <typeparam name="T">Must be a value type.</typeparam>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message">Optional. Custom error message</param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <returns><paramref name="input" /> if the value is not null.</returns>
        /// <exception cref="Exception"></exception>
        public static T Null<T>(this IGuardClause guardClause,
                                [NotNull] T? input,
                                string? message = null,
                                [CallerArgumentExpression("input")] string? parameterName = null,
                                Func<Exception>? exceptionCreator = null) where T : struct
        {
            if (input is not null)
            {
                return input.Value;
            }

            var exception = exceptionCreator?.Invoke();

            if (string.IsNullOrEmpty(message))
            {
                throw exception ?? new ArgumentNullException(parameterName);
            }

            throw exception ?? new ArgumentNullException(parameterName, message);
        }

        /// <summary>
        /// 如果输入的值为null或者空，抛出自定义异常
        /// </summary>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string NullOrEmpty(this IGuardClause guardClause,
                                         string input,
                                         string? message = null,
                                         [CallerArgumentExpression("input")] string? parameterName = null,
                                         Func<Exception>? exceptionCreator = null)
        {
            Guard.Against.Null(input, parameterName, message, exceptionCreator);
            if (input == string.Empty)
            {
                throw exceptionCreator?.Invoke() ??
                      new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
            }

            return input;
        }

        /// <summary>
        /// 如果输入的值为null或者空，抛出自定义异常
        /// </summary>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Guid NullOrEmpty(this IGuardClause guardClause,
                                       [NotNull] Guid? input,
                                       string? message = null,
                                       [CallerArgumentExpression("input")] string? parameterName = null,
                                       Func<Exception>? exceptionCreator = null)
        {
            Guard.Against.Null(input, message: message, parameterName: parameterName, exceptionCreator: exceptionCreator);
            if (input == Guid.Empty)
            {
                throw exceptionCreator?.Invoke() ??
                      new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
            }

            return input.Value;
        }

        /// <summary>
        /// 如果输入的值为null或者空，抛出自定义异常
        /// </summary>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEnumerable<T> NullOrEmpty<T>(this IGuardClause guardClause,
                                                    IEnumerable<T> input,
                                                    string? message = null,
                                                    [CallerArgumentExpression("input")] string? parameterName = null,
                                                    Func<Exception>? exceptionCreator = null)
        {
            Guard.Against.Null(input, parameterName, message, exceptionCreator: exceptionCreator);

            if (input is Array and
                    { Length: 0 } //Try checking first with pattern matching because it's faster than TryGetNonEnumeratedCount on Array
#if NET6_0_OR_GREATER
            || input.TryGetNonEnumeratedCount(out var count) && count == 0
#endif
                ||
                !input.Any())
            {
                throw exceptionCreator?.Invoke() ??
                      new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
            }

            return input;
        }

        /// <summary>
        /// 如果输入的值为null或者空格，抛出自定义异常
        /// </summary>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string NullOrWhiteSpace(this IGuardClause guardClause,
                                              [NotNull] string input,
                                              string? message = null,
                                              [CallerArgumentExpression("input")] string? parameterName = null,
                                              Func<Exception>? exceptionCreator = null)
        {
            Guard.Against.NullOrEmpty(input, message: message, parameterName: parameterName, exceptionCreator: exceptionCreator);
            if (string.IsNullOrWhiteSpace(input))
            {
                throw exceptionCreator?.Invoke() ??
                      new ArgumentException(message ?? $"Required input {parameterName} was empty.", parameterName);
            }

            return input;
        }

        /// <summary>
        /// 如果输入的值为默认值，抛出自定义异常
        /// </summary>
        /// <param name="guardClause"></param>
        /// <param name="input"></param>
        /// <param name="message"></param>
        /// <param name="parameterName"></param>
        /// <param name="exceptionCreator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T Default<T>(this IGuardClause guardClause,
                                   [AllowNull, NotNull] T input,
                                   string? message = null,
                                   [CallerArgumentExpression("input")] string? parameterName = null,
                                   Func<Exception>? exceptionCreator = null)
        {
            if (input is null || EqualityComparer<T>.Default.Equals(input, default))
            {
                throw exceptionCreator?.Invoke() ??
                      new ArgumentException(message ?? $"Parameter [{parameterName}] is default value for type {typeof(T).Name}",
                          parameterName);
            }

            return input;
        }
    }
}