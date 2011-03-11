using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using Vts.MonteCarlo.Detectors;
using Vts.MonteCarlo.Interfaces;
using Vts.MonteCarlo.PhotonData;
using Vts.MonteCarlo.Tissues;

namespace Vts.MonteCarlo.PostProcessing
{
    /// <summary>
    /// Sets up and postprocesses Monte Carlo termination data that has been 
    /// saved in a database.
    /// </summary>
    public class PhotonTerminationDatabasePostProcessor
    {
        /// <summary>
        /// GenerateOutput takes IDetectorInput (which designates tallies), reads PhotonExitHistory, and generates 
        /// Output.  This runs the conventional post-processing.
        /// </summary>
        /// <param name="detectorInputs">List of IDetectorInputs designating binning</param>
        /// <param name="database">PhotonTerminationDatabase</param>
        /// <param name="databaseInput">Database information needed for post-processing</param>
        /// <returns></returns>
        public static Output GenerateOutput(
            IList<IDetectorInput> detectorInputs, 
            PhotonDatabase database, 
            SimulationInput databaseInput)
        {
            Output postProcessedOutput = new Output();

            ITissue tissue = Factories.TissueFactory.GetTissue(
                databaseInput.TissueInput,
                databaseInput.Options.AbsorptionWeightingType,
                databaseInput.Options.PhaseFunctionType);

            DetectorController detectorController = Factories.DetectorControllerFactory.GetStandardDetectorController(detectorInputs, tissue);

            foreach (var dp in database.DataPoints)
            {
                detectorController.TerminationTally(dp);     
            }

            detectorController.NormalizeDetectors(databaseInput.N);

            // todo: call output generation method on detectorController (once it's implemented)
            return postProcessedOutput;
        }

        /// <summary>
        /// pMC overload
        /// GenerateOutput takes IDetectorInput (which designates tallies),
        /// reads PhotonExitHistory, and generates Output.
        /// </summary>
        /// <param name="detectorInputs>List of IDetectorInputs designating binning</param>
        /// <param name="database">PhotonTerminationDatabase</param>
        /// <param name="databaseInput">Database information needed for post-processing</param>
        /// <param name="perturbedOps">Perturbed optical properties</param>
        /// <param name="perturbedRegionsIndices">Indices of regions being perturbed</param>
        /// <returns></returns>
        public static Output GenerateOutput(
            IList<IpMCDetectorInput> detectorInputs, 
            PhotonDatabase database, 
            SimulationInput databaseInput,
            List<OpticalProperties> perturbedOps,
            List<int> perturbedRegionsIndices)
        {
            Output postProcessedOutput = new Output();

            ITissue tissue = Factories.TissueFactory.GetTissue(
                databaseInput.TissueInput, 
                databaseInput.Options.AbsorptionWeightingType,
                databaseInput.Options.PhaseFunctionType);

            pMCDetectorController detectorController = Factories.DetectorControllerFactory.GetpMCDetectorController(detectorInputs, tissue);
            IList<SubRegionCollisionInfo> collisionInfo = null; // todo: revisit
            foreach (var dp in database.DataPoints)
            {
                detectorController.TerminationTally(dp, collisionInfo);
            }

            detectorController.NormalizeDetectors(databaseInput.N);
            
            // todo: call output generation method on detectorController (once it's implemented)
            return postProcessedOutput;
        }

    }
}
