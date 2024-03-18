using System;
using System.Linq;
using Vts.Common;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.VirtualBoundaries
{
    /// <summary>
    /// Implements IVirtualBoundary.  Used to capture all diffuse transmittance infinite cylinder detectors
    /// </summary>
    public class DiffuseTransmittanceInfiniteCylinderVirtualBoundary : IVirtualBoundary
    {
        private ITissue _tissue;
        /// <summary>
        /// diffuse reflectance VB
        /// </summary>
        /// <param name="tissue">ITissue</param>
        /// <param name="detectorController">IDetectorController</param>
        /// <param name="name">string name</param>
        public DiffuseTransmittanceInfiniteCylinderVirtualBoundary(ITissue tissue, IDetectorController detectorController, string name)
        {
            var airInnermostCylinder = (InfiniteCylinderTissueRegion)tissue.Regions.Last();

            WillHitBoundary = dp =>
                dp.StateFlag.HasFlag(PhotonStateType.PseudoTransmittedInfiniteCylinderTissueBoundary) &&
                airInnermostCylinder.ContainsPosition(dp.Position);

            VirtualBoundaryType = VirtualBoundaryType.DiffuseTransmittanceInfiniteCylinder;
            PhotonStateType = PhotonStateType.PseudoDiffuseTransmittanceInfiniteCylinderVirtualBoundary;

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
            var innerCylinder = (InfiniteCylinderTissueRegion)_tissue.Regions[^1];

            var distanceToBoundary = double.PositiveInfinity;
            // check if VB not applied
            if (!dp.StateFlag.HasFlag(PhotonStateType.PseudoTransmittedInfiniteCylinderTissueBoundary))
            {
                return distanceToBoundary;
            }
            // VB applies to outermost "tissue" cylinder
            // determine location of end of ray
            var dp2 = new Position(dp.Position.X + dp.Direction.Ux * double.PositiveInfinity,
                dp.Position.Y + dp.Direction.Uy * double.PositiveInfinity,
                dp.Position.Z + dp.Direction.Uz * double.PositiveInfinity);

            CylinderTissueRegionToolbox.RayIntersectInfiniteCylinder(
                dp.Position,
                dp2,
                true,
                CylinderTissueRegionAxisType.Y,
                innerCylinder.Center,
                innerCylinder.Radius,
                out distanceToBoundary);
            return distanceToBoundary;
        }
    }
}
