using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.Sources;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.DataStructuresValidation.TissueInputs
{
    [TestFixture]
    public class MultiLayerInfiniteCylinderTissueInputValidationTests
    {        
        /// <summary>
        /// Test to check that underlying MultiLayerInfiniteCylinderTissueInput is good
        /// </summary>
        [Test]
        public void Validate_underlying_multilayerinfinitecylinder_tissue_definition()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerInfiniteCylinderTissueInput(
                    new ITissueRegion[] // air-cylinder-cylinder-air
                    {
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            double.NegativeInfinity,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(0.0, 5.0),
                            20,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(5.0, 10.0),
                            15,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(10.0, 20),
                            10,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
            new List<IDetectorInput>() 
                {
                    new FluenceOfXAndYAndZDetectorInput()
                }
            );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsTrue(result.IsValid);
        }

        /// <summary>
        /// Test to check that infinite cylinders have non-zero axis definitions.
        /// </summary>
        [Test]
        public void Validate_infinite_cylinders_have_nonzero_radii()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerInfiniteCylinderTissueInput(
                    new ITissueRegion[] // air-cylinder-cylinder-air
                    {
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            double.NegativeInfinity,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(0.0, 5.0),
                            20,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(5.0, 10.0),
                            0,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(10.0, 20),
                            10,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IDetectorInput>()
                {
                    new FluenceOfXAndYAndZDetectorInput()
                }
            );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }

        /// <summary>
        /// Test to check that at least one tissue infinite cylinder layer is defined
        /// </summary>
        [Test]
        public void Validate_at_least_one_tissue_infinite_cylinder_layer_defined()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerInfiniteCylinderTissueInput(
                    new ITissueRegion[] // air-cylinder-cylinder-air
                    {
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            double.NegativeInfinity,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(0.0, 20),
                            10,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IDetectorInput>()
                {
                    new FluenceOfXAndYAndZDetectorInput()
                }
            );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }

        /// <summary>
        /// Test to check that infinite cylinders have same center
        /// </summary>
        [Test]
        public void Validate_infinite_cylinders_have_same_center()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerInfiniteCylinderTissueInput(
                    new ITissueRegion[] // air-cylinder-cylinder-air
                    {
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            double.NegativeInfinity,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(0.0, 5.0),
                            20,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(5.0, 10.0),
                            15,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(10.0, 20),
                            10,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IDetectorInput>() 
                {
                    new FluenceOfXAndYAndZDetectorInput()
                }
            );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsTrue(result.IsValid);
        }

        /// <summary>
        /// Test to check when infinite cylinders don't have same center
        /// </summary>
        [Test]
        public void Validate_infinite_cylinders_do_not_have_same_center()
        {
            var input = new SimulationInput(
                10,
                "",
                new SimulationOptions(),
                new DirectionalPointSourceInput(),
                new MultiLayerInfiniteCylinderTissueInput(
                    new ITissueRegion[] // air-cylinder-cylinder-air
                    {
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(double.NegativeInfinity, 0.0),
                            double.NegativeInfinity,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(0.0, 5.0),
                            20,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(5.0, 10.0),
                            10,
                            new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                        new InfiniteCylinderTissueRegion(
                            new DoubleRange(10.0, 20),
                            10,
                            new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                    }
                ),
                new List<IDetectorInput>() 
                {
                    new FluenceOfXAndYAndZDetectorInput() 
                }
            );
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }

    }
}
