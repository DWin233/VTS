using System;
using System.Runtime.Serialization;
using Vts.Common;
using Vts.MonteCarlo.Helpers;
using Vts.MonteCarlo.PhotonData;

namespace Vts.MonteCarlo.Detectors
{
    /// <summary>
    /// Implements IDetector&lt;double[,]&gt;.  Tally for reflectance as a function 
    /// of Rho and Time.
    /// This implementation works for Analog, DAW and CAW processing.
    /// </summary>
    [KnownType(typeof(ROfRhoAndTimeDetector))]
    public class ROfRhoAndTimeDetector : IDetectorOld<double[,]> 
    {
        private bool _tallySecondMoment;
        /// <summary>
        /// constructor for reflectance as a function of rho and time detector input
        /// </summary>
        /// <param name="rho">rho binning</param>
        /// <param name="time">time binning</param>
        /// <param name="tallySecondMoment">flag indicating whether to tally second moment info for error results</param>
        /// <param name="name">detector name</param>
        public ROfRhoAndTimeDetector(DoubleRange rho, DoubleRange time, bool tallySecondMoment, String name)
        {
            Rho = rho;
            Time = time;
            _tallySecondMoment = tallySecondMoment;
            Mean = new double[Rho.Count - 1, Time.Count - 1];
            SecondMoment = null;
            if (_tallySecondMoment)
            {
                SecondMoment = new double[Rho.Count - 1, Time.Count - 1];
            }
            TallyType = "ROfRhoAndTime";
            Name = name;
            TallyCount = 0;
        }

        /// <summary>
        /// Returns a default instance of ROfRhoAndTimeDetector (for serialization purposes only)
        /// </summary>
        public ROfRhoAndTimeDetector()
            : this(
            new DoubleRange(),  
            new DoubleRange(),
            true,
            "ROfRhoAndTime")
        {
        }
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
        /// detector identifier
        /// </summary>
        public string TallyType { get; set; }
        /// <summary>
        /// detector name, default uses TallyType, but can be user specified
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// number of times detector gets tallied to
        /// </summary>
        public long TallyCount { get; set; }
        /// <summary>
        /// rho binning
        /// </summary>
        public DoubleRange Rho { get; set; }
        /// <summary>
        /// time binning
        /// </summary>
        public DoubleRange Time { get; set; }

        /// <summary>
        /// method to tally to detector
        /// </summary>
        /// <param name="photon">photon data needed to tally</param>
        public void Tally(Photon photon)
        {
            var it = DetectorBinning.WhichBin(photon.DP.TotalTime, Time.Count - 1, Time.Delta, Time.Start);
            var ir = DetectorBinning.WhichBin(DetectorBinning.GetRho(photon.DP.Position.X, photon.DP.Position.Y), Rho.Count - 1, Rho.Delta, Rho.Start);

            Mean[ir, it] += photon.DP.Weight;
            if (_tallySecondMoment)
            {
                SecondMoment[ir, it] += photon.DP.Weight * photon.DP.Weight;
            }
            TallyCount++;
        }

        /// <summary>
        /// method to normalize detector results after numPhotons launched
        /// </summary>
        /// <param name="numPhotons">number of photons launched</param>
        public void Normalize(long numPhotons)
        {
            var normalizationFactor = 2.0 * Math.PI * Rho.Delta * Time.Delta;
            for (int ir = 0; ir < Rho.Count - 1; ir++)
            {
                for (int it = 0; it < Time.Count - 1; it++)
                {
                    var areaNorm = (Rho.Start + (ir + 0.5) * Rho.Delta) * normalizationFactor;
                    Mean[ir, it] /= areaNorm * numPhotons;
                    if (_tallySecondMoment)
                    {
                        SecondMoment[ir, it] /= areaNorm * areaNorm * numPhotons;
                    }
                }
            }
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
