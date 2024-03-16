using System.Collections.Generic;
using System.Linq;
using Vts.MonteCarlo.DataStructuresValidation;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo
{
    /// <summary>
    /// This verifies that the infinite cylinders are concentric and have air above and inside
    /// </summary>
    public static class MultiLayerInfiniteCylinderTissueInputValidation
    {
        /// <summary>
        /// Main validation method for MultiLayerInfiniteCylinderTissueInput.
        /// </summary>
        /// <param name="input">tissue input defined in SimulationInput</param>
        /// <returns>An instance of the ValidationResult class</returns>
        public static ValidationResult ValidateInput(ITissueInput input)
        {
            var cylinders = ((MultiLayerInfiniteCylinderTissueInput)input).Regions.Select(
                region => (InfiniteCylinderTissueRegion)region).ToArray();
            var tempResult = ValidateGeometry(cylinders);
            return tempResult;
        }

        /// <summary>
        /// Method to validate that the geometry of tissue infinite cylinders agree with capabilities
        /// of code.
        /// </summary>
        /// <param name="infiniteCylinders">List of InfiniteCylinderTissueRegion</param>
        /// <returns>An instance of the ValidationResult class</returns>
        private static ValidationResult ValidateGeometry(IList<InfiniteCylinderTissueRegion> infiniteCylinders)
        {            
            if (infiniteCylinders.Any(region => region.Radius == 0.0))
            {
                return new ValidationResult(
                    false,
                    "MultiLayerInfiniteCylinderTissueInput: one infinite cylinder has radius equal to 0",
                    "MultiLayerInfiniteCylinderTissueInput: make sure infinite cylinder radii are > 0");
            }

            // test for air layers
            var airLayers = infiniteCylinders.Where(layer => layer.IsAir()).ToArray();
            if (!airLayers.Any())
            {
                return new ValidationResult(
                    false,
                    "MultiLayerInfiniteCylinderTissueInput: tissue assumed to be defined with air layer above and below",
                    "MultiLayerInfiniteCylinderTissueInput: redefine tissue definition to have outermost and innermost cylinders of air");

            }

            // check that there is at least one layer of tissue 
            if (airLayers.Length == infiniteCylinders.Count)
            {
                return new ValidationResult(
                    false,
                    "MultiLayerInfiniteCylinderTissueInput: tissue layer is assumed to be at least a single layer with air layer above and below",
                    "MultiLayerInfiniteCylinderTissueInput: redefine tissue definition to contain at least a single layer of tissue");
            }

            // check that infinite cylinders all have same Center
            var theCenter = infiniteCylinders[1].Center;

            foreach (var cylinder in infiniteCylinders.Skip(1))
            {
                if (cylinder.Center != theCenter)
                {
                    return new ValidationResult(
                    false,
                    "MultiLayerInfiniteCylinderTissueInput: infinite cylinders are not concentric",
                    "MultiLayerInfiniteCylinderTissueInput: set Center of each to be the same");
                }
            }

            return new ValidationResult(
                true,
                "MultiLayerInfiniteCylinderTissueInput: geometry and refractive index settings validated");
        }
    }
}
