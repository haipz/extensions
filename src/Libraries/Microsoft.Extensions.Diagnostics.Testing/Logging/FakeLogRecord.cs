// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.Logging.Testing;

/// <summary>
/// A single log record tracked by <see cref="FakeLogger"/>.
/// </summary>
public class FakeLogRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FakeLogRecord"/> class.
    /// </summary>
    /// <param name="level">The level used when producing the log record.</param>
    /// <param name="id">The ID representing the specific log statement.</param>
    /// <param name="state">The opaque state supplied by the caller when creating the log record.</param>
    /// <param name="exception">An optional exception associated with the log record.</param>
    /// <param name="message">The formatted message text for the record.</param>
    /// <param name="scopes">List of active scopes active for this log record.</param>
    /// <param name="category">The optional category for this record, which corresponds to the T in <see cref="ILogger{T}"/>.</param>
    /// <param name="enabled">Whether the log level was enabled or not when the <see cref="FakeLogger.Log"/> method was called.</param>
    /// <param name="timestamp">The time at which the log record was created.</param>
#pragma warning disable S107 // Methods should not have too many parameters
    public FakeLogRecord(LogLevel level, EventId id, object? state, Exception? exception, string message, IReadOnlyList<object?> scopes, string? category, bool enabled, DateTimeOffset timestamp)
#pragma warning restore S107 // Methods should not have too many parameters
    {
        Level = level;
        Id = id;
        State = state;
        Exception = exception;
        Message = Throw.IfNull(message);
        Scopes = Throw.IfNull(scopes);
        Category = category;
        LevelEnabled = enabled;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the level used when producing the log record.
    /// </summary>
    public LogLevel Level { get; }

    /// <summary>
    /// Gets the ID representing the specific log statement.
    /// </summary>
    public EventId Id { get; }

    /// <summary>
    /// Gets the opaque state supplied by the caller when creating the log record.
    /// </summary>
    public object? State { get; }

    /// <summary>
    /// Gets the opaque state supplied by the caller when creating the log record as a read-only list.
    /// </summary>
    /// <remarks>
    /// When logging using the code generator logging model, the arguments you supply to the logging method are packaged into a single state object which is delivered to the <see cref="ILogger.Log"/>
    /// method. This state can be retrieved as a set of name/value pairs encoded in a read-only list.
    ///
    /// The object returned by this property is the same as what <see cref="State"/> returns, except it has been cast to a read-only list.
    /// </remarks>
    /// <exception cref="InvalidCastException">The state object is not compatible with supported logging model and is not a read-only list.</exception>
    public IReadOnlyList<KeyValuePair<string, string?>>? StructuredState => (IReadOnlyList<KeyValuePair<string, string?>>?)State;

    /// <summary>
    /// Gets the value of a particular key value pair in the record's structured state.
    /// </summary>
    /// <param name="key">The key to search for in the record's structured state.</param>
    /// <returns>
    /// The value associated with the key, or <see langword="null"/> if the key was not found. If the structured
    /// state contains multiple entries with the same key, the value associated with the first matching key encountered is returned.
    /// </returns>
    public string? GetStructuredStateValue(string key)
    {
        if (StructuredState is not null)
        {
            foreach (var kvp in StructuredState)
            {
                if (kvp.Key == key)
                {
                    return kvp.Value;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets an optional exception associated with the log record.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the formatted message text for the record.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the logging scopes active when the log record was created.
    /// </summary>
    public IReadOnlyList<object?> Scopes { get; }

    /// <summary>
    /// Gets the optional category of this record.
    /// </summary>
    /// <remarks>
    /// The category corresponds to the T value in <see cref="ILogger{T}" />.
    /// </remarks>
    public string? Category { get; }

    /// <summary>
    /// Gets a value indicating whether the log level was enabled or disabled when this record was collected.
    /// </summary>
    public bool LevelEnabled { get; }

    /// <summary>
    /// Gets the time at which the log record was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Returns a string representing this object.
    /// </summary>
    /// <returns>A string that helps identity this object.</returns>
    public override string ToString() => Formatter(this);

    internal static string Formatter(FakeLogRecord record)
    {
        // these strings are kept to the same length so the output lines up nicely
        var level = record.Level switch
        {
            LogLevel.Debug => "debug",
            LogLevel.Information => " info",
            LogLevel.Warning => " warn",
            LogLevel.Error => "error",
            LogLevel.Critical => " crit",
            LogLevel.Trace => "trace",
            LogLevel.None => " none",
            _ => "invld",
        };

        return $"[{record.Timestamp:mm:ss.fff}, {level}] {record.Message}";
    }
}
