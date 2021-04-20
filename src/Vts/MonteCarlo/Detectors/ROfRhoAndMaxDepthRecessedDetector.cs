using System;
using System.Linq;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Tally for reflectance as a function of Rho and MaxDepth recessed in air
    /// This works for Analog, DAW and CAW processing.
    /// </summary>
    public class ROfRhoAndMaxDepthRecessedDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for reflectance as a function of rho and MaxDepth detector input
        /// </summary>
        public ROfRhoAndMaxDepthRecessedDetectorInput()
        {
            TallyType = "ROfRhoAndMaxDepthRecessed";
            Name = "ROfRhoAndMaxDepthRecessed";
            Rho = new DoubleRange(0.0, 10, 101);
            ZPlane = -1.0;
            MaxDepth = new DoubleRange(0.0, 1.0, 101);
            NA = double.PositiveInfinity; // set default NA completely open regardless of detector region refractive index
            FinalTissueRegionIndex = 0; // assume detector is in air

            // modify base class TallyDetails to take advantage of built-in validation capabilities (error-checking)
            TallyDetails.IsReflectanceTally = true;
            TallyDetails.IsCylindricalTally = true;
        }

        /// <summary>
        /// rho binning
        /// </summary>
        public DoubleRange Rho { get; set; }
        /// <summary>
        /// MaxDepth binning
        /// </summary>
        public DoubleRange MaxDepth { get; set; }
        /// <summary>
        /// z-plane above tissue in air
        /// </summary>
        public double ZPlane { get; set; }
        /// <summary>
        /// Detector region index
        /// </summary>
        public int FinalTissueRegionIndex { get; set; }
        /// <summary>
        /// detector numerical aperture
        /// </summary>
        public double NA { get; set; }

        public IDetector CreateDetector()
        {
            return new ROfRhoAndMaxDepthRecessedDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,

                // optional/custom detector-specific properties
                Rho = this.Rho,
                MaxDepth = this.MaxDepth,
                ZPlane = this.ZPlane,
                NA = this.NA,
                FinalTissueRegionIndex = this.FinalTissueRegionIndex
            };
        }
    }
    /// <summary>
    /// Implements IDetector.  Tally for reflectance as a function  of Rho and MaxDepth.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    public class ROfRhoAndMaxDepthRecessedDetector : Detector, IDetector
    {
        private ITissue _tissue;

        /* ==== Place optional/user-defined input properties here. They will be saved in text (JSON) format ==== */
        /* ==== Note: make sure to copy over all optional/user-defined inputs from corresponding input class ==== */
        /// <summary>
        /// rho binning
        /// </summary>
        public DoubleRange Rho { get; set; }
        /// <summary>
        /// MaxDepth binning
        /// </summary>
        public DoubleRange MaxDepth { get; set; }
        /// <summary>
        /// height above tissue in air
        /// </summary>
        public double ZPlane { get; set; }
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
        public double[,] Mean { get; set; }
        /// <summary>
        /// detector second moment
        /// </summary>
        [IgnoreDataMember]
        public double[,] SecondMoment { get; set; }

        /* ==== Place optional/user-defined output properties here. They will be saved in text (JSON) format ==== */
        /// <summary>
        /// number of times detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }

        public void Initialize(ITissue tissue, Random rng)
        {
            // assign any user-defined outputs (except arrays...we'll make those on-demand)
            TallyCount = 0;

            // if the data arrays are null, create them (only create second moment if TallySecondMoment is true)
            Mean = Mean ?? new double[Rho.Count - 1, MaxDepth.Count - 1];
            SecondMoment = SecondMoment ?? (TallySecondMoment ? new double[Rho.Count - 1, MaxDepth.Count - 1] : null);

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

            // ray trace exit location and direction to location at ZPlane in air
            var positionAtHeight = LayerTissueRegionToolbox.RayExtendToInfinitePlane(
                photon.DP.Position, photon.DP.Direction, ZPlane);

            var ir = DetectorBinning.WhichBin(DetectorBinning.GetRho(positionAtHeight.X, positionAtHeight.Y), Rho.Count - 1, Rho.Delta, Rho.Start);
            double maxDepth = photon.History.HistoryData.Max(d => d.Position.Z);
            var id = DetectorBinning.WhichBin(maxDepth, MaxDepth.Count - 1, MaxDepth.Delta, MaxDepth.Start);

            Mean[ir, id] += photon.DP.Weight; // mean integrated over max depth = R(rho)
            if (TallySecondMoment)
            {
                SecondMoment[ir, id] += photon.DP.Weight * photon.DP.Weight;
            }
            TallyCount++;
        }
        /// <summary>
        /// method to normalize detector results after all photons launched
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            var normalizationFactor = 2.0 * Math.PI * Rho.Delta;
            var sum = 0.0;
            for (int ir = 0; ir < Rho.Count - 1; ir++)
            {
                var areaNorm = (Rho.Start + (ir + 0.5) * Rho.Delta) * normalizationFactor;
                for (int id = 0; id < MaxDepth.Count - 1; id++)
                {
                    Mean[ir, id] /= areaNorm * numPhotons;
                    sum += Mean[ir, id];
                    if (TallySecondMoment)
                    {
                        SecondMoment[ir, id] /= areaNorm * areaNorm * numPhotons;
                    }
                }
            }
            var dum = sum;
        }
        // this is to allow saving of large arrays separately as a binary file
        public BinaryArraySerializer[] GetBinarySerializers()
        {
            return new []
            {
                new BinaryArraySerializer {
                    DataArray = Mean,
                    Name = "Mean",
                    FileTag = "",
                    WriteData = binaryWriter => {
                        for (int i = 0; i < Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {                                
                                binaryWriter.Write(Mean[i, j]);
                            }
                        }
                    },
                    ReadData = binaryReader => {
                        Mean = Mean ?? new double[ Rho.Count - 1, MaxDepth.Count - 1];
                        for (int i = 0; i <  Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {
                               Mean[i, j] = binaryReader.ReadDouble(); 
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
                        for (int i = 0; i < Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {
                                binaryWriter.Write(SecondMoment[i, j]);
                            }                            
                        }
                    },
                    ReadData = binaryReader => {
                        if (!TallySecondMoment || SecondMoment == null) return;
                        SecondMoment = new double[ Rho.Count - 1, MaxDepth.Count - 1];
                        for (int i = 0; i < Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {
                                SecondMoment[i, j] = binaryReader.ReadDouble();
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