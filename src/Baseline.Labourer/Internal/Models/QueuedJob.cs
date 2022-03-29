﻿using System;
using System.Threading.Tasks;

namespace Baseline.Labourer.Internal;

/// <summary>
/// Model that represents a job in a queue.
/// </summary>
public class QueuedJob
{
    /// <summary>
    /// Gets or sets the id of the message.
    /// </summary>
    public string MessageId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the serialized definition of the job.
    /// </summary>
    public string SerializedDefinition { get; set; } = null!;

    /// <summary>
    /// Identifies if one <see cref="QueuedJob"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The other <see cref="QueuedJob" /> to compare.</param>
    protected bool Equals(QueuedJob other)
    {
        return SerializedDefinition == other.SerializedDefinition;
    }

    /// <summary>
    /// Identifies if one object is equal to the current one.
    /// </summary>
    /// <param name="obj">The object to compare.</param>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((QueuedJob)obj);
    }

    /// <summary>
    /// Gets the hash code used for equality comparisons in hash sets.
    /// </summary>
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return HashCode.Combine(SerializedDefinition);
    }

    /// <summary>
    /// Deserializes the definition of the queued job into an object and then returns it.
    /// </summary>
    public async Task<T> DeserializeAsync<T>()
    {
        return await SerializationUtils.DeserializeFromStringAsync<T>(SerializedDefinition);
    }
}
