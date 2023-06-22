using NUnit.Framework;

namespace Vts.Benchmark.Test
{
    [TestFixture]
    public class ProgramTest
    {
        // LM: possibly I don't need this because the Benchmark Program runs MCCL
        // already?  But what infile does it use?

        /// <summary>
        /// clear all previously generated folders and files.
        /// </summary>

        // Note: needs to be kept current with SimulationInputProvider.  If an infile is added there, it should be added here.

        private readonly List<string> _listOfMcclInfiles = new List<string>()
        {
            "ellip_FluenceOfRhoAndZ",
            "infinite_cylinder_AOfXAndYAndZ",
            "multi_infinite_cylinder_AOfXAndYAndZ",
            "fluorescence_emission_AOfXAndYAndZ_source_infinite_cylinder",
            "embedded_directional_circular_source_ellip_tissue",
            "Flat_2D_source_one_layer_ROfRho",
            "Flat_2D_source_two_layer_bounded_AOfRhoAndZ",
            "Gaussian_2D_source_one_layer_ROfRho",
            "Gaussian_line_source_one_layer_ROfRho",
            "one_layer_all_detectors",
            "one_layer_FluenceOfRhoAndZ_RadianceOfRhoAndZAndAngle",
            "one_layer_ROfRho_FluenceOfRhoAndZ",
            "pMC_one_layer_ROfRho_DAW",
            "three_layer_ReflectedTimeOfRhoAndSubregionHist",
            "two_layer_momentum_transfer_detectors",
            "two_layer_ROfRho",
            "two_layer_ROfRho_TOfRho_with_databases",
            "voxel_ROfXAndY_FluenceOfXAndYAndZ",
            "surface_fiber_detector"
        };

        private const string BenchmarkFolder = "BenchmarkDotNet.Artifacts";

        /// <summary>
        /// clear all previously generated folders and files, then regenerate sample infiles using "geninfiles" option.
        /// </summary>
        [OneTimeSetUp]
        public void Setup()
        {
            Clear_folders_and_files();

            var arguments = new string[] { "geninfiles" };
            // generate sample MCCL infiles to obtain infile to run
            MonteCarlo.CommandLineApplication.Program.Main(arguments);

            // run MCCL with default infile with all detectors specified
            arguments = new string[] { "infile=infile_one_layer_all_detectors" };
            MonteCarlo.CommandLineApplication.Program.Main(arguments);
        }

        [OneTimeTearDown]
        public void Clear_folders_and_files()
        {
            // delete any previously generated infiles to test that "geninfiles" option creates them
            foreach (var infile in _listOfMcclInfiles)
            {
                if (File.Exists("infile_" + infile + ".txt"))
                {
                    File.Delete("infile_" + infile + ".txt");
                }

                if (Directory.Exists(infile))
                {
                    Directory.Delete(infile, true); // delete recursively
                }
            }

            // delete any previously generated benchmark logs
            if (Directory.Exists(BenchmarkFolder))
            {
                Directory.Delete(BenchmarkFolder, true);
            }
        }


        /// <summary>
        /// Test to obtain output from Benchmark program and verify its contents
        /// </summary>
        [Test]
        public void Validate_Benchmark_output()
        {
            // need to build and run this in configuration in "Benchmark"
            var arguments = new string[] { "" };
            Program.Main(arguments);
            // read in output file
            if (!Directory.Exists(BenchmarkFolder)) return;

            // need to figure out how to use wild card for file name
            const string logFile = BenchmarkFolder + "/Vts.MonteCarlo.MonteCarloSimulation-*.log";
            if (File.Exists(logFile))
            {
                // open file to read statistics
                var text = File.ReadAllText(logFile);

            }

        }
    }
}