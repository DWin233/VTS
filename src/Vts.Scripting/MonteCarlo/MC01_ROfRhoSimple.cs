﻿using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.Scripting.Utilities;
using Plotly.NET.CSharp;

namespace Vts.Scripting.MonteCarlo;

/// <summary>
/// Class using the Vts.dll library to demonstrate performing a simple Monte Carlo simulation
/// </summary>
public class MC01_ROfRhoSimple : IDemoScript
{
    /// <summary>
    /// Sample script to demonstrate this class' stated purpose
    /// </summary>
    public static void RunDemo()
    {
        // Example 01: run a simple Monte Carlo simulation with 1000 photons.
        // Notes:
        //    - default source is a point source beam normally incident at the origin 
        //    - default tissue is a 100mm thick slab with air-tissue boundary and optical properties: mua: 0.01, musp: 1.0, g: 0.8, n:1.4

        // create a SimulationInput object to define the simulation
        var detectorRange = new DoubleRange(start: 0, stop: 40, number: 201);
        var simulationInput = new SimulationInput
        {
            // specify the number of photons to run
            N = 1000,

            // define a single R(rho) detector by the endpoints of rho bins
            DetectorInputs = new [] { new ROfRhoDetectorInput { Rho = detectorRange, Name = "ROfRho" } }, // name can be whatever you want
        };

        // create the simulation
        var simulation = new MonteCarloSimulation(simulationInput);

        // run the simulation
        var simulationOutput = simulation.Run();

        // plot the results using Plotly.NET
        var detectorResults = (ROfRhoDetector)simulationOutput.ResultsDictionary["ROfRho"];
        var logReflectance = detectorResults.Mean.Select(r => Math.Log(r)).ToArray();
        var (detectorMidpoints, xLabel, yLabel) = (detectorRange.GetMidpoints(), "rho [mm]", "log(R(ρ)) [mm-2]");
        PlotHelper.LineChart(detectorMidpoints, logReflectance, xLabel, yLabel, title: "log(R(ρ)) [mm-2]").Show();
    }
}