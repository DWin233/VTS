﻿namespace Vts.Scripting;

/// <summary>
/// Interface to constrain all demo scripts to a uniform signature
/// </summary>
interface IDemoScript
{
    /// <summary>
    /// The one required static method for all demo scripts
    /// </summary>
    static abstract void RunDemo();
}
