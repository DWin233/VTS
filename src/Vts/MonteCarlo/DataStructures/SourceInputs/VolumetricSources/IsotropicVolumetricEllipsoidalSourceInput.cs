﻿using Vts.Common;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Interfaces;
using Vts.MonteCarlo.Sources.SourceProfiles;

namespace Vts.MonteCarlo.Sources
{
    public class IsotropicVolumetricEllipsoidalSourceInput : ISourceInput
    {
        // this handles isotropic ellipsoidal (volumetric)
        public IsotropicVolumetricEllipsoidalSourceInput(
            double aParameter,
            double bParameter,
            double cParameter,
            ISourceProfile sourceProfile,
            Direction newDirectionOfPrincipalSourceAxis,
            Position translationFromOrigin,
            int initialTissueRegionIndex)
        {
            SourceType = SourceType.IsotropicVolumetricEllipsoidal;
            AParameter = aParameter;
            BParameter = bParameter;
            CParameter = cParameter;
            SourceProfile = sourceProfile;
            NewDirectionOfPrincipalSourceAxis = newDirectionOfPrincipalSourceAxis;
            TranslationFromOrigin = translationFromOrigin;
            InitialTissueRegionIndex = initialTissueRegionIndex;
        }

        public IsotropicVolumetricEllipsoidalSourceInput(
            double aParameter,
            double bParameter,
            double cParameter,
            ISourceProfile sourceProfile)
            : this(
                aParameter,
                bParameter,
                cParameter,
                sourceProfile,
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public IsotropicVolumetricEllipsoidalSourceInput()
            : this(
                1.0,
                1.0,
                2.0,
                new FlatSourceProfile(),
                SourceDefaults.DefaultDirectionOfPrincipalSourceAxis.Clone(),
                SourceDefaults.DefaultPosition.Clone(),
                0) { }

        public SourceType SourceType { get; set; }
        public double AParameter { get; set; }
        public double BParameter { get; set; }
        public double CParameter { get; set; }
        public ISourceProfile SourceProfile { get; set; }     
        public Direction NewDirectionOfPrincipalSourceAxis { get; set; }
        public Position TranslationFromOrigin { get; set; }
        public int InitialTissueRegionIndex { get; set; }
    }
}