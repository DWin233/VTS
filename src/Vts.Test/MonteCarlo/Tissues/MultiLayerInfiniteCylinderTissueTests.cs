using System;
using NSubstitute.Exceptions;
using NUnit.Framework;
using Vts.Common;
using Vts.MonteCarlo;
using Vts.MonteCarlo.Tissues;

namespace Vts.Test.MonteCarlo.Tissues
{
    /// <summary>
    /// Unit tests for MultiLayerInfiniteCylinderTissue
    /// </summary>
    [TestFixture]
    public class MultiLayerInfiniteCylinderTissueTests
    {
        private MultiLayerInfiniteCylinderTissue _tissue;
        /// <summary>
        /// Validate general constructor of Tissue
        /// </summary>
        [OneTimeSetUp]
        public void Create_instance_of_class()
        {
            _tissue = new MultiLayerInfiniteCylinderTissue(
                new ITissueRegion[] // air-cylinder-cylinder-air
                {
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(double.NegativeInfinity, 0.0),
                        double.NegativeInfinity,
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0)),
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(0.0, 5.0),
                        20,
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(5.0, 10.0),
                        15,
                        new OpticalProperties(0.0, 1.0, 0.8, 1.4)),
                    new InfiniteCylinderTissueRegion(
                        new DoubleRange(10.0, 20),
                        10,
                        new OpticalProperties(0.0, 1e-10, 1.0, 1.0))
                }
            );
        }

        /// <summary>
        /// Validate method GetRegionIndex return correct Boolean
        /// </summary>
        [Test]
        public void Verify_GetRegionIndex_method_returns_correct_result()
        {
            var index = _tissue.GetRegionIndex(new Position(0, 0, 17)); // air inside
            Assert.AreEqual(3, index);
            index = _tissue.GetRegionIndex(new Position(0, 0, 4)); // inside outer cylinder outside inner
            Assert.AreEqual(1, index);
            index = _tissue.GetRegionIndex(new Position(0, 0, 6.0)); // inside inner cylinder
            Assert.AreEqual(2, index);
        }

        /// <summary>
        /// Validate method GetNeighborRegionIndex return correct Boolean
        /// </summary>
        [Test]
        public void Verify_GetNeighborRegionIndex_method_returns_correct_result()
        {
            // concave down portion of cylindrical tissue layers
            var photon = new Photon( // on bottom of outer infinite cylinder pointed into it
                new Position(0, 0, 5.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                2,
                new Random());
            var index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(1, index);
            photon = new Photon( // on top of outer infinite cylinder pointed out of it
                new Position(0, 0, 0.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                1,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(0, index);
            photon = new Photon( // on top of inner infinite cylinder pointed into it
                new Position(0, 0, 5.0),
                new Direction(0.0, 0, 1.0),
                1.0,
                _tissue,
                1,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(2, index);
            photon = new Photon( // on top of inner infinite cylinder pointed out of it
                new Position(0, 0, 5.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                2,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(1, index);

            // do symmetric opposite tests on concave up portion of tissue cylinders
            photon = new Photon( // on bottom of outer tissue infinite cylinder pointed out of it
                new Position(0, 0, 35.0),
                new Direction(0.0, 0, 1.0),
                1.0,
                _tissue,
                2,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(1, index);
            photon = new Photon( // on top of outer infinite cylinder pointed out of it
                new Position(0, 0, 40.0),
                new Direction(0.0, 0, 1.0),
                1.0,
                _tissue,
                1,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(0, index);
            photon = new Photon( // on top of inner infinite cylinder pointed into it
                new Position(0, 0, 30.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                2,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(3, index);
            photon = new Photon( // on top of outer infinite cylinder pointed out of it
                new Position(0, 0, 35.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                1,
                new Random());
            index = _tissue.GetNeighborRegionIndex(photon);
            Assert.AreEqual(2, index);
        }

        /// <summary>
        /// Test to make sure that shortest distance to layer boundary or infinite cylinders is correct
        /// </summary>
        [Test]
        public void Validate_GetDistanceToBoundary_method_returns_correct_results()
        {
            var photon = new Photon( // below outer infinite cylinder pointed into it
                new Position(0, 0, 7),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                2,
                new Random())
            {
                S = 10
            };
            var distance = _tissue.GetDistanceToBoundary(photon);
            Assert.IsTrue(Math.Abs(distance - 2) < 1e-6);
            photon = new Photon(        // above inner infinite cylinder pointed into it
                new Position(0, 0, 3),
                new Direction(0.0, 0, 1.0),
                1.0,
                _tissue,
                1,
                new Random())
            {
                S = 10
            };
            distance = _tissue.GetDistanceToBoundary(photon);
            Assert.IsTrue(Math.Abs(distance - 2) < 1e-6);
            photon = new Photon(        // inside inner infinite cylinder pointed out and down
                new Position(0, 0, 7.0),
                new Direction(0.0, 0, 1.0),
                1.0,
                _tissue,
                2,
                new Random())
            {
                S = 10
            };
            distance = _tissue.GetDistanceToBoundary(photon);
            Assert.IsTrue(Math.Abs(distance - 3.0) < 1e-6);
            photon = new Photon( // on bottom of outer cylinder pointed towards inner
                new Position(0, 0, 37.0),
                new Direction(0.0, 0, -1.0),
                1.0,
                _tissue,
                1,
                new Random())
            {
                S = 10
            };
            distance = _tissue.GetDistanceToBoundary(photon);
            Assert.IsTrue(Math.Abs(distance - 2) < 1e-6);
        }

        /// <summary>
        /// Validate method GetReflectedDirection return correct Direction.  Note that Photon class
        /// determines whether in critical angle and if so, whether to reflect or refract.  This unit
        /// test just tests isolated method.
        /// </summary>
        [Test]
        public void Verify_GetReflectedDirection_method_returns_correct_result()
        {
            // put photon on boundary of domain to make sure base (MultiLayerTissue) call works
            var currentPosition = new Position(0, 0, 0);
            var currentDirection = new Direction(1 / Math.Sqrt(2), 0, -1 / Math.Sqrt(2));
            var reflectedDir = _tissue.GetReflectedDirection(
                currentPosition, currentDirection);
            Assert.IsTrue(Math.Abs(1 / Math.Sqrt(2) - reflectedDir.Ux) < 1e-6);
            Assert.AreEqual(0, reflectedDir.Uy);
            Assert.IsTrue(Math.Abs(1 / Math.Sqrt(2) - reflectedDir.Uz) < 1e-6); // reflection off layer just flips sign of Uz
            // index mismatched perpendicular between tissue and outer infinite cylinder
            _tissue.Regions[1].RegionOP.N = 1.0; // outer infinite cylinder has n=1.4
            currentPosition = new Position(0, 0, 5); // put photon on infinite cylinder
            currentDirection = new Direction(0, 0, -1); // pointed into it
            reflectedDir = _tissue.GetReflectedDirection(currentPosition, currentDirection);
            Assert.AreEqual(0, reflectedDir.Ux);
            Assert.AreEqual(0, reflectedDir.Uy);
            Assert.AreEqual(1, reflectedDir.Uz);
            // index matched perpendicular
            _tissue.Regions[1].RegionOP.N = 1.4; // reset back to 1.4
            _tissue.Regions[2].RegionOP.N = 1.4; // surrounding layer has n=1.4
            currentPosition = new Position(0, 0, 5 + 1e-6); // put photon on infinite cylinder
            currentDirection = new Direction(0, 0, -1); // pointed into it
            reflectedDir = _tissue.GetReflectedDirection(currentPosition, currentDirection);
            Assert.AreEqual(0, reflectedDir.Ux);
            Assert.AreEqual(0, reflectedDir.Uy);
            Assert.AreEqual(-1, reflectedDir.Uz);
            // index mismatched 45 deg to tangent surface, set n mismatch
            _tissue.Regions[2].RegionOP.N = 1.0;
            currentDirection = new Direction(1 / Math.Sqrt(2), 0, -1 / Math.Sqrt(2));
            reflectedDir = _tissue.GetReflectedDirection(currentPosition, currentDirection);
            Assert.IsTrue(Math.Abs(reflectedDir.Ux - 1 / Math.Sqrt(2)) < 1e-6);
            Assert.AreEqual(0, reflectedDir.Uy);
            Assert.IsTrue(Math.Abs(reflectedDir.Uz - 1 / Math.Sqrt(2)) < 1e-6);
        }

        /// <summary>
        /// Validate method GetReflectedDirection returns correct direction.
        /// </summary>
        [Test]
        public void Verify_GetRefractedDirection_method_returns_correct_result()
        {
            // put photon on boundary of domain at bottom of inner tissue cylinder
            var currentPosition = new Position(0, 0, 35);
            var currentDirection = new Direction(1 / Math.Sqrt(2), 0, -1 / Math.Sqrt(2));
            var nCurrent = 1.4; // matched
            const double nNext = 1.4;
            var cosThetaSnell = 1 / Math.Sqrt(2);
            var refractedDir = _tissue.GetRefractedDirection(
                currentPosition, currentDirection, nCurrent, nNext, cosThetaSnell);
            Assert.IsTrue(Math.Abs(refractedDir.Ux - 1 / Math.Sqrt(2)) < 1e-6);
            Assert.AreEqual(0, refractedDir.Uy);
            Assert.IsTrue(Math.Abs(refractedDir.Uz + 1 / Math.Sqrt(2)) < 1e-6);
            // index matched perpendicular between tissue and outer infinite cylinder
            currentPosition = new Position(0, 0, 5); // put photon on outer infinite cylinder
            currentDirection = new Direction(0, 0, -1); // pointed into it
            refractedDir = _tissue.GetRefractedDirection(
                currentPosition, currentDirection, nCurrent, nNext, cosThetaSnell);
            Assert.AreEqual(0, refractedDir.Ux);
            Assert.AreEqual(0, refractedDir.Uy);
            Assert.AreEqual(-1, refractedDir.Uz);
            // index mismatched perpendicular between tissue and outer infinite cylinder
            nCurrent = 1.0;
            currentPosition = new Position(0, 0, 5); // put photon on outer infinite cylinder
            currentDirection = new Direction(0, 0, -1); // pointed into it
            refractedDir = _tissue.GetRefractedDirection(
                currentPosition, currentDirection, nCurrent,nNext, cosThetaSnell);
            Assert.AreEqual(0, refractedDir.Ux);
            Assert.AreEqual(0, refractedDir.Uy);
            Assert.AreEqual(-1, refractedDir.Uz);
            // index matched 45 degree angle
            nCurrent = 1.4;
            currentPosition = new Position(0, 0, 35); // put photon on outer infinite cylinder
            currentDirection = new Direction(1 / Math.Sqrt(2), 0, -1 / Math.Sqrt(2)); // at 45 deg
            refractedDir = _tissue.GetRefractedDirection(
                currentPosition, currentDirection, nCurrent, nNext, cosThetaSnell);
            Assert.IsTrue(Math.Abs(refractedDir.Ux - 1 / Math.Sqrt(2)) < 1e-6);
            Assert.AreEqual(0, refractedDir.Uy);
            Assert.IsTrue(Math.Abs(refractedDir.Uz + 1 / Math.Sqrt(2)) < 1e-6);
            // index mismatched 45 degrees
            nCurrent = 1.0;
            refractedDir = _tissue.GetRefractedDirection(
                currentPosition, currentDirection, nCurrent, nNext, cosThetaSnell);
            Assert.IsTrue(Math.Abs(refractedDir.Ux - 0.260331) < 1e-6);
            Assert.AreEqual(0, refractedDir.Uy);
            Assert.IsTrue(Math.Abs(refractedDir.Uz + 0.965519) < 1e-6);
            Assert.IsTrue(Math.Sqrt(refractedDir.Ux * refractedDir.Ux +
                                    refractedDir.Uy * refractedDir.Uy +
                                    refractedDir.Uz * refractedDir.Uz) - 1 < 1e-6);
        }

        /// <summary>
        /// Validate method GetAngleRelativeToBoundaryNormal return correct Boolean
        /// </summary>
        [Test]
        public void Verify_GetAngleRelativeToBoundaryNormal_method_returns_correct_result()
        {
            var photon = new Photon( // on top of inner cylinder pointed into it
                new Position(0, 0, 5.0),
                new Direction(0.0, 0, 1.0),
                1,
                _tissue,
                1,
                new Random());
            var cosTheta = _tissue.GetAngleRelativeToBoundaryNormal(photon);
            Assert.AreEqual(1,cosTheta);
        }
    }
}
