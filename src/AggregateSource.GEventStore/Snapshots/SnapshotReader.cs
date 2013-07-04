﻿using System;
using EventStore.ClientAPI;

namespace AggregateSource.GEventStore.Snapshots {
  /// <summary>
  /// Represents the default behavior that reads a <see cref="Snapshot"/> from the underlying storage.
  /// </summary>
  public class SnapshotReader : ISnapshotReader {
    readonly EventStoreConnection _connection;
    readonly SnapshotStoreReadConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotReader"/> class.
    /// </summary>
    /// <param name="connection">The event store connection to use.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="connection"/> or <paramref name="configuration"/> are <c>null</c>.</exception>
    public SnapshotReader(EventStoreConnection connection, SnapshotStoreReadConfiguration configuration) {
      if (connection == null) throw new ArgumentNullException("connection");
      if (configuration == null) throw new ArgumentNullException("configuration");
      _connection = connection;
      _configuration = configuration;
    }

    /// <summary>
    /// Gets the event store connection.
    /// </summary>
    /// <value>
    /// The connection.
    /// </value>
    public EventStoreConnection Connection {
      get { return _connection; }
    }

    /// <summary>
    /// Gets the configuration used to read.
    /// </summary>
    /// <value>
    /// The configuration.
    /// </value>
    public SnapshotStoreReadConfiguration Configuration {
      get { return _configuration; }
    }

    public Optional<Snapshot> ReadOptional(string identifier) {
      if (identifier == null) throw new ArgumentNullException("identifier");
      var streamName = Configuration.Resolver.Resolve(identifier);
      var slice = Connection.ReadStreamEventsBackward(streamName, StreamPosition.End, 1, false);
      if (slice.Status == SliceReadStatus.StreamDeleted || slice.Status == SliceReadStatus.StreamNotFound || slice.NextEventNumber == -1) {
        return Optional<Snapshot>.Empty;
      }
      return new Optional<Snapshot>(Configuration.Deserializer.Deserialize(slice.Events[0]));
    }
  }
}