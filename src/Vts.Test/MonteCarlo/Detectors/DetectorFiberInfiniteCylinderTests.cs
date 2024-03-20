using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.Sources;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Detectors
{
    /// <summary>
    /// These tests verify that the specification of a detector fiber for reflectance and transmittance
    /// on a MultiLayerInfiniteCylinder tissue processes the exiting photon
    /// correctly. 
    /// </summary>
    [TestFixture]
    public class DetectorFiberInfiniteCylinderTests
    {
        private SimulationOptions _simulationOptions;
        private ISourceInput _source;
        private ITissueInput _tissue;
        private IList<IDetectorInput> _detectors;
        private SimulationOutput _output;
        private const double DetectorRadius = 1; // debug set to 10

        /// <summary>
        /// Setup input to the MC for a homogeneous one layer tissue with 
        /// fiber surface circle and specify fiber detector and R(rho).
        /// Need to create new simulation for open and NA cases since output
        /// cannot handle two detectors of same type
        /// </summary>
        [OneTimeSetUp]
        public void Execute_Monte_Carlo()
        {
            // instantiate common classes
            _simulationOptions = new SimulationOptions(
                0,
                RandomNumberGeneratorType.MersenneTwister,
                AbsorptionWeightingType.Discrete,
                PhaseFunctionType.HenyeyGreenstein,
                new List<DatabaseType>(), 
                false, // track statistics
                0.0, // RR threshold -> 0 = no RR performed
                0);
            _source = new DirectionalPointSourceInput(
                     new Position(0.01, 0.0, 0.0),
                     new Direction(0.0, 0.0, 1.0),
                     0);

            _tissue = new MultiLayerInfiniteCylinderTissueInput(
                new ITissueRegion[] // air-cylinder-cylinder-air
                {
                    //new InfiniteCylinderTissueRegion(
                    //    new DoubleRange(double.NegativeInfinity, 0.0),
                    //    double.PositiveInfinity,
                    //    new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                    //new InfiniteCylinderTissueRegion(
                    //    new DoubleRange(0.0, 1.0),
                    //    4,
                    //    new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                    //new InfiniteCylinderTissueRegion(
                    //    new DoubleRange(1.0, 2.0),
                    //    3,
                    //    new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                    //new InfiniteCylinderTissueRegion(
                    //    new DoubleRange(2.0, 4.0),
                    //    2,
                    //    new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    new InfiniteCylinderTissueRegion(
                    new Position(0,0,10),
                    double.PositiveInfinity,
                    new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                    new InfiniteCylinderTissueRegion(
                        new Position(0,0,20),
                        20,
                        new OpticalProperties(0.1, 1.0, 0.8, 1.4)),
                    new InfiniteCylinderTissueRegion(
                        new Position(0,0,20),
                        15,
                        new OpticalProperties(0.1, 1.0, 0.8, 1.4)),
                    new InfiniteCylinderTissueRegion(
                        new Position(0,0, 20),
                        10,
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                });

            _detectors = new List<IDetectorInput>
            {
                new SurfaceFiberReflectanceInfiniteCylinderDetectorInput()
                {
                    Center = new Position(0, 0, 0), 
                    Radius = DetectorRadius, 
                    TallySecondMoment = true,
                    N = 1.4,  
                    NA = 1.4,
                    FinalTissueRegionIndex = 0,
                    Name = "reflectOpen"
                },
                new SurfaceFiberReflectanceInfiniteCylinderDetectorInput()
                {
                    Center = new Position(0, 0, 0), 
                    Radius = DetectorRadius, 
                    TallySecondMoment = true,
                    N = 1.4,
                    FinalTissueRegionIndex = 0,
                    NA = 0.39,
                    Name = "reflectNA"
                },
                new SurfaceFiberTransmittanceInfiniteCylinderDetectorInput()
                {
                    Center = new Position(0, 0, 10),
                    Radius = DetectorRadius,
                    TallySecondMoment = true,
                    N = 1.4,
                    NA = 1.4,
                    FinalTissueRegionIndex = 3,
                    Name = "transmitOpen"
                },
                new SurfaceFiberTransmittanceInfiniteCylinderDetectorInput()
                {
                    Center = new Position(0, 0, 10),
                    Radius = DetectorRadius,
                    TallySecondMoment = true,
                    N = 1.4,
                    FinalTissueRegionIndex = 3,
                    NA = 0.39,
                    Name = "transmitNA"
                },
            };

            var inputOpen = new SimulationInput(
                100,
                "",
                _simulationOptions,
                _source,
                _tissue,
                _detectors);
            _output = new MonteCarloSimulation(inputOpen).Run();

        }

        /// <summary>
        /// Test to validate fiber at tissue reflectance surface fully open. 
        /// Validation values based on prior test.
        /// </summary>
        [Test]
        public void Validate_fully_open_reflectance_surface_fiber_detector_produces_correct_results()
        {
            Assert.Less(Math.Abs(_output.SurFibRefInfCyl - 0.079266), 0.000001);
            Assert.Less(Math.Abs(_output.SurFibRefInfCyl2 - 0.024315), 0.000001);
            Assert.AreEqual(26, _output.SurFib_TallyCount);
        }

    }
}
