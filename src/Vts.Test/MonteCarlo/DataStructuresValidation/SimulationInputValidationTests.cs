using System;
using System.Collections.Generic;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;

namespace Vts.Test.MonteCarlo.DataStructuresValidation
{
    [TestFixture]
    public class SimulationInputValidationTests
    {
        [Test]
        public void validate_null_detector_input_is_invalid_when_no_database_specified()
        {
            // generate input without any detector inputs and no database specified
            var input = new SimulationInput()  // default constructor has empty list of databases
            {
                DetectorInputs = new List<IDetectorInput> {}
            };
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }
        [Test]
        public void validate_null_detector_input_is_valid_when_database_specified()
        {
            // generate input without any detector inputs but with database specified
            var input = new SimulationInput()  
            {
                DetectorInputs = new List<IDetectorInput> {}
            };
            input.Options.Databases = new List<DatabaseType> {DatabaseType.DiffuseReflectance};
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsTrue(result.IsValid);
        }
        [Test]
        public void validate_detector_input_not_implemented_is_invalid()
        {
            // generate input with detector input not implemented yet
            var input = new SimulationInput()
            {
                Options = new SimulationOptions() { AbsorptionWeightingType = AbsorptionWeightingType.Continuous},
                DetectorInputs = new List<IDetectorInput> { new RadianceOfXAndYAndZAndThetaAndPhiDetectorInput()}
            };
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }
        [Test]
        public void validate_angled_source_and_cylindrical_detectors_are_not_defined_together()
        {
            // generate input with angled source and cylindrical detector
            var input = new SimulationInput()
            {
                SourceInput = new DirectionalPointSourceInput(
                    new Position(0,0,0), new Direction(1.0/Math.Sqrt(2), 0, 1.0/Math.Sqrt(2)),1),
                DetectorInputs = new List<IDetectorInput> { new ROfRhoDetectorInput() }
            };
            var result = SimulationInputValidation.ValidateInput(input);
            Assert.IsFalse(result.IsValid);
        }
    }
}
