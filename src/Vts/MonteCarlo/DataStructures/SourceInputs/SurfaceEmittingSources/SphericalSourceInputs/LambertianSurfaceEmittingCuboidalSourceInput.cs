﻿using Vts.Common;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Interfaces;
using Vts.MonteCarlo.Sources.SourceProfiles;
using Vts.MonteCarlo.Sources;

namespace Vts.MonteCarlo.SourceInputs
{
    /// Implements ISourceInput. Defines input data for LambertianSurfaceEmittingSphericalSource 
    /// implementation including radius, position and initial tissue region index.
    public class LambertianSurfaceEmittingSphericalSourceInput : ISourceInput
    {
        /// <summary>
        /// Initializes a new instance of the LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="translationFromOrigin">New source location</param>
        /// <param name="initialTissueRegionIndex">Tissue region index</param>
        public LambertianSurfaceEmittingSphericalSourceInput(
            double radius,
            Position translationFromOrigin,
            int initialTissueRegionIndex)
        {
            SourceType = SourceType.LambertianSurfaceEmittingSpherical;
            Radius = radius;            
            TranslationFromOrigin = translationFromOrigin;
            InitialTissueRegionIndex = initialTissueRegionIndex;
        }

        /// <summary>
        /// Initializes a new instance of the LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        /// <param name="radius">The radius of the sphere</param>
        public LambertianSurfaceEmittingSphericalSourceInput(
            double radius)
            : this(
                radius,
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        /// <summary>
        /// Initializes a new instance of the LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        public LambertianSurfaceEmittingSphericalSourceInput()
            : this(
                1.0,
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public SourceType SourceType { get; set; }
        public double Radius { get; set; }
        public Position TranslationFromOrigin { get; set; }
        public int InitialTissueRegionIndex { get; set; }
    }
}