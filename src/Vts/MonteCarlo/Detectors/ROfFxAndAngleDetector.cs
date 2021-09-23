using System;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// DetectorInput for reflectance as a function of spatial frequency fx and exiting theta angle
    /// </summary>
    public class ROfFxAndAngleDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for reflectance as a function of Fx and Angle detector input
        /// </summary>
        public ROfFxAndAngleDetectorInput()
        {
            TallyType = "ROfFxAndAngle";
            Name = "ROfFxAndAngle";
            Fx = new DoubleRange(0.0, 0.5, 51);
            Angle = new DoubleRange(Math.PI / 2, Math.PI, 2);
            NA = double.PositiveInfinity; // set default NA completely open regardless of detector region refractive index
            FinalTissueRegionIndex = 0; // assume detector is in air

            // modify base class TallyDetails to take advantage of built-in validation capabilities (error-checking)
            TallyDetails.IsReflectanceTally = true;
        }

        /// <summary>
        /// detector Fx binning
        /// </summary>
        public DoubleRange Fx { get; set; }
        /// <summary>
        /// angle binning
        /// </summary>
        public DoubleRange Angle { get; set; }
        /// <summary>
        /// Detector region index
        /// </summary>
        public int FinalTissueRegionIndex { get; set; }
        /// <summary>
        /// numerical aperture
        /// </summary>
        public double NA { get; set; }

        /// <summary>
        /// Method to create detector from detector input
        /// </summary>
        /// <returns>created IDetector</returns>
        public IDetector CreateDetector()
        {
            return new ROfFxAndAngleDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,

                // optional/custom detector-specific properties
                Fx = this.Fx,
                Angle = this.Angle,
                NA = this.NA,
                FinalTissueRegionIndex = this.FinalTissueRegionIndex
            };
        }
    }

    /// <summary>
    /// Implements IDetector.  Tally for reflectance as a function  of Fx and Angle.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    public class ROfFxAndAngleDetector : Detector, IDetector
    {
        private ITissue _tissue;
        /* ==== Place optional/user-defined input properties here. They will be saved in text (JSON) format ==== */
        /* ==== Note: make sure to copy over all optional/user-defined inputs from corresponding input class ==== */

        /// <summary>
        /// Fx binning
        /// </summary>
        public DoubleRange Fx { get; set; }
        /// <summary>
        /// angle binning
        /// </summary>
        public DoubleRange Angle { get; set; }
        /// <summary>
        /// Detector region index
        /// </summary>
        public int FinalTissueRegionIndex { get; set; }
        /// <summary>
        /// numerical aperture
        /// </summary>
        public double NA { get; set; }
        /* ==== Place user-defined output arrays here. They should be prepended with "[IgnoreDataMember]" attribute ==== */
        /* ==== Then, GetBinaryArrays() should be implemented to save them separately in binary format ==== */
        /// <summary>
        /// detector mean
        /// </summary>
        [IgnoreDataMember]
        public Complex[,] Mean { get; set; }
        /// <summary>
        /// detector second moment
        /// </summary>
        [IgnoreDataMember]
        public Complex[,] SecondMoment { get; set; }

        /* ==== Place optional/user-defined output properties here. They will be saved in text (JSON) format ==== */
        /// <summary>
        /// number of times detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }

        /// <summary>
        /// Method to initialize detector
        /// </summary>
        /// <param name="tissue">tissue definition</param>
        /// <param name="rng">random number generator</param>
        public void Initialize(ITissue tissue, Random rng)
        {
            // assign any user-defined outputs (except arrays...we'll make those on-demand)
            TallyCount = 0;

            // if the data arrays are null, create them (only create second moment if TallySecondMoment is true)
            Mean = Mean ?? new Complex[Fx.Count, Angle.Count - 1];
            SecondMoment = SecondMoment ?? (TallySecondMoment ? new Complex[Fx.Count, Angle.Count - 1] : null);

            // initialize any other necessary class fields here
            _tissue = tissue;
        }

        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {
            if (!IsWithinDetectorAperture(photon))
                return;

            var dp = photon.DP;
            var ia = DetectorBinning.WhichBin(Math.Acos(photon.DP.Direction.Uz), Angle.Count - 1, Angle.Delta, Angle.Start);

            if (ia != -1)
            { 
                var x = dp.Position.X;
                var fxArray = Fx.AsEnumerable().ToArray();
                for (int ifx = 0; ifx < fxArray.Length; ifx++)
                {
                    double freq = fxArray[ifx];
                    var sinNegativeTwoPiFX = Math.Sin(-2*Math.PI*freq*x);
                    var cosNegativeTwoPiFX = Math.Cos(-2*Math.PI*freq*x);
                    // convert to Hz-sec from GHz-ns 1e-9*1e9=1
                    var deltaWeight = dp.Weight*(cosNegativeTwoPiFX + Complex.ImaginaryOne*sinNegativeTwoPiFX);

                    Mean[ifx, ia] += deltaWeight;
                    // 2nd moment is E[xx*]=E[xreal^2]+E[ximag^2] and with cos^2+sin^2=1 => weight^2
                    if (TallySecondMoment) 
                    {
                       SecondMoment[ifx, ia] += dp.Weight * dp.Weight;
                    }
                }
                TallyCount++;
            }
        }

        /// <summary>
        /// method to normalize detector tally results
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            var normalizationFactor = 2.0 * Math.PI * Angle.Delta;
            for (int ifx = 0; ifx < Fx.Count; ifx++)
            {
                for (int ia = 0; ia < Angle.Count - 1; ia++)
                {
                    var areaNorm = Math.Sin(Angle.Start + (ia + 0.5) * Angle.Delta) * normalizationFactor;
                    Mean[ifx, ia] /= areaNorm * numPhotons;
                    if (TallySecondMoment)
                    {
                        SecondMoment[ifx, ia] /= areaNorm * areaNorm * numPhotons;
                    } 
                }
            }
        }

        // this is to allow saving of large arrays separately as a binary file
        public BinaryArraySerializer[] GetBinarySerializers() // NEED TO ASK DC: about complex array implementation
        {
            return new[] {
                new BinaryArraySerializer {
                    DataArray = Mean,
                    Name = "Mean",
                    FileTag = "",
                    WriteData = binaryWriter => {
                        for (int i = 0; i < Fx.Count; i++) {
                            for (int j = 0; j < Angle.Count - 1; j++)
                            {
                                binaryWriter.Write(Mean[i, j].Real);
                                binaryWriter.Write(Mean[i, j].Imaginary);
                            }
                        }
                    },
                    ReadData = binaryReader => {
                        Mean = Mean ?? new Complex[ Fx.Count, Angle.Count - 1];
                        for (int i = 0; i <  Fx.Count; i++) {
                            for (int j = 0; j < Angle.Count - 1; j++)
                            {
                                var real = binaryReader.ReadDouble();
                                var imag = binaryReader.ReadDouble();
                                Mean[i, j] = new Complex(real, imag);
                            }
                        }
                    }
                },
                // return a null serializer, if we're not serializing the second moment
                !TallySecondMoment ? null :  new BinaryArraySerializer {
                    DataArray = SecondMoment,
                    Name = "SecondMoment",
                    FileTag = "_2",
                    WriteData = binaryWriter => {
                        if (!TallySecondMoment || SecondMoment == null) return;
                        for (int i = 0; i < Fx.Count; i++) {
                            for (int j = 0; j < Angle.Count - 1; j++)
                            {
                                binaryWriter.Write(SecondMoment[i, j].Real);
                                binaryWriter.Write(SecondMoment[i, j].Imaginary);
                            }                            
                        }
                    },
                    ReadData = binaryReader => {
                        if (!TallySecondMoment || SecondMoment == null) return;
                        SecondMoment = new Complex[ Fx.Count, Angle.Count - 1];
                        for (int i = 0; i < Fx.Count; i++) {
                            for (int j = 0; j < Angle.Count - 1; j++)
                            {
                                var real = binaryReader.ReadDouble();
                                var imag = binaryReader.ReadDouble();
                                SecondMoment[i, j] = new Complex(real, imag);
                            }                       
			            }
                    },
                },
            };
        }

        /// <summary>
        /// Method to determine if photon is within detector NA
        /// </summary>
        /// <param name="photon">photon</param>
        public bool IsWithinDetectorAperture(Photon photon)
        {
            if (photon.CurrentRegionIndex == FinalTissueRegionIndex)
            {
                var detectorRegionN = _tissue.Regions[photon.CurrentRegionIndex].RegionOP.N;
                return photon.DP.IsWithinNA(NA, Direction.AlongNegativeZAxis, detectorRegionN);
            }
            else // determine n of prior tissue region
            {
                var detectorRegionN = _tissue.Regions[FinalTissueRegionIndex].RegionOP.N;
                return photon.History.PreviousDP.IsWithinNA(NA, Direction.AlongNegativeZAxis, detectorRegionN);
            }
            //return true; // or, possibly test for NA or confined position, etc
            //return (dp.StateFlag.Has(PhotonStateType.PseudoTransmissionDomainTopBoundary));
        }

    }
}
