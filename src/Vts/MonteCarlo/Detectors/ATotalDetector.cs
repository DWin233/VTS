using System;
using System.Collections.Generic;
using System.Linq;
using Vts.IO;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Tally for total absorption.
    /// This implementation works for Analog, DAW and CAW.
    /// </summary>
    public class ATotalDetectorInput : DetectorInput, IDetectorInput
    {
        /// <summary>
        /// constructor for total absorption detector input
        /// </summary>
        public ATotalDetectorInput()
        {
            TallyType = "ATotal";
            Name = "ATotal";

            // modify base class TallyDetails to take advantage of built-in validation capabilities (error-checking)
            TallyDetails.IsVolumeTally = true;
        }

        /// <summary>
        /// Method to create detector from detector input
        /// </summary>
        /// <returns>created IDetector</returns>
        public IDetector CreateDetector()
        {
            return new ATotalDetector
            {
                // required properties (part of DetectorInput/Detector base classes)
                TallyType = this.TallyType,
                Name = this.Name,
                TallySecondMoment = this.TallySecondMoment,
                TallyDetails = this.TallyDetails,

                // optional/custom detector-specific properties
            };
        }
    }

    /// <summary>
    /// Implements IDetector.  Tally for total absorbed energy.
    /// This implementation works for Analog, DAW processing.
    /// </summary>
    public class ATotalDetector : Detector, IHistoryDetector
    {
        private double _tallyForOnePhoton;
        /* ==== Place optional/user-defined input properties here. They will be saved in text (JSON) format ==== */
        /* ==== Note: make sure to copy over all optional/user-defined inputs from corresponding input class ==== */

        /* ==== Place user-defined output arrays here. They should be prepended with "[IgnoreDataMember]" attribute ==== */
        /* ==== Then, GetBinaryArrays() should be implemented to save them separately in binary format ==== */
        /// <summary>
        /// detector mean
        /// </summary>
        public double Mean { get; set; }
        /// <summary>
        /// detector second moment
        /// </summary>
        public double SecondMoment { get; set; }

        /* ==== Place optional/user-defined output properties here. They will be saved in text (JSON) format ==== */
        /// <summary>
        /// number of times detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }

        private Func<PhotonDataPoint, PhotonDataPoint, int, double> _absorptionWeightingMethod;
        private ITissue _tissue;
        private IList<OpticalProperties> _ops;

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
            //Mean = Mean ?? new double();
            //SecondMoment = SecondMoment ?? (TallySecondMoment ? new double() : null);
            Mean = new double();
            if (TallySecondMoment)
            {
                SecondMoment = new double();
            }

            // initialize any other necessary class fields here
            _absorptionWeightingMethod = AbsorptionWeightingMethods.GetVolumeAbsorptionWeightingMethod(tissue, this);
            _tissue = tissue;
            _ops = _tissue.Regions.Select(r => r.RegionOP).ToArray();
        }
        /// <summary>
        /// method to tally a single photon collision
        /// </summary>
        /// <param name="previousDP">previous data point</param>
        /// <param name="dp">current data point</param>
        /// <param name="currentRegionIndex">current tissue region index</param>
        public void TallySingle(PhotonDataPoint previousDP, PhotonDataPoint dp, int currentRegionIndex)
        {
            var weight = _absorptionWeightingMethod(previousDP, dp, currentRegionIndex);

            if (weight > 0.0)
            {
                Mean += weight;
                if (TallySecondMoment)
                {
                    _tallyForOnePhoton += weight;
                }
                TallyCount++;
            }
        }

        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {            
            // second moment is calculated AFTER the entire photon biography has been processed
            _tallyForOnePhoton = 0.0;
           
            PhotonDataPoint previousDP = photon.History.HistoryData.First();
            foreach (PhotonDataPoint dp in photon.History.HistoryData.Skip(1))
            {
                TallySingle(previousDP, dp, _tissue.GetRegionIndex(dp.Position)); // unoptimized version, but HistoryDataController calls this once
                previousDP = dp;
            }            
            if (TallySecondMoment)
            {
                SecondMoment += _tallyForOnePhoton * _tallyForOnePhoton;
            }
        }

        /// <summary>
        /// method to normalize detector results after numPhotons are launched
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            Mean /= numPhotons;
            if (TallySecondMoment)
            {
                SecondMoment /= numPhotons;
            }
        }

        /// <summary>
        /// this scalar tally is saved to json
        /// </summary>
        /// <returns></returns>
        public BinaryArraySerializer[] GetBinarySerializers()
        {
            return null;
        }
        /// <summary>
        /// Method to determine if photon is within detector
        /// </summary>
        /// <param name="photon">photon</param>
        public bool IsWithinDetectorAperture(Photon photon)
        {
            return true; // or, possibly test for NA or confined position, etc
        }
    }
}
