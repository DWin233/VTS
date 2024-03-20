using System;
using MathNet.Numerics.Random;
using Vts.Common;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;
using Vts.SpectralMapping;

namespace Vts.MonteCarlo.VirtualBoundaries
{
    /// <summary>
    /// Implements IVirtualBoundary.  Used to capture all diffuse reflectance infinite cylinder detectors
    /// </summary>
    public class DiffuseReflectanceInfiniteCylinderVirtualBoundary : IVirtualBoundary
    {
        private readonly ITissue _tissue;
        private readonly ITissueRegion _airAroundOuterCylinder;
        /// <summary>
        /// diffuse reflectance VB
        /// </summary>
        /// <param name="tissue">ITissue</param>
        /// <param name="detectorController">IDetectorController</param>
        /// <param name="name">string name</param>
        public DiffuseReflectanceInfiniteCylinderVirtualBoundary(ITissue tissue, IDetectorController detectorController, string name)
        {
            _airAroundOuterCylinder = (InfiniteCylinderTissueRegion)tissue.Regions[0];

            WillHitBoundary = dp =>
                dp.StateFlag.HasFlag(PhotonStateType.PseudoReflectedInfiniteCylinderTissueBoundary) &&
                _airAroundOuterCylinder.ContainsPosition(dp.Position);

            VirtualBoundaryType = VirtualBoundaryType.DiffuseReflectanceInfiniteCylinder;
            PhotonStateType = PhotonStateType.PseudoDiffuseReflectanceInfiniteCylinderVirtualBoundary;

            DetectorController = detectorController;

            Name = name;
            _tissue = tissue;
        }

        /// <summary>
        /// VirtualBoundaryType enum indicating type of VB
        /// </summary>
        public VirtualBoundaryType VirtualBoundaryType { get; }
        /// <summary>
        /// PhotonStateType enum indicating state of photon at this VB
        /// </summary>
        public PhotonStateType PhotonStateType { get; }
        /// <summary>
        /// Name string of VB
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Predicate method to indicate if photon will hit VB boundary
        /// </summary>
        public Predicate<PhotonDataPoint> WillHitBoundary { get; }
        /// <summary>
        /// IDetectorController specifying type of detector controller.
        /// </summary>
        public IDetectorController DetectorController { get; }

        /// <summary>
        /// finds distance to VB
        /// </summary>
        /// <param name="dp">PhotonDataPoint</param>
        /// <returns>distance to VB</returns>
        public double GetDistanceToVirtualBoundary(PhotonDataPoint dp)
        {
            var outerCylinder = (InfiniteCylinderTissueRegion)_tissue.Regions[1];

            var distanceToBoundary = double.PositiveInfinity;
            // check if VB not applied
            if (!dp.StateFlag.HasFlag(PhotonStateType.PseudoReflectedInfiniteCylinderTissueBoundary))
            {
                return distanceToBoundary;
            }
            // check if in innermost air cylinder 
            if (_airAroundOuterCylinder.ContainsPosition(dp.Position)) return 0.0;

            // VB applies to outermost "tissue" cylinder
            // determine location of end of long ray
            var S = outerCylinder.Radius;
            var dp2 = new Position(dp.Position.X + dp.Direction.Ux * S,
                dp.Position.Y + dp.Direction.Uy * S,
                dp.Position.Z + dp.Direction.Uz * S);

            CylinderTissueRegionToolbox.RayIntersectInfiniteCylinder(
                    dp.Position, 
                    dp2, 
                    true,
                CylinderTissueRegionAxisType.Y, 
                outerCylinder.Center,
                outerCylinder.Radius,
                out distanceToBoundary);
            return distanceToBoundary;
        }
    }
}
