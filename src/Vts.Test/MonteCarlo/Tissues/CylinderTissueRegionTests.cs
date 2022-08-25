﻿using System;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Tissues
{
    /// <summary>
    /// Unit tests for CylinderTissueRegion
    /// </summary>
    [TestFixture]
    public class CylinderTissueRegionTests
    {
        private CylinderTissueRegion _cylinderTissueRegion;
        /// <summary>
        /// Validate general constructor of TissueRegion
        /// </summary>
        [OneTimeSetUp]
        public void create_instance_of_class()
        {
            _cylinderTissueRegion = new CylinderTissueRegion(
               new Position(0, 0, 2), // center
                1.0, 
                2.0, // z goes from z=1 to 3
                new OpticalProperties(0.01, 1.0, 0.8, 1.4));
        }
        /// <summary>
        /// Validate general constructor of TissueRegion
        /// </summary>
        [Test]
        public void validate_cylinder_properties()
        {
            Assert.AreEqual(0.0, _cylinderTissueRegion.Center.X);
            Assert.AreEqual(0.0, _cylinderTissueRegion.Center.Y);
            Assert.AreEqual(2.0, _cylinderTissueRegion.Center.Z);
            Assert.AreEqual(1.0, _cylinderTissueRegion.Radius);
            Assert.AreEqual(2.0, _cylinderTissueRegion.Height);
            Assert.AreEqual(0.01, _cylinderTissueRegion.RegionOP.Mua);
            Assert.AreEqual(1.0, _cylinderTissueRegion.RegionOP.Musp);
            Assert.AreEqual(0.8, _cylinderTissueRegion.RegionOP.G);
            Assert.AreEqual(1.4, _cylinderTissueRegion.RegionOP.N);
        }

        /// <summary>
        /// Validate method OnBoundary return correct Boolean.
        /// Currently OnBoundary of an inclusion region isn't called by any code ckh 3/5/19.
        /// </summary>
        [Test]
        public void verify_OnBoundary_method_returns_correct_result()
        {
            // OnBoundary returns true if *exactly* on boundary
            bool result = _cylinderTissueRegion.OnBoundary(new Position(0, 0, 1.0)); // on top cap boundary
            Assert.IsTrue(result);
            result = _cylinderTissueRegion.OnBoundary(new Position(0, 0, 3.0)); // on bottom cap boundary
            Assert.IsTrue(result);
            result = _cylinderTissueRegion.OnBoundary(new Position(0, 0, 10.0)); // not on boundary
            Assert.IsFalse(result);
            // on cylinder
            result = _cylinderTissueRegion.OnBoundary(new Position(1.0/Math.Sqrt(2), 1.0/Math.Sqrt(2), 2.0)); 
            Assert.IsTrue(result);
        }
        /// <summary>
        /// Validate method ContainsPositions return correct Boolean. ContainsPosition is true if inside
        /// or *on* boundary.
        /// </summary>
        [Test]
        public void verify_ContainsPosition_method_returns_correct_result()
        {
            bool result = _cylinderTissueRegion.ContainsPosition(new Position(0, 0, 2.0)); // inside
            Assert.IsTrue(result);
            result = _cylinderTissueRegion.ContainsPosition(new Position(0, 0, 3.0)); // on boundary
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Validate method RayIntersectBoundary return correct result
        /// </summary>
        [Test]
        public void verify_RayIntersectBoundary_method_returns_correct_result()
        {
            // test intersection with sides
            Photon photon = new Photon();
            photon.DP.Position = new Position(0, 0, 2);
            photon.DP.Direction = new Direction(1, 0, 0);
            photon.S = 2.0; // definitely intersect sides
            double distanceToBoundary;
            bool result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1.0, distanceToBoundary);
            photon.S = 0.5; // definitely don't intersect sides
            result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(false, result);
            Assert.AreEqual(Double.PositiveInfinity, distanceToBoundary);
            photon.S = 1.0; // ends right at boundary => both out and no intersection
            result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(false, result);
            Assert.AreEqual(Double.PositiveInfinity, distanceToBoundary);
            // intersect cap of cylinder tests
            photon.DP.Position = new Position(0, 0, 0); // intersect top cap
            photon.DP.Direction = new Direction(0, 0, 1);  
            photon.S = 2.0; // make sure intersects top cap
            result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1.0, distanceToBoundary);
            photon.DP.Position = new Position(0, 0, 4); // intersect bottom cap
            photon.DP.Direction = new Direction(0, 0, -1);
            photon.S = 2.0; // make sure intersects top cap
            result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1.0, distanceToBoundary);
            photon.DP.Position = new Position(0, 0, 0); // intersect both
            photon.DP.Direction = new Direction(0, 0, 1);
            photon.S = 10.0; // make sure intersects both
            result = _cylinderTissueRegion.RayIntersectBoundary(photon, out distanceToBoundary);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1.0, distanceToBoundary);
        }
    }
}
