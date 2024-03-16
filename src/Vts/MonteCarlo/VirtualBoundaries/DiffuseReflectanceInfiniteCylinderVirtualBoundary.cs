using System;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.VirtualBoundaries
{
    /// <summary>
    /// Implements IVirtualBoundary.  Used to capture all diffuse reflectance infinite cylinder detectors
    /// </summary>
    public class DiffuseReflectanceInfiniteCylinderVirtualBoundary : IVirtualBoundary
    {
        /// <summary>
        /// diffuse reflectance VB
        /// </summary>
        /// <param name="tissue">ITissue</param>
        /// <param name="detectorController">IDetectorController</param>
        /// <param name="name">string name</param>
        public DiffuseReflectanceInfiniteCylinderVirtualBoundary(ITissue tissue, IDetectorController detectorController, string name)
        {
            var airAroundOuterCylinder = (InfiniteCylinderTissueRegion)tissue.Regions[0];

            WillHitBoundary = dp =>
                dp.StateFlag.HasFlag(PhotonStateType.PseudoReflectedInfiniteCylinderTissueBoundary) &&
                airAroundOuterCylinder.ContainsPosition(dp.Position);

            VirtualBoundaryType = VirtualBoundaryType.DiffuseReflectanceInfiniteCylinder;
            PhotonStateType = PhotonStateType.PseudoDiffuseReflectanceInfiniteCylinderVirtualBoundary;

            DetectorController = detectorController;

            Name = name;
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
            const double distanceToBoundary = double.PositiveInfinity;
            // check if VB not applied
            return !dp.StateFlag.HasFlag(PhotonStateType.PseudoReflectedInfiniteCylinderTissueBoundary) ? distanceToBoundary : distanceToBoundary;
        }
    }
}
