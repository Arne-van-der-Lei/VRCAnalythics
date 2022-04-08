using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The event vrchat sends to us 
/// </summary>
[Serializable]
public class AnalythicsElement
{
    /// <summary>
    /// ID of the event
    /// </summary>
    public string ID;
    /// <summary>
    /// the world id of the event
    /// </summary>
    public string worldId;
    /// <summary>
    /// the metric id we are tracking AKA: event id
    /// </summary>
    public string metricId;
    /// <summary>
    /// the data that we are tracking
    /// </summary>
    public int count;
    /// <summary>
    /// position of the user that fired the event
    /// </summary>
    public Vector3 position;
    /// <summary>
    /// When the event was fired
    /// </summary>
    public DateTime timestamp;
}