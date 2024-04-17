using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Vts.Common;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueInput.  Defines input to MultiConcentricInfiniteCylinderTissue class.
    /// This assumes infinite cylinders are concentric and lie entirely within a single layer of tissue.
    /// By convention largest cylinder defined first in list of cylinders
    /// </summary>
    public class MultiConcentricInfiniteCylinderTissueInput : TissueInput, ITissueInput
    {
        private ITissueRegion[] _infiniteCylinderRegions;
        private ITissueRegion[] _layerRegions;

        /// <summary>
        /// constructor for Multi-ConcentricInfiniteCylinder tissue input
        /// </summary>
        /// <param name="infiniteCylinderRegions">concentric cylinder regions, larger radius first</param>
        /// <param name="layerRegions">layer regions</param>
        public MultiConcentricInfiniteCylinderTissueInput(
            ITissueRegion[] infiniteCylinderRegions,
            ITissueRegion[] layerRegions)
        {
            TissueType = "MultiConcentricInfiniteCylinder";
            LayerRegions = layerRegions;
            InfiniteCylinderRegions = infiniteCylinderRegions;
            RegionPhaseFunctionInputs = new Dictionary<string, IPhaseFunctionInput>();
            Regions = LayerRegions.Concat(InfiniteCylinderRegions).ToArray();
        }

        /// <summary>
        /// MultiConcentricInfiniteCylinderTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiConcentricInfiniteCylinderTissueInput()
            : this(
                new ITissueRegion[]
                {
                    new InfiniteCylinderTissueRegion(
                        new Position(0, 0, 1),
                        0.75,
                        new OpticalProperties(0.05, 1.0, 0.8, 1.4),
                        "HenyeyGreensteinKey4"
                    ),
                    new InfiniteCylinderTissueRegion(
                        new Position(0, 0, 1),
                        0.5,
                        new OpticalProperties(0.05, 1.0, 0.8, 1.4),
                        "HenyeyGreensteinKey5"
                    )
                },
                new ITissueRegion[]
                {
                    new LayerTissueRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        new OpticalProperties( 0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey1"),
                    new LayerTissueRegion(
                        new DoubleRange(0.0, 100.0),
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4),
                        "HenyeyGreensteinKey2"),
                    new LayerTissueRegion(
                        new DoubleRange(100.0, double.PositiveInfinity),
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0),
                        "HenyeyGreensteinKey3")
                })
        {
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey1", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey2", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey3", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey4", new HenyeyGreensteinPhaseFunctionInput());
            RegionPhaseFunctionInputs.Add("HenyeyGreensteinKey5", new HenyeyGreensteinPhaseFunctionInput());
        }

        /// <summary>
        /// list of tissue regions comprising tissue
        /// </summary>
        [IgnoreDataMember]
        public ITissueRegion[] Regions { get; private set; }

        /// <summary>
        /// tissue layer regions
        /// </summary>
        public ITissueRegion[] LayerRegions
        {
            get => _layerRegions;
            set
            {
                _layerRegions = value;
                if (InfiniteCylinderRegions != null) Regions = _layerRegions.Concat(InfiniteCylinderRegions).ToArray();
            }
        }
        /// <summary>
        /// tissue outer infinite cylinder region
        /// </summary>
        public ITissueRegion[] InfiniteCylinderRegions
        {
            get => _infiniteCylinderRegions;
            set
            {
                _infiniteCylinderRegions = value;
                if (LayerRegions != null) Regions = LayerRegions.Concat(_infiniteCylinderRegions).ToArray();
            }
        }

        /// <summary>
        /// dictionary of region phase function inputs
        /// </summary>
        public IDictionary<string, IPhaseFunctionInput> RegionPhaseFunctionInputs { get; set; }

        /// <summary>
        /// Required factory method to create the corresponding 
        /// ITissue based on the ITissueInput data
        /// </summary>
        /// <param name="awt">Absorption Weighting Type</param>
        /// <param name="regionPhaseFunctions">Phase Function dictionary</param>
        /// <param name="russianRouletteWeightThreshold">Russian Roulette Weight Threshold</param>
        /// <returns>instantiated tissue</returns>
        public ITissue CreateTissue(AbsorptionWeightingType awt, IDictionary<string, IPhaseFunction> regionPhaseFunctions, double russianRouletteWeightThreshold)
        {
            var t = new MultiConcentricInclusionTissue(InfiniteCylinderRegions, LayerRegions);

            t.Initialize(awt, regionPhaseFunctions, russianRouletteWeightThreshold);

            return t;
        }
    }
}
