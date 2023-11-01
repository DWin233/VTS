using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using Vts.Common;
using Vts.IO;
using Vts.MonteCarlo.Extensions;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Tally for Transmitted dynamic MT as a function of X and Y using blood volume fraction in each tissue region.
    /// This detector also tallies the total and dynamic MT as a function of Z.   If a random number is less
    /// than blood volume fraction for the tissue region in which the collision occurred, then hit blood and considered
    /// "dynamic" event.  Otherwise, it is a "static" event.
    /// This works for Analog and DAW processing.
    /// </summary>
    public class TransmittedDynamicMTOfXAndYAndSubregionHistDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for TransmittedMT as a function of rho and tissue subregion detector input
        /// </summary>
        public TransmittedDynamicMTOfXAndYAndSubregionHistDetectorInput()
        {
            TallyType = "TransmittedDynamicMTOfXAndYAndSubregionHist";
            Name = "TransmittedDynamicMTOfXAndYAndSubregionHist";
            X = new DoubleRange(-10.0, 10.0, 101);
            Y = new DoubleRange(-10.0, 10.0, 101);
            Z = new DoubleRange(0.0, 10.0, 51);
            MTBins = new DoubleRange(0.0, 500.0, 51);
            NA = double.PositiveInfinity; // set default NA completely open regardless of detector region refractive index
            FinalTissueRegionIndex = 2; // assume detector is in air below tissue

            // modify base class TallyDetails to take advantage of built-in validation capabilities (error-checking)
            TallyDetails.IsTransmittanceTally = true;
            TallyDetails.IsCylindricalTally = true;
        }

        /// <summary>
        /// x binning
        /// </summary>
        public DoubleRange X { get; set; }
        /// <summary>
        /// y binning
        /// </summary>
        public DoubleRange Y { get; set; }
        /// <summary>
        /// z binning
        /// </summary>
        public DoubleRange Z { get; set; }
        /// <summary>
        /// subregion blood volume fraction
        /// </summary>
        public IList<double> BloodVolumeFraction { get; set; }
        /// <summary>
        /// momentum transfer binning
        /// </summary>
        public DoubleRange MTBins { get; set; }
        /// <summary>
        /// fractional momentum transfer binning
        /// </summary>
        public DoubleRange FractionalMTBins { get; set; }
        /// <summary>
        /// Detector region index
        /// </summary>
        public int FinalTissueRegionIndex { get; set; }
        /// <summary>
        /// numerical aperture
        /// </summary>
        public double NA { get; set; }
        /// <summary>
        /// number of dynamic and static collisions in each subregion
        /// </summary>
        [IgnoreDataMember]
        public double[,] SubregionCollisions { get; set; }

        /// <summary>
        /// Method to create detector from detector input
        /// </summary>
        /// <returns>created IDetector</returns>
        public IDetector CreateDetector()
        {
            return new TransmittedDynamicMTOfXAndYAndSubregionHistDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,

                // optional/custom detector-specific properties
                X = this.X,
                Y = this.Y,
                Z = this.Z,
                MTBins = this.MTBins,
                BloodVolumeFraction = this.BloodVolumeFraction,
                FractionalMTBins = this.FractionalMTBins,
                NA = this.NA,
                FinalTissueRegionIndex = this.FinalTissueRegionIndex
            };
        }
    }

    /// <summary>
    /// Implements IDetector.  Tally for momentum transfer as a function  of X, Y and tissue subregion
    /// using blood volume fraction in each tissue subregion.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    public class TransmittedDynamicMTOfXAndYAndSubregionHistDetector : Detector, IDetector
    {
        private ITissue _tissue;
        private IList<double> _bloodVolumeFraction;
        private Random _rng;

        /* ==== Place optional/user-defined input properties here. They will be saved in text (JSON) format ==== */
        /* ==== Note: make sure to copy over all optional/user-defined inputs from corresponding input class ==== */
        /// <summary>
        /// x binning
        /// </summary>
        public DoubleRange X { get; set; }
        /// <summary>
        /// y binning
        /// </summary>
        public DoubleRange Y { get; set; }
        /// <summary>
        /// z binning
        /// </summary>
        public DoubleRange Z { get; set; }
        /// <summary>
        /// momentum transfer binning
        /// </summary>
        public DoubleRange MTBins { get; set; }
        /// <summary>
        /// subregion blood volume fraction
        /// </summary>
        public IList<double> BloodVolumeFraction { get; set; }
        /// <summary>
        /// fractional momentum transfer binning
        /// </summary>
        public DoubleRange FractionalMTBins { get; set; }
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
        public double[,,] Mean { get; set; }
        /// <summary>
        /// detector second moment
        /// </summary>
        [IgnoreDataMember]
        public double[,,] SecondMoment { get; set; }
        /// <summary>
        /// total MT as a function of Z multiplied by final photon weight
        /// </summary>
        [IgnoreDataMember]
        public double[, ,] TotalMTOfZ { get; set; }
        /// <summary>
        /// total MT Second Moment as a function of Z multiplied by final photon weight
        /// </summary>
        [IgnoreDataMember]
        public double[, ,] TotalMTOfZSecondMoment { get; set; }
        /// <summary>
        /// dynamic MT as a function of Z multiplied by final photon weight
        /// </summary>
        [IgnoreDataMember]
        public double[, ,] DynamicMTOfZ { get; set; }
        /// <summary>
        /// dynamic MT Second Moment as a function of Z multiplied by final photon weight
        /// </summary>
        [IgnoreDataMember]
        public double[, ,] DynamicMTOfZSecondMoment { get; set; }
        /// <summary>
        /// fraction of DYNAMIC MT spent in tissue
        /// </summary>
        [IgnoreDataMember]
        public double[,,,] FractionalMT { get; set; }
        /// <summary>
        /// number of dynamic and static collisions in each subregion
        /// </summary>
        [IgnoreDataMember]
        public double[,] SubregionCollisions { get; set; }

        /* ==== Place optional/user-defined output properties here. They will be saved in text (JSON) format ==== */
        /// <summary>
        /// number of Zs detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }
        /// <summary>
        /// Z binning
        /// </summary>
        public int NumSubregions { get; set; }

        /// <summary>
        /// Method to initialize detector
        /// </summary>
        /// <param name="tissue">tissue definition</param>
        /// <param name="rng">random number generator</param>
        public void Initialize(ITissue tissue, Random rng)
        {
            // initialize any necessary class fields here
            _tissue = tissue;
            _rng = rng;

            // assign any user-defined outputs (except arrays...we'll make those on-demand)
            TallyCount = 0;
            NumSubregions = _tissue.Regions.Count;

            // if the data arrays are null, create them (only create second moment if TallySecondMoment is true)
            Mean = Mean ?? new double[X.Count - 1, Y.Count - 1, MTBins.Count - 1];
            SecondMoment = SecondMoment ?? (TallySecondMoment ? new double[X.Count - 1, Y.Count - 1, MTBins.Count - 1] : null);

            TotalMTOfZ = TotalMTOfZ ?? new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            DynamicMTOfZ = DynamicMTOfZ ?? new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            TotalMTOfZSecondMoment = TotalMTOfZSecondMoment ?? new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            DynamicMTOfZSecondMoment = DynamicMTOfZSecondMoment ?? new double[X.Count - 1, Y.Count - 1, Z.Count - 1];

            // Fractional MT has FractionalMTBins.Count numnber of bins PLUS 2, one for =1, an d one for =0
            FractionalMT = FractionalMT ?? new double[X.Count - 1, Y.Count - 1, MTBins.Count - 1, FractionalMTBins.Count + 1];

            SubregionCollisions = new double[NumSubregions, 2]; // 2nd index: 0=static, 1=dynamic 

            // initialize any other necessary class fields here
            _bloodVolumeFraction = BloodVolumeFraction;
 
        }

        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {
            if (!IsWithinDetectorAperture(photon)) return;

            // calculate the radial bin to attribute the deposition
            var ix = DetectorBinning.WhichBin(photon.DP.Position.X, X.Count - 1, X.Delta, X.Start);
            var iy = DetectorBinning.WhichBin(photon.DP.Position.Y, Y.Count - 1, Y.Delta, Y.Start);  
          
            var tissueMt = new double[2]; // 2 is for [static, dynamic] tally separation
            var talliedMt = false;
            double totalMt = 0;
            var totalMtOfZForOnePhoton = new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            var dynamicMtOfZForOnePhoton = new double[X.Count - 1, Y.Count - 1, Z.Count - 1];

            // go through photon history and claculate momentum transfer
            // assumes that no MT tallied at pseudo-collisions (reflections and refractions)
            // this algorithm needs to look ahead to angle of next DP, but needs info from previous to determine whether real or pseudo-collision
            var previousDp = photon.History.HistoryData.First();
            var currentDp = photon.History.HistoryData.Skip(1).Take(1).First();
            foreach (var nextDp in photon.History.HistoryData.Skip(2))
            {
                if (previousDp.Weight != currentDp.Weight) // only for true collision points
                {
                    var csr = _tissue.GetRegionIndex(currentDp.Position); // get current region index
                    // get z bin of current position
                    var iz = DetectorBinning.WhichBin(currentDp.Position.Z, Z.Count - 1, Z.Delta, Z.Start);
                    // get angle between current and next
                    var cosineBetweenTrajectories = Direction.GetDotProduct(currentDp.Direction, nextDp.Direction);
                    var momentumTransfer = 1 - cosineBetweenTrajectories;
                    totalMt += momentumTransfer;
                    TotalMTOfZ[ix, iy, iz] += photon.DP.Weight * momentumTransfer;
                    totalMtOfZForOnePhoton[ix, iy, iz] += photon.DP.Weight * momentumTransfer;
                    if (_rng.NextDouble() < _bloodVolumeFraction[csr]) // hit blood 
                    {
                        tissueMt[1] += momentumTransfer;
                        DynamicMTOfZ[ix, iy, iz] += photon.DP.Weight * momentumTransfer;
                        dynamicMtOfZForOnePhoton[ix, iy, iz] += photon.DP.Weight * momentumTransfer;
                        SubregionCollisions[csr, 1] += 1; // add to dynamic collision count
                    }
                    else // index 0 captures static events
                    {
                        tissueMt[0] += momentumTransfer;
                        SubregionCollisions[csr, 0] += 1; // add to static collision count
                    }
                    talliedMt = true;
                }
                previousDp = currentDp;
                currentDp = nextDp;
            }
            if (totalMt > 0.0)  // only tally if momentum transfer accumulated
            {
                var imt = DetectorBinning.WhichBin(totalMt, MTBins.Count - 1, MTBins.Delta, MTBins.Start);
                Mean[ix, iy, imt] += photon.DP.Weight;
                if (TallySecondMoment)
                {
                    SecondMoment[ix, iy, imt] += photon.DP.Weight * photon.DP.Weight;
                    for (var i = 0; i < X.Count - 1; i++)
                    {
                        for (var j = 0; j < Y.Count - 1; j++)
                        {
                            for (var k = 0; k < Z.Count - 1; k++)
                            {
                                TotalMTOfZSecondMoment[i, j, k] += totalMtOfZForOnePhoton[i, j, k] *
                                                                totalMtOfZForOnePhoton[i, j, k];
                                DynamicMTOfZSecondMoment[i, j, k] += dynamicMtOfZForOnePhoton[i, j, k] *
                                                                  dynamicMtOfZForOnePhoton[i, j, k];
                            }
                        }
                    }
                }

                if (talliedMt) TallyCount++;

                // tally DYNAMIC fractional MT in each sub-region
                int ifrac;
                for (var isr = 0; isr < NumSubregions; isr++)
                {
                    // add 1 to ifrac to offset bin 0 added for =0 only tallies
                    ifrac = DetectorBinning.WhichBin(tissueMt[1] / totalMt,
                        FractionalMTBins.Count - 1, FractionalMTBins.Delta, FractionalMTBins.Start) + 1;
                    // put identically 0 fractional MT into separate bin at index 0
                    if (tissueMt[1] / totalMt == 0.0)
                    {
                        ifrac = 0;
                    }
                    // put identically 1 fractional MT into separate bin at index Count+1 -1
                    if (tissueMt[1] / totalMt == 1.0)
                    {
                        ifrac = FractionalMTBins.Count;
                    }
                    FractionalMT[ix, iy, imt, ifrac] += photon.DP.Weight;
                }
            }       
        }

        /// <summary>
        /// method to normalize detector results after all photons launched
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            var areaNorm = X.Delta * Y.Delta;
            for (var ix = 0; ix < X.Count - 1; ix++)
            {
                for (var iy = 0; iy < Y.Count - 1; iy++)
                {
                    for (var imt = 0; imt < MTBins.Count - 1; imt++)
                    {
                        // normalize by area dx * dy and N
                        Mean[ix, iy, imt] /= areaNorm*numPhotons;
                        if (TallySecondMoment)
                        {
                            SecondMoment[ix, iy, imt] /= areaNorm*areaNorm*numPhotons;
                        }
                        for (var ifrac = 0; ifrac < FractionalMTBins.Count + 1; ifrac++)
                        {
                            FractionalMT[ix, iy, imt, ifrac] /= areaNorm * numPhotons;
                        }
                    }
                    for (var iz = 0; iz < Z.Count - 1; iz++)
                    {
                        TotalMTOfZ[ix, iy, iz] /= areaNorm * numPhotons;
                        DynamicMTOfZ[ix, iy, iz] /= areaNorm * numPhotons;
                        if (TallySecondMoment)
                        {
                            TotalMTOfZSecondMoment[ix, iy, iz] /= areaNorm * areaNorm * numPhotons;
                            DynamicMTOfZSecondMoment[ix, iy, iz] /= areaNorm * areaNorm * numPhotons;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// this is to allow saving of large arrays separately as a binary file
        /// </summary>
        /// <returns>BinaryArraySerializer[]</returns>
        public BinaryArraySerializer[] GetBinarySerializers()
        {
            Mean ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            FractionalMT ??= new double[X.Count - 1, Y.Count - 1, MTBins.Count - 1, FractionalMTBins.Count + 1];
            TotalMTOfZ ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            DynamicMTOfZ ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            SubregionCollisions ??= new double[NumSubregions, 2];
            if (TallySecondMoment)
            {
                SecondMoment ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
                TotalMTOfZSecondMoment ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
                DynamicMTOfZSecondMoment ??= new double[X.Count - 1, Y.Count - 1, Z.Count - 1];
            }

            var allSerializers = new List<BinaryArraySerializer>
            {
                BinaryArraySerializerFactory.GetSerializer(
                    Mean, "Mean", ""),
                BinaryArraySerializerFactory.GetSerializer(
                    FractionalMT, "FractionalMT", "_FractionalMT"),
                BinaryArraySerializerFactory.GetSerializer(
                    TotalMTOfZ, "TotalMTOfZ", "_TotalMTOfZ"),
                BinaryArraySerializerFactory.GetSerializer(
                    DynamicMTOfZ, "DynamicMTOfZ", "_DynamicMTOfZ"),
                BinaryArraySerializerFactory.GetSerializer(
                    SubregionCollisions, "SubregionCollisions", "_SubregionCollisions"),
                TallySecondMoment ? BinaryArraySerializerFactory.GetSerializer(
                        SecondMoment, "SecondMoment", "_2") : null,
                TallySecondMoment ? BinaryArraySerializerFactory.GetSerializer(
                        TotalMTOfZSecondMoment, "TotalMTOfZSecondMoment", "_TotalMTOfZ_2") : null,
                TallySecondMoment ? BinaryArraySerializerFactory.GetSerializer(
                        DynamicMTOfZSecondMoment, "DynamicMTOfZSecondMoment", "_DynamicMTOfZ_2") : null
            };
            return allSerializers.Where(s => s is not null).ToArray();

        }
        /// <summary>
        /// Method to determine if photon is within detector NA
        /// </summary>
        /// <param name="photon">photon</param>
        /// <returns>Boolean indicating whether photon is within detector</returns>
        public bool IsWithinDetectorAperture(Photon photon)
        {
            if (photon.CurrentRegionIndex == FinalTissueRegionIndex)
            {
                var detectorRegionN = _tissue.Regions[photon.CurrentRegionIndex].RegionOP.N;
                return photon.DP.IsWithinNA(NA, Direction.AlongPositiveZAxis, detectorRegionN);
            }
            else // determine n of prior tissue region
            {
                var detectorRegionN = _tissue.Regions[FinalTissueRegionIndex].RegionOP.N;
                return photon.History.PreviousDP.IsWithinNA(NA, Direction.AlongPositiveZAxis, detectorRegionN);
            }
        }

    }
}
