﻿using System;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Tissues
{
    /// <summary>
    /// Unit tests for BoundingVoxelTissue and BoundingVoxelTissue
    /// </summary>
    [TestFixture]
    public class BoundingVoxelTissueTests
    {
        private BoundedTissue _oneLayerTissue, _twoLayerTissue;
        /// <summary>
        /// Validate general constructor of Tissue for a one layer and two layer tissue voxel
        /// </summary>
        [OneTimeSetUp]
        public void create_instance_of_class()
        {
            _oneLayerTissue = new BoundedTissue(new CaplessVoxelTissueRegion(
                new DoubleRange(-1, 1, 2), // x range
                new DoubleRange(-1, 1, 2), // y range
                new DoubleRange(0, 100.0, 2),  // z range spans tissue
                new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                new ITissueRegion[]
                {
                    new LayerTissueRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        new OpticalProperties( 0.0, 1e-10, 1.0, 1.0)),
                    new LayerTissueRegion(
                        new DoubleRange(0.0, 100.0),
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new LayerTissueRegion(
                        new DoubleRange(100.0, double.PositiveInfinity),
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                });
            _twoLayerTissue = new BoundedTissue(new CaplessVoxelTissueRegion(
                new DoubleRange(-1, 1, 2), // x range
                new DoubleRange(-1, 1, 2), // y range
                new DoubleRange(0, 100.0, 2),  // z range spans tissue
                new OpticalProperties(0.01, 1.0, 0.8, 1.4)),
                new ITissueRegion[]
                {
                    new LayerTissueRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        new OpticalProperties( 0.0, 1e-10, 1.0, 1.0)),
                    new LayerTissueRegion(
                        new DoubleRange(0.0, 1.0),
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new LayerTissueRegion(
                        new DoubleRange(1.0, 100.0),
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new LayerTissueRegion(
                        new DoubleRange(100.0, double.PositiveInfinity),
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                });
        }

        /// <summary>
        /// Validate method GetRegionIndex return correct Boolean
        /// </summary>
        [Test]
        public void verify_GetRegionIndex_method_returns_correct_result()
        {
            int index = _oneLayerTissue.GetRegionIndex(new Position(10, 0, 0)); // outside voxel
            Assert.AreEqual(3, index);
            index = _oneLayerTissue.GetRegionIndex(new Position(0, 0, 2.5)); // inside voxel
            Assert.AreEqual(1, index);
            index = _oneLayerTissue.GetRegionIndex(new Position(0, 0, 0)); // on voxel is considered in
            Assert.AreEqual(1, index);
            // two layer results
            index = _twoLayerTissue.GetRegionIndex(new Position(10, 0, 0)); // outside voxel
            Assert.AreEqual(4, index);
            index = _twoLayerTissue.GetRegionIndex(new Position(0, 0, 2.5)); // inside voxel
            Assert.AreEqual(2, index);
            index = _twoLayerTissue.GetRegionIndex(new Position(0, 0, 0)); // on voxel is considered in
            Assert.AreEqual(1, index);
        }

        /// <summary>
        /// Validate method GetNeighborRegionIndex return correct Boolean
        /// </summary>
        [Test]
        public void verify_GetNeighborRegionIndex_method_returns_correct_result()
        {
            Photon photon = new Photon( // on side of voxel pointed into it
                new Position(-1, 0, 1),
                new Direction(1.0, 0, 0),
                1.0,
                _oneLayerTissue,
                3,
                new Random());
            var index = _oneLayerTissue.GetNeighborRegionIndex(photon); 
            Assert.AreEqual(1, index);
            photon = new Photon( // on side of voxel pointed out of it
                new Position(-1, 0, 1),
                new Direction(-1.0, 0, 0),
                1.0,
                _oneLayerTissue,
                1,
                new Random());
            index = _oneLayerTissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(3, index);
            // two layer results
            photon = new Photon( // on side of voxel pointed into LAYER 1
                new Position(-1, 0, 0.5),  
                new Direction(1.0, 0, 0),
                1.0,
                _twoLayerTissue,
                4,
                new Random());
            index = _twoLayerTissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(1, index);
            photon = new Photon( // on side of voxel in LAYER 1 pointed out of it
                new Position(-1, 0, 0.5),
                new Direction(-1.0, 0, 0),
                1.0,
                _twoLayerTissue,
                1,
                new Random());
            index = _twoLayerTissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(4, index);
            photon = new Photon( // on side of voxel pointed into LAYER 2
                new Position(-1, 0, 1.5),
                new Direction(1.0, 0, 0),
                1.0,
                _twoLayerTissue,
                4,
                new Random());
            index = _twoLayerTissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(2, index);
            photon = new Photon( // on side of voxel in LAYER 2 pointed out of it
                new Position(-1, 0, 1.5),
                new Direction(-1.0, 0, 0),
                1.0,
                _twoLayerTissue,
                1,
                new Random());
            index = _twoLayerTissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(4, index);
        }

        /// <summary>
        /// Validate method GetAngleRelativeToBoundaryNormal return correct Boolean
        /// </summary>
        [Test]
        public void verify_GetAngleRelativeToBoundaryNormal_method_returns_correct_result()
        {
            Photon photon = new Photon( // on top of voxel pointed into it
                new Position(0, 0, 1.0),
                new Direction(0.0, 0, 1.0),
                1,
                _twoLayerTissue,
                1,
                new Random());
            double cosTheta = _twoLayerTissue.GetAngleRelativeToBoundaryNormal(photon);
            Assert.AreEqual(1,cosTheta);
        }


        ///// <summary>
        ///// Validate method GetAngleRelativeToBoundaryNormal return correct Boolean
        ///// </summary>
        //[Test]
        //public void verify_GetRefractedAngle_method_returns_correct_result()
        //{
        //    Photon photon = new Photon( // on top of voxel pointed into it
        //        new Position(0, 0, 1.0),
        //        new Direction(0.0, 0, 1.0),
        //        1,
        //        _twoLayerTissue,
        //        1,
        //        new Random());
        //    double cosTheta = _twoLayerTissue.GetAngleRelativeToBoundaryNormal(photon);
        //    Assert.AreEqual(1, cosTheta);
        //}

    }
}
