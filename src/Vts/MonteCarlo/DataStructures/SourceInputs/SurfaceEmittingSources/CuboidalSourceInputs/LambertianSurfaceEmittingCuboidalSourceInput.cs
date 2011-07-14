﻿using Vts.Common;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Interfaces;
using Vts.MonteCarlo.Sources.SourceProfiles;

namespace Vts.MonteCarlo.Sources
{
    public class LambertianSurfaceEmittingCuboidalSourceInput : ISourceInput
    {
        // this handles custom circular
        public LambertianSurfaceEmittingCuboidalSourceInput(
            double cubeLengthX,
            double cubeWidthY,
            double cubeHeightZ,
            ISourceProfile sourceProfile,
            Direction newDirectionOfPrincipalSourceAxis,
            Position translationFromOrigin,
            int initialTissueRegionIndex)
        {
            SourceType = SourceType.LambertianSurfaceEmittingCubiodal;
            CubeLengthX = cubeLengthX;
            CubeWidthY = cubeWidthY;
            CubeHeightZ = cubeHeightZ;
            SourceProfile = sourceProfile;          
            NewDirectionOfPrincipalSourceAxis = newDirectionOfPrincipalSourceAxis;
            TranslationFromOrigin = translationFromOrigin;
            InitialTissueRegionIndex = initialTissueRegionIndex;
        }

        public LambertianSurfaceEmittingCuboidalSourceInput(
            double cubeLengthX,
            double cubeWidthY,
            double cubeHeightZ,
            ISourceProfile sourceProfile)
            : this(
                cubeLengthX,
                cubeWidthY,
                cubeHeightZ,
                sourceProfile,
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public LambertianSurfaceEmittingCuboidalSourceInput()            
            : this(
                1.0,
                1.0,
                1.0,
                new FlatSourceProfile(),
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public SourceType SourceType { get; set; }
        public double CubeLengthX { get; set; }
        public double CubeWidthY { get; set; }
        public double CubeHeightZ { get; set; }
        public ISourceProfile SourceProfile { get; set; }    
        public Direction NewDirectionOfPrincipalSourceAxis { get; set; }
        public Position TranslationFromOrigin { get; set; }
        public int InitialTissueRegionIndex { get; set; }
    }
}