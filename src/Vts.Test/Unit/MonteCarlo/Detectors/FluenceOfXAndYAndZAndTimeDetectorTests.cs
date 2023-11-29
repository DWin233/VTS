﻿using NUnit.Framework;
using System.IO;
using System.Reflection;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Detectors;

namespace Vts.Test.Unit.MonteCarlo.Detectors;

[TestFixture]
public class FluenceOfXAndYAndZAndTimeDetectorTests
{
    /// <summary>
    /// clear all test generated files
    /// </summary>
    [OneTimeSetUp]
    [OneTimeTearDown]
    public void Clear_previously_generated_files()
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        FolderCleanup.DeleteFileContaining(currentPath, "testfluenceofxandyandzandtime");
    }

    /// <summary>
    /// Test to verify that GetBinarySerializers are working correctly for this detector.
    /// </summary>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void Validate_deserialized_class_is_correct_when_using_GetBinarySerializers(bool tallySecondMoment)
    {
        const string detectorName = "testfluenceofxandyandzandtime";
        var detector = new FluenceOfXAndYAndZAndTimeDetector
        {
            X = new DoubleRange(-10, 10, 3),
            Y = new DoubleRange(-10, 10, 3),
            Z = new DoubleRange(0, 1, 3),
            Time = new DoubleRange(0, 1, 4),
            TallySecondMoment = tallySecondMoment,
            Name = detectorName,
            Mean = new double[,,,] // X.Count-1 x Y.Count-1 x Z.Count-1 x Time.Count-1: 2x2x2x3
            {
                {
                    { { 1, 2, 3 }, { 4, 5, 6 } },
                    { { 7, 8, 9 }, { 10, 11, 12 } }
                },
                {
                    { { 13, 14, 15 }, { 16, 17, 18 } },
                    { { 19, 20, 21 }, { 22, 23, 24 } }
                }
            },
            SecondMoment = new double[,,,]
            {
                {
                    { { 25, 26, 27 }, { 28, 29, 30 } },
                    { { 31, 32, 33 }, { 34, 35, 36 } }
                },
                {
                    { { 37, 38, 39 }, { 40, 41, 42 } },
                    { { 43, 44, 45 }, { 46, 47, 48 } }
                }
            }
        };

        DetectorBinarySerializationHelper.WriteClearAndReReadArrays(detector, detector.Mean, detector.SecondMoment);

        Assert.AreEqual(1, detector.Mean[0, 0, 0, 0]);
        Assert.AreEqual(2, detector.Mean[0, 0, 0, 1]);
        Assert.AreEqual(3, detector.Mean[0, 0, 0, 2]);
        Assert.AreEqual(4, detector.Mean[0, 0, 1, 0]);
        Assert.AreEqual(5, detector.Mean[0, 0, 1, 1]);
        Assert.AreEqual(6, detector.Mean[0, 0, 1, 2]);
        Assert.AreEqual(7, detector.Mean[0, 1, 0, 0]);
        Assert.AreEqual(8, detector.Mean[0, 1, 0, 1]);
        Assert.AreEqual(9, detector.Mean[0, 1, 0, 2]);
        Assert.AreEqual(10, detector.Mean[0, 1, 1, 0]);
        Assert.AreEqual(11, detector.Mean[0, 1, 1, 1]);
        Assert.AreEqual(12, detector.Mean[0, 1, 1, 2]);
        Assert.AreEqual(13, detector.Mean[1, 0, 0, 0]);
        Assert.AreEqual(14, detector.Mean[1, 0, 0, 1]);
        Assert.AreEqual(15, detector.Mean[1, 0, 0, 2]);
        Assert.AreEqual(16, detector.Mean[1, 0, 1, 0]);
        Assert.AreEqual(17, detector.Mean[1, 0, 1, 1]);
        Assert.AreEqual(18, detector.Mean[1, 0, 1, 2]);
        Assert.AreEqual(19, detector.Mean[1, 1, 0, 0]);
        Assert.AreEqual(20, detector.Mean[1, 1, 0, 1]);
        Assert.AreEqual(21, detector.Mean[1, 1, 0, 2]);
        Assert.AreEqual(22, detector.Mean[1, 1, 1, 0]);
        Assert.AreEqual(23, detector.Mean[1, 1, 1, 1]);
        Assert.AreEqual(24, detector.Mean[1, 1, 1, 2]);
        if (!tallySecondMoment) return;
        Assert.AreEqual(25, detector.SecondMoment[0, 0, 0, 0]);
        Assert.AreEqual(26, detector.SecondMoment[0, 0, 0, 1]);
        Assert.AreEqual(27, detector.SecondMoment[0, 0, 0, 2]);
        Assert.AreEqual(28, detector.SecondMoment[0, 0, 1, 0]);
        Assert.AreEqual(29, detector.SecondMoment[0, 0, 1, 1]);
        Assert.AreEqual(30, detector.SecondMoment[0, 0, 1, 2]);
        Assert.AreEqual(31, detector.SecondMoment[0, 1, 0, 0]);
        Assert.AreEqual(32, detector.SecondMoment[0, 1, 0, 1]);
        Assert.AreEqual(33, detector.SecondMoment[0, 1, 0, 2]);
        Assert.AreEqual(34, detector.SecondMoment[0, 1, 1, 0]);
        Assert.AreEqual(35, detector.SecondMoment[0, 1, 1, 1]);
        Assert.AreEqual(36, detector.SecondMoment[0, 1, 1, 2]);
        Assert.AreEqual(37, detector.SecondMoment[1, 0, 0, 0]);
        Assert.AreEqual(38, detector.SecondMoment[1, 0, 0, 1]);
        Assert.AreEqual(39, detector.SecondMoment[1, 0, 0, 2]);
        Assert.AreEqual(40, detector.SecondMoment[1, 0, 1, 0]);
        Assert.AreEqual(41, detector.SecondMoment[1, 0, 1, 1]);
        Assert.AreEqual(42, detector.SecondMoment[1, 0, 1, 2]);
        Assert.AreEqual(43, detector.SecondMoment[1, 1, 0, 0]);
        Assert.AreEqual(44, detector.SecondMoment[1, 1, 0, 1]);
        Assert.AreEqual(45, detector.SecondMoment[1, 1, 0, 2]);
        Assert.AreEqual(46, detector.SecondMoment[1, 1, 1, 0]);
        Assert.AreEqual(47, detector.SecondMoment[1, 1, 1, 1]);
        Assert.AreEqual(48, detector.SecondMoment[1, 1, 1, 2]);
    }
}