﻿using System;
using Vts.Common;
using Vts.MonteCarlo.Helpers;

namespace Vts.MonteCarlo.Sources
{
    /// <summary>
    /// Implements ISourceInput. Defines input data for LambertianSurfaceEmittingSphericalSource 
    /// implementation including radius, position and initial tissue region index.
    /// </summary>
    public class LambertianSurfaceEmittingSphericalSourceInput : ISourceInput
    {
        /// <summary>
        /// Initializes a new instance of LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="translationFromOrigin">New source location</param>
        /// <param name="initialTissueRegionIndex">Initial tissue region index</param>
        public LambertianSurfaceEmittingSphericalSourceInput(
            double radius,
            Position translationFromOrigin,
            int initialTissueRegionIndex)
        {
            SourceType = "LambertianSurfaceEmittingSpherical";
            Radius = radius;
            TranslationFromOrigin = translationFromOrigin;
            InitialTissueRegionIndex = initialTissueRegionIndex;
        }

        /// <summary>
        /// Initializes a new instance of LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        /// <param name="radius">The radius of the sphere</param>
        public LambertianSurfaceEmittingSphericalSourceInput(
            double radius)
            : this(
                radius,
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        /// <summary>
        /// Initializes the default constructor of LambertianSurfaceEmittingSphericalSourceInput class
        /// </summary>
        public LambertianSurfaceEmittingSphericalSourceInput()
            : this(
                1.0,
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        /// <summary>
        /// Spherical source type
        /// </summary>
        public string SourceType { get; set; }
        /// <summary>
        /// The radius of the sphere
        /// </summary>
        public double Radius { get; set; }
        /// <summary>
        /// New source location
        /// </summary>
        public Position TranslationFromOrigin { get; set; }
        /// <summary>
        /// Initial tissue region index
        /// </summary>
        public int InitialTissueRegionIndex { get; set; }

        /// <summary>
        /// Required code to create a source based on the input values
        /// </summary>
        /// <param name="rng"></param>
        /// <returns></returns>
        public ISource CreateSource(Random rng = null)
        {
            rng = rng ?? new Random();

            return new LambertianSurfaceEmittingSphericalSource(
                this.Radius,
                this.TranslationFromOrigin,
                this.InitialTissueRegionIndex) { Rng = rng };
        }        
    }

    /// <summary>
    /// Implements LambertianSurfaceEmittingSphericalSource with radius, position and
    /// initial tissue region index.
    /// </summary>
    public class LambertianSurfaceEmittingSphericalSource : SurfaceEmittingSphericalSourceBase
    {
        /// <summary>
        /// Returns an instance of Lambertian Spherical Surface Emitting Source with a specified translation.
        /// </summary>
        /// <param name="radius">The radius of the sphere</param> 
        /// <param name="translationFromOrigin">New source location</param>
        /// <param name="initialTissueRegionIndex">Initial tissue region index</param>
        public LambertianSurfaceEmittingSphericalSource(
            double radius,
            Position translationFromOrigin = null,
            int initialTissueRegionIndex = 0)
            : base(
                radius,
                SourceDefaults.DefaultFullPolarAngleRange.Clone(),
                SourceDefaults.DefaultAzimuthalAngleRange.Clone(),
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                translationFromOrigin,
                initialTissueRegionIndex)
        {
            if (translationFromOrigin == null)
                translationFromOrigin = SourceDefaults.DefaultPosition.Clone();
        }
            
        /// <summary>
        /// Returns Direction
        /// </summary>
        /// <param name="direction">initial direction</param>
        /// <param name="position">initial position</param>
        /// <returns></returns>
        protected override Direction GetFinalDirection(Direction direction, Position position)
        {
            //Lambertian distribution 
            PolarAzimuthalAngles polarAzimuthalPair = SourceToolbox.GetPolarAzimuthalPairForLambertianRandom(Rng);

            //Use a dummy variable to avoid update the position during next rotation
            var dummyPosition = new Position(position.X, position.Y, position.Z);

            //Rotate polar azimuthal angle by polarAzimuthalPair vector
            SourceToolbox.UpdateDirectionPositionAfterRotatingByGivenAnglePair(polarAzimuthalPair, ref direction, ref dummyPosition);

            return direction;
        }              
    }    
}
