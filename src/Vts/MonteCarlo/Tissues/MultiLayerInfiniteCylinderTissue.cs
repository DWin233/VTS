using System;
using System.Collections.Generic;
using System.Linq;
using Vts.Common;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueInput.  Defines input to MultiLayerInfiniteCylinderTissue class.
    /// This assumes air is above and below layered set of concentric infinite cylinders
    /// </summary>
    public class MultiLayerInfiniteCylinderTissueInput : TissueInput, ITissueInput
    {

        /// <summary>
        /// constructor for Multi-layer tissue input
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        public MultiLayerInfiniteCylinderTissueInput(ITissueRegion[] regions)
        {
            TissueType = "MultiLayerInfiniteCylinder";
            Regions = regions;
        }

        /// <summary>
        /// MultiLayerInfiniteCylinderTissue default constructor provides homogeneous tissue
        /// </summary>
        public MultiLayerInfiniteCylinderTissueInput()
            : this(
                new ITissueRegion[] // air - infinite cylinder - air
                { 
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        double.NegativeInfinity,
                        new OpticalProperties( 0.0, 1e-10, 1.0, 1.0)),
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(0.0, 10.0),
                        10,
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(10.0, 20),
                        10,
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                })
        {
        }

        /// <summary>
        /// list of tissue regions comprising tissue
        /// </summary>
        public ITissueRegion[] Regions { get; set; }

        /// <summary>
        /// Required factory method to create the corresponding 
        /// ITissue based on the ITissueInput data
        /// </summary>
        /// <param name="awt">Absorption Weighting Type</param>
        /// <param name="pft">Phase Function Type</param>
        /// <param name="russianRouletteWeightThreshold">Russian Roulette Weight Threshold</param>
        /// <returns>instantiated tissue class</returns>
        public ITissue CreateTissue(AbsorptionWeightingType awt, PhaseFunctionType pft, double russianRouletteWeightThreshold)
        {
            var t = new MultiLayerInfiniteCylinderTissue(Regions);

            t.Initialize(awt, pft, russianRouletteWeightThreshold);

            return t;
        }
    }

    /// <summary>
    /// Implements ITissue.  Defines tissue geometries comprised of layers
    /// (including homogenous with air layers above and below).  Layers are infinite along
    /// x- and y- axes.
    /// </summary>
    public class MultiLayerInfiniteCylinderTissue : TissueBase, ITissue
    {
        private readonly IList<InfiniteCylinderTissueRegion> _cylinderRegions;
        private readonly Position _center;

        /// <summary>
        /// Creates an instance of a MultiLayerInfiniteCylinderTissue
        /// </summary>
        /// <param name="regions">list of tissue regions comprising tissue</param>
        /// <remarks>air above and below tissue needs to be specified for a slab geometry</remarks>
        public MultiLayerInfiniteCylinderTissue(
            IList<ITissueRegion> regions)
            : base(regions)
        {
            _cylinderRegions = regions.Select(region => (InfiniteCylinderTissueRegion) region).ToArray();
            _center = _cylinderRegions[1].Center;
        }

        /// <summary>
        /// Creates a default instance of a MultiLayerInfiniteCylinderTissue based on a homogeneous medium slab geometry
        /// and discrete absorption weighting
        /// </summary>
        public MultiLayerInfiniteCylinderTissue() 
            : this(new MultiLayerInfiniteCylinderTissueInput().Regions)
        {
        }

        /// <summary>
        /// method to determine region index of region photon is currently in
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns>integer index of tissue region position is in</returns>
        public virtual int GetRegionIndex(Position position)
        {
            // use ITissueRegion interface method ContainsPosition for InfiniteCylinderTissueRegion to determine
            // which region photon resides

            var index = -1;
            // go from outside in because innermost will contain positions that outermost contain too
            for (var i = 0; i < _cylinderRegions.Count; i++)
            {
                if (_cylinderRegions[i].ContainsPosition(position))
                {
                    index = i;
                }
            }
            return index;
        }
        
        /// <summary>
        /// Finds the distance to the next boundary and independent of hitting it
        /// </summary>
        /// <param name="photon">photon</param>
        /// <returns>distance to boundary</returns>
        public virtual double GetDistanceToBoundary(Photon photon)
        {
            // then check if inclusion boundaries are closer
            var smallestInclusionDistance = double.PositiveInfinity;

            foreach (var inclusionRegion in _cylinderRegions)
            {
                inclusionRegion.RayIntersectBoundary(photon, out var distToInclusion);
                // first check that photon isn't sitting on boundary of one of the inclusions
                // note 1e-9 was found by trial and error using unit tests to verify selection
                // if you change value, need to update InclusionTissueRegion.ContainsPosition eps
                if (distToInclusion > 1e-9 && distToInclusion < smallestInclusionDistance)
                {
                    smallestInclusionDistance = distToInclusion;
                }
            }

            return smallestInclusionDistance;
        }

        /// <summary>
        /// method to determine if on boundary of tissue, i.e. at tissue/air interface
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns>Boolean indicating whether on boundary or not</returns>
        public virtual bool OnDomainBoundary(Position position)
        {
            // this code assumes that the first and last cylinder layer is air
            return _cylinderRegions[0].OnBoundary(position)
                   || _cylinderRegions.Last().OnBoundary(position);
        }

        /// <summary>
        /// method to determine index of region photon is about to enter
        /// </summary>
        /// <param name="photon">photon info including position and direction</param>
        /// <returns>region index</returns>
        public virtual int GetNeighborRegionIndex(Photon photon)
        {
            var currentCylinderIndex = GetRegionIndex(photon.DP.Position);
            var surfaceNormal = _cylinderRegions[currentCylinderIndex].SurfaceNormal(photon.DP.Position);
            var currentDirDotNormal = Direction.GetDotProduct(photon.DP.Direction, surfaceNormal);

            return currentDirDotNormal > 0
                    ? photon.CurrentRegionIndex - 1
                    : photon.CurrentRegionIndex + 1;
        }

        /// <summary>
        /// method to determine photon state type of photon exiting tissue boundary
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns>PhotonStateType</returns>
        public PhotonStateType GetPhotonDataPointStateOnExit(Position position)
        {
            // this code assumes that the first and last cylinder layer is air
            if (_cylinderRegions[0].OnBoundary(position)) 
                return PhotonStateType.PseudoReflectedTissueBoundary;
            if (_cylinderRegions.Last().OnBoundary(position)) 
                return PhotonStateType.PseudoTransmittedTissueBoundary;
            return PhotonStateType.None;
        }

        /// <summary>
        /// method to determine direction of reflected photon
        /// </summary>
        /// <param name="currentPosition">current photon position</param>
        /// <param name="currentDirection">current photon direction</param>
        /// <returns>direction of reflected photon</returns>
        public virtual Direction GetReflectedDirection(
            Position currentPosition, 
            Direction currentDirection)
        {
            var currentIndex = 0;
            // on boundary of an inclusion, check which one
            for (var i = 0; i < _cylinderRegions.Count; i++)
            {
                if (_cylinderRegions[i].ContainsPosition(currentPosition)) currentIndex = i;
            }
            // get neighbor index
            int neighborIndex;
            if (currentDirection.Uz > 0) 
            {
                if (currentPosition.Z < _center.Z) neighborIndex = currentIndex + 1;
                else neighborIndex = currentIndex - 1;
            }
            else
            {
                if (currentPosition.Z < _center.Z) neighborIndex = currentIndex - 1;
                else neighborIndex = currentIndex + 1;
            }
            
            if (_cylinderRegions[currentIndex].RegionOP.N == _cylinderRegions[neighborIndex].RegionOP.N)
            {
                return currentDirection;  // no refractive index mismatch
            }
            var currentCylinderIndex = GetRegionIndex(currentPosition);
            // reflection equation reflected = incident - 2(incident dot surfaceNormal)surfaceNormal
            var surfaceNormal = _cylinderRegions[currentCylinderIndex].SurfaceNormal(currentPosition);

            var currentDirDotNormal = Direction.GetDotProduct(currentDirection, surfaceNormal);
            var newX = currentDirection.Ux - 2 * currentDirDotNormal * surfaceNormal.Ux;
            var newY = currentDirection.Uy - 2 * currentDirDotNormal * surfaceNormal.Uy;
            var newZ = currentDirection.Uz - 2 * currentDirDotNormal * surfaceNormal.Uz;
            var norm = Math.Sqrt(newX * newX + newY * newY + newZ * newZ);
            return new Direction(newX / norm, newY / norm, newZ / norm);
        }

        /// <summary>
        /// method to determine refracted direction of photon
        /// </summary>
        /// <param name="currentPosition">current photon position</param>
        /// <param name="currentDirection">current photon direction</param>
        /// <param name="currentN">refractive index of current region</param>
        /// <param name="nextN">refractive index of next region</param>
        /// <param name="cosThetaSnell">cos(theta) resulting from Snell's law</param>
        /// <returns>direction</returns>
        public virtual Direction GetRefractedDirection(
            Position currentPosition, 
            Direction currentDirection, 
            double currentN, 
            double nextN, 
            double cosThetaSnell)
        {
            if (currentN == nextN)
            {
                return currentDirection;  // no refractive index mismatch
            }
            var currentCylinderIndex = GetRegionIndex(currentPosition);
            var normal = _cylinderRegions[currentCylinderIndex].SurfaceNormal(currentPosition);
            var cosTheta1 = Direction.GetDotProduct(currentDirection, normal);
            var nRatio = currentN / nextN;
            var sinTheta2Squared = nRatio * nRatio * (1 - cosTheta1 * cosTheta1);
            var factor = nRatio * cosTheta1 - Math.Sqrt(1 - sinTheta2Squared);
            var newX = nRatio * currentDirection.Ux + factor * normal.Ux;
            var newY = nRatio * currentDirection.Uy + factor * normal.Uy;
            var newZ = nRatio * currentDirection.Uz + factor * normal.Uz;
            var norm = Math.Sqrt(newX * newX + newY * newY + newZ * newZ);
            return new Direction(newX / norm, newY / norm, newZ / norm);
        }

        /// <summary>
        /// method to get cosine of the angle between photons current direction and boundary normal
        /// </summary>
        /// <param name="photon">photon</param>
        /// <returns>Uz=cos(theta)</returns>
        public virtual double GetAngleRelativeToBoundaryNormal(Photon photon)
        {
            var currentCylinderIndex = GetRegionIndex(photon.DP.Position);
            var normal = _cylinderRegions[currentCylinderIndex].SurfaceNormal(photon.DP.Position);
            return Math.Abs(Direction.GetDotProduct(photon.DP.Direction, normal)); // abs will work for upward normal and downward normal
        }
    }
}
