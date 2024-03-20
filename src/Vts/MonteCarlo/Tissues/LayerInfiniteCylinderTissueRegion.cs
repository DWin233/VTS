using System;
using Vts.Common;

namespace Vts.MonteCarlo.Tissues
{
    /// <summary>
    /// Implements ITissueRegion.  Defines cylindrical region infinite along y-axis with center at (xc,zc)
    /// but finite along z-axis.
    /// </summary>
    public class LayerInfiniteCylinderTissueRegion : ITissueRegion
    {
        private bool _onBoundary;

        /// <summary>
        /// Defines a finite infinite cylinder layer
        /// </summary>
        /// <param name="center"></param>
        /// <param name="outerRadius"></param>
        /// <param name="innerRadius"></param>
        /// <param name="op"></param>
        public LayerInfiniteCylinderTissueRegion(Position center, double outerRadius, double innerRadius, OpticalProperties op)
        {
            TissueRegionType = "LayerInfiniteCylinder";
            OuterRadius = outerRadius;
            InnerRadius = innerRadius;
            RegionOP = op;
            // determine Center from radius and zRange
            Center = center;
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public LayerInfiniteCylinderTissueRegion() : this(new (0, 0, 2), 2, 0,
            new OpticalProperties(0.01, 1.0, 0.8, 1.4)) {}

        /// <summary>
        /// tissue region identifier
        /// </summary>
        public string TissueRegionType { get; set; }

        /// <summary>
        /// center of cylinder
        /// </summary>
        public Position Center { get; set; }
        /// <summary>
        /// radius of outer cylinder boundary
        /// </summary>
        public double OuterRadius { get; set; }
        /// <summary>
        /// radius of inner cylinder boundary
        /// </summary>
        public double InnerRadius { get; set; }
        /// <summary>
        /// optical properties of cylinder
        /// </summary>
        public OpticalProperties RegionOP { get; set; }
        
        /// <summary>
        /// Method to determine if photon position within or on cylinder.  The loss of precision in floating
        /// point operations necessitates the checks of if "inside" is close but not exact
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns>Boolean</returns>
        public bool ContainsPosition(Position position)
        {
            var inOuter = false;
            // wrote following to give "buffer" of error
            var deltaOuterRadius = Math.Sqrt((position.X - Center.X) * (position.X - Center.X) +
                                   (position.Z - Center.Z) * (position.Z - Center.Z)) - OuterRadius;
            // check if inside outer
            switch (deltaOuterRadius)
            {
                // the epsilon needs to match MultiConcentricLayerInfiniteCylinder
                // GetDistanceToBoundary or code goes through cycles at cylinder boundary            
                case < -1e-9:
                    inOuter = true;
                    break;
                case > 1e-9:
                    break; // inOuter = false
                default:
                    _onBoundary = true;
                    inOuter = true;
                    break;
            }

            if (inOuter) // check if outside of inner
            {
                var deltaInnerRadius = Math.Sqrt((position.X - Center.X) * (position.X - Center.X) +
                                            (position.Z - Center.Z) * (position.Z - Center.Z)) - InnerRadius;
                switch (deltaInnerRadius)
                {         
                    case < -1e-9:
                        return false; // within both radii
                    case > 1e-9:
                        return true;
                    default:
                        _onBoundary = true;
                        return true;  
                }
            }

            return false;
        }

        /// <summary>
        /// Method to determine if photon on boundary of infinite cylinder.
        /// Currently OnBoundary of an inclusion region isn't called by any code ckh 3/5/19.
        /// </summary>
        /// <param name="position">photon position</param>
        /// <returns>Boolean</returns>
        public bool OnBoundary(Position position)
        {
            return !ContainsPosition(position) && _onBoundary; // match with EllipsoidTissueRegion
        }

        /// <summary>
        /// method to determine normal to surface at given position. Note this returns outward facing normal
        /// from OuterRadius.
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>Direction normal to surface at position</returns>
        public Direction SurfaceNormal(Position position)
        {
            var dx = position.X - Center.X;
            var dz = position.Z - Center.Z;
            var norm = Math.Sqrt(dx * dx + dz * dz);
            return new Direction(dx / norm, 0, dz / norm);
        }

        /// <summary>
        /// Method to determine if photon ray (or track) will intersect boundary of cylinder
        /// equations to determine intersection are derived by parameterizing ray from p1 to p2
        /// as p2=p1+[dx dy dz]t t in [0,1] where dx=p2.x-p1.x dy=p2.y-p1.y dz=p2.z-p2.z
        /// and substituting into ellipsoid equations and solving quadratic in t, i.e. t1, t2
        /// t1,t2 less than 0 or t1,t2 greater than 1 => no intersection
        /// 0 less than t1 less than 1 => one intersection
        /// 0 less than t2 less than 1 => one intersections, if above line true too => two intersections
        /// Equations obtained from pdf at https://mrl.nyu.edu/~dzorin/rendering/lectures/lecture3/lecture3-6pp.pdf
        /// and modified to assume cylinder infinite along y-axis
        /// </summary>
        /// <param name="photon">photon position, direction, etc.</param>
        /// <param name="distanceToBoundary">return: distance to boundary</param>
        /// <returns>Boolean</returns>
        public bool RayIntersectBoundary(Photon photon, out double distanceToBoundary)
        {
            distanceToBoundary = double.PositiveInfinity;
            _onBoundary = false; // reset _onBoundary
            var dp = photon.DP;
            var p1 = dp.Position;
            var d1 = dp.Direction;

            // determine location of end of ray
            var p2 = new Position(p1.X + d1.Ux * photon.S,
                                  p1.Y + d1.Uy * photon.S,
                                  p1.Z + d1.Uz * photon.S);

            var oneIn = this.ContainsPosition(p1);
            var twoIn = this.ContainsPosition(p2);

            // check if ray within cylinder
            if ((oneIn || _onBoundary) && twoIn)
            {
                return false;
            }
            _onBoundary = false; // reset flag

            // FIX!
            return true;
            //return (CylinderTissueRegionToolbox.RayIntersectLayerInfiniteCylinder(p1, p2, oneIn,
            //    CylinderTissueRegionAxisType.Y, Center, Radius,
            //    out distanceToBoundary));
        }
    }
}
