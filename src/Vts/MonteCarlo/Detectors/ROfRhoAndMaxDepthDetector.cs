using System;
using System.Linq;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Tally for reflectance as a function of Rho and MaxDepth.
    /// This works for Analog, DAW and CAW processing.
    /// </summary>
    public class ROfRhoAndMaxDepthDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for reflectance as a function of rho and MaxDepth detector input
        /// </summary>
        public ROfRhoAndMaxDepthDetectorInput()
        {
            TallyType = "ROfRhoAndMaxDepth";
            Name = "ROfRhoAndMaxDepth";
            Rho = new DoubleRange(0.0, 10, 101);
            MaxDepth = new DoubleRange(0.0, 1.0, 101);

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

        public IDetector CreateDetector()
        {
            return new ROfRhoAndMaxDepthDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,

                // optional/custom detector-specific properties
                Rho = this.Rho,
                MaxDepth = this.MaxDepth
            };
        }
    }
    /// <summary>
    /// Implements IDetector.  Tally for reflectance as a function  of Rho and MaxDepth.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    public class ROfRhoAndMaxDepthDetector : Detector, IDetector
    {
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
        /// <summary>
        /// distribution of maximum depths at each rho
        /// </summary>
        [IgnoreDataMember]
        public double[,] MaxDepthDistribution { get; set; }

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

            // intialize any other necessary class fields here
            MaxDepthDistribution = MaxDepthDistribution ?? new double[Rho.Count - 1, MaxDepth.Count - 1];        }

        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {
            var ir = DetectorBinning.WhichBin(DetectorBinning.GetRho(photon.DP.Position.X, photon.DP.Position.Y), Rho.Count - 1, Rho.Delta, Rho.Start);
            double maxDepth = photon.History.HistoryData.Max(d => d.Position.Z);
            var id = DetectorBinning.WhichBin(maxDepth, MaxDepth.Count - 1, MaxDepth.Delta, MaxDepth.Start);

            Mean[ir, id] += photon.DP.Weight; // mean integrated over max depth = R(rho)
            MaxDepthDistribution[ir, id] += 1; // max depth distribution independent of photon weight
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
                    MaxDepthDistribution[ir, id] /= areaNorm * numPhotons;
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
                new BinaryArraySerializer {
                    DataArray = MaxDepthDistribution,
                    Name = "MaxDepthDistribution",
                    FileTag = "_MaxDepthDistribution",
                    WriteData = binaryWriter => {
                        for (int i = 0; i < Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {
                                binaryWriter.Write(MaxDepthDistribution[i, j]);
                            }
                        }
                    },
                    ReadData = binaryReader => {
                        MaxDepthDistribution = MaxDepthDistribution ?? new double[ Rho.Count - 1, MaxDepth.Count - 1];
                        for (int i = 0; i <  Rho.Count - 1; i++) {
                            for (int j = 0; j < MaxDepth.Count - 1; j++)
                            {
                               Mean[i, j] = binaryReader.ReadDouble();
                            }
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Method to determine if photon is within detector
        /// </summary>
        /// <param name="dp">photon data point</param>
        /// <returns>method always returns true</returns>
        public bool ContainsPoint(PhotonDataPoint dp)
        {
            return true; // or, possibly test for NA or confined position, etc
            //return (dp.StateFlag.Has(PhotonStateType.PseudoTransmissionDomainTopBoundary));
        }

    }
}