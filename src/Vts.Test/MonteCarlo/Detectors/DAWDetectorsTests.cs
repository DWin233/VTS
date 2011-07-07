using System;
using System.Numerics;
using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Detectors
{
    /// <summary>
    /// These tests execute a discrete absorption weighting (DAW)
    /// MC simulation with 100 photons and verify
    /// that the tally results match the linux results given the same seed
    /// mersenne twister STANDARD_TEST
    /// </summary>
    [TestFixture]
    public class DAWDetectorsTests
    {
        private Output _outputOneLayerTissue;
        private Output _outputTwoLayerTissue;
        private double _layerThickness = 2.0;
        private double _factor;

        /// <summary>
        /// DiscreteAbsorptionWeighting detection.
        /// Setup input to the MC for a homogeneous one layer tissue and a homogeneous
        /// two layer tissue (both layers have same optical properties), execute simulations
        /// and verify results agree with linux results given the same seed
        /// mersenne twister STANDARD_TEST.  The linux results assumes photon passes
        /// through specular and deweights photon by specular.  This test starts photon 
        /// inside tissue and then multiplies result by specular deweighting to match.
        /// NOTE: currently two region executes same photon biography except for pauses
        /// at layer interface.  Variance for DAW results not degraded.
        /// </summary>
        [TestFixtureSetUp]
        public void execute_Monte_Carlo()
        {
            // instantiate common classes
            var simulationOptions = new SimulationOptions(
                     0,
                     RandomNumberGeneratorType.MersenneTwister,
                     AbsorptionWeightingType.Discrete,
                     PhaseFunctionType.HenyeyGreenstein,
                     new List<DatabaseType>() { DatabaseType.PhotonExitDataPoints, DatabaseType.CollisionInfo },
                     //null,
                     true,
                    false, // track statistics
                     0);
            var source = new DirectionalPointSourceInput(
                     new Position(0.0, 0.0, 0.0),
                     new Direction(0.0, 0.0, 1.0),
                     1); // start inside tissue
            var detectors = new List<IDetectorInput>()
                {
                    new RDiffuseDetectorInput(),
                    new ROfAngleDetectorInput(new DoubleRange(0.0, Math.PI / 2, 2)),
                    new ROfRhoDetectorInput(new DoubleRange(0.0, 10.0, 101)),
                    new ROfRhoAndAngleDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, Math.PI / 2, 2)),
                    new ROfRhoAndTimeDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, 1, 101)),
                    new ROfXAndYDetectorInput(
                        new DoubleRange(-200.0, 200.0, 401), // x
                        new DoubleRange(-200.0, 200.0, 401)), // y,
                    new ROfRhoAndOmegaDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, 1000.0, 21)),                        
                    new AOfRhoAndZDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, 10.0, 101)),
                    new ATotalDetectorInput(),
                    new FluenceOfRhoAndZDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, 10.0, 101)),
                    new TDiffuseDetectorInput(),
                    new TOfAngleDetectorInput(new DoubleRange(0.0, Math.PI / 2, 2)),
                    new TOfRhoDetectorInput(new DoubleRange(0.0, 10.0, 101)),
                    new TOfRhoAndAngleDetectorInput(
                        new DoubleRange(0.0, 10.0, 101),
                        new DoubleRange(0.0, Math.PI / 2, 2)),
                    new DosimetryOfRhoDetectorInput(
                        5.5, // z depth
                        new DoubleRange(0.0, 10.0, 101))
                };
            var inputOneLayerTissue = new SimulationInput(
                100,
                "",
                simulationOptions,
                source,
                new MultiLayerTissueInput(
                    new List<ITissueRegion>
                    { 
                        new LayerRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new LayerRegion(
                            new DoubleRange(0.0, 20.0),
                            new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                        new LayerRegion(
                            new DoubleRange(20.0, double.PositiveInfinity),
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                detectors);             
            _outputOneLayerTissue = new MonteCarloSimulation(inputOneLayerTissue).Run();

            var inputTwoLayer = new SimulationInput(
                100,
                "",
                simulationOptions,
                source,
                new MultiLayerTissueInput(
                    new List<ITissueRegion>
                                { 
                                    new LayerRegion(
                                        new DoubleRange(double.NegativeInfinity, 0.0),
                                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                                    new LayerRegion(
                                        new DoubleRange(0.0, _layerThickness),
                                        new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                                    new LayerRegion(
                                        new DoubleRange(_layerThickness, 20.0),
                                        new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                                    new LayerRegion(
                                        new DoubleRange(20.0, double.PositiveInfinity),
                                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                                }
                ),
                detectors);
            _outputTwoLayerTissue = new MonteCarloSimulation(inputTwoLayer).Run();

            _factor = 1.0 - Optics.Specular(
                            inputOneLayerTissue.TissueInput.Regions[0].RegionOP.N,
                            inputOneLayerTissue.TissueInput.Regions[1].RegionOP.N);
        }

        // validation values obtained from linux run using above input and 
        // seeded the same for:
        // Diffuse Reflectance
        [Test]
        public void validate_DAW_RDiffuse()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.Rd * _factor - 0.565017749), 0.000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.Rd * _factor - 0.565017749), 0.000000001);
        }
        // Reflection R(rho)
        [Test]
        public void validate_DAW_ROfRho()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_r[0] * _factor - 0.615238307), 0.000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_r[0] * _factor - 0.615238307), 0.000000001);
        }
        // Reflection R(rho) 2nd moment, linux value output in printf statement
        [Test]
        public void validate_CAW_ROfRho_second_moment()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_r2[0] * _factor * _factor - 18.92598), 0.00001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_r2[0] * _factor * _factor - 18.92598), 0.00001);
        }
        // Reflection R(angle)
        [Test]
        public void validate_DAW_ROfAngle()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_a[0] * _factor - 0.0809612757), 0.0000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_a[0] * _factor - 0.0809612757), 0.0000000001);
        }
        // Reflection R(rho,angle)
        [Test]
        public void validate_DAW_ROfRhoAndAngle()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_ra[0, 0] * _factor - 0.0881573691), 0.0000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_ra[0, 0] * _factor - 0.0881573691), 0.0000000001);
        }
        // Reflection R(rho,time)
        [Test]
        public void validate_DAW_ROfRhoAndTime()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_rt[0, 0] * _factor - 61.5238307), 0.0000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_rt[0, 0] * _factor - 61.5238307), 0.0000001);
        }
        // Reflection R(rho,omega)
        public void validate_DAW_ROfRhoAndOmega()
        {
            Assert.Less(Complex.Abs(
                _outputOneLayerTissue.R_rw[0, 0] * _factor - (0.6152383 - Complex.ImaginaryOne * 0.0002368336)), 0.000001);
            Assert.Less(Complex.Abs(
                _outputTwoLayerTissue.R_rw[0, 0] * _factor - (0.6152383 - Complex.ImaginaryOne * 0.0002368336)), 0.000001);
        }
        // Total Absorption
        [Test]
        public void validate_DAW_ATotal()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.Atot * _factor - 0.384363881), 0.000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.Atot * _factor - 0.384363881), 0.000000001);
        }
        // Absorption A(rho,z)
        [Test]
        public void validate_DAW_AOfRhoAndZ()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.A_rz[0, 0] * _factor - 0.39494647), 0.00000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.A_rz[0, 0] * _factor - 0.39494647), 0.00000001);
        }
        // Diffuse Transmittance
        [Test]
        public void validate_DAW_TDiffuse()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.Td * _factor - 0.0228405921), 0.000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.Td * _factor - 0.0228405921), 0.000000001);
        }
        // Transmittance T(rho)
        [Test]
        public void validate_DAW_TOfRho()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.T_r[54] * _factor - 0.00169219067), 0.00000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.T_r[54] * _factor - 0.00169219067), 0.00000000001);
        }
        // Transmittance T(angle)
        [Test]
        public void validate_DAW_TOfAngle()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.T_a[0] * _factor - 0.00327282369), 0.00000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.T_a[0] * _factor - 0.00327282369), 0.00000000001);
        }
        // Transmittance T(rho,angle)
        [Test]
        public void validate_DAW_TOfRhoAndAngle()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.T_ra[54, 0] * _factor - 0.000242473649), 0.000000000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.T_ra[54, 0] * _factor - 0.000242473649), 0.000000000001);
        }
        // Fluence Flu(rho,z)
        [Test]
        public void validate_DAW_FluenceOfRhoAndZ()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.Flu_rz[0, 0] * _factor - 39.4946472), 0.0000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.Flu_rz[0, 0] * _factor - 39.4946472), 0.0000001);
        }
        // Reflectance R(x,y)
        [Test]
        public void validate_DAW_ROfXAndY()
        {
            Assert.Less(Math.Abs(_outputOneLayerTissue.R_xy[198, 201] * _factor - 0.00825301), 0.00000001);
            Assert.Less(Math.Abs(_outputTwoLayerTissue.R_xy[198, 201] * _factor - 0.00825301), 0.00000001);
        }
        // Dosimetry(rho)
        //[Test]
        //public void validate_DAW_DosimetryOfRho()
        //{
        // need radiance detector to compare results
        //    Assert.Less(Math.Abs(_outputOneLayerTissue.Dos_r[0] - _outputOneLayerTissue.Flu_rz[0, 5]), 0.0000001); 
        //}
    }
}
