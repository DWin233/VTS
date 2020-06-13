﻿$version = $args[0];
$runtime = $args[1];

$vtslevel = $PWD
Write-Host "Build Vts library Debug & Release" -ForegroundColor Green
dotnet build $PWD\src\Vts\Vts.csproj -c Debug
dotnet build $PWD\src\Vts\Vts.csproj -c Release

Write-Host "Build MCCL Debug, Release" -ForegroundColor Green
$mcclcsproj = "$PWD\src\Vts.MonteCarlo.CommandLineApplication\Vts.MonteCarlo.CommandLineApplication.csproj"
dotnet build $mcclcsproj -c Debug
dotnet build $mcclcsproj -c Release

Write-Host "Build MCPP Debug, Release" -ForegroundColor Green
$mcppcsproj = "$PWD\src\Vts.MonteCarlo.PostProcessor\Vts.MonteCarlo.PostProcessor.csproj"
dotnet build $mcppcsproj -c Debug
dotnet build $mcppcsproj -c Release

Write-Host "Build Vts.Test Debug & Release" -ForegroundColor Green
dotnet build $PWD\src\Vts.Test\Vts.Test.csproj -c Debug
dotnet build $PWD\src\Vts.Test\Vts.Test.csproj -c Release
Write-Host "Run Vts.Test Debug and Release" -ForegroundColor Green
dotnet test $PWD\src\Vts.Test\Vts.Test.csproj -c Debug
dotnet test $PWD\src\Vts.Test\Vts.Test.csproj -c Release

Write-Host "Release Packages" -ForegroundColor Green
Write-Host "Clean Release folders" -ForegroundColor Green
Remove-Item "$PWD/release" -Recurse -ErrorAction Ignore
if (Test-Path $PWD\publish) {
  Remove-Item $PWD\publish -Recurse -ErrorAction Ignore
}
New-Item -Path $PWD -Name ".\publish\$runtime" -ItemType "directory"

# run next 2 line prior to RunMATLABUnitTests to setup up publish with results
dotnet build $mcclcsproj -c Release -r $runtime -o $PWD\publish\$runtime 
dotnet build $mcppcsproj -c Release -r $runtime -o $PWD\publish\$runtime 

Write-Host "version = $version runtime = $runtime" -ForegroundColor Green

$builddir = ".\release\$runtime"
if (Test-Path "$builddir") {
  Remove-Item "$builddir" -Recurse -ErrorAction Ignore
}
New-Item -Path $PWD -Name $builddir -ItemType "directory"
$archive = $builddir + "\MC_v" + $version + "Beta.zip"
$source = "publish\$runtime\*"

Compress-Archive -Path $source -DestinationPath $archive 

$matlabfiles = "$PWD\matlab\post_processing\monte_carlo\simulation_result_loading\*"
Compress-Archive -Path $matlabfiles -Update -DestinationPath $archive

Write-Host "Run MCCL MATLAB post-processing tests" -ForegroundColor Green
# Change current dir to publish 
cd "$vtslevel\publish\$runtime"
#$PWD
# Generate infiles and run Monte Carlo with general infile
.\mc.exe geninfiles
.\mc.exe infile=infile_one_layer_all_detectors.txt

# Change current dir to MATLAB Monte Carlo post-processing
cd "$vtslevel\matlab\post_processing\monte_carlo\simulation_result_loading"

# remove any residual folder
# Copy results from Monte Carlo to current directory 
$MCmatlabdir = "$vtslevel\matlab\post_processing\monte_carlo\simulation_result_loading\one_layer_all_detectors"
Remove-Item  $MCmatlabdir -Recurse -ErrorAction Ignore
New-Item $MCmatlabdir -ItemType "directory"
$MCresults = "$vtslevel\publish\$runtime\one_layer_all_detectors\*"
Copy-Item -Path $MCresults -Destination $MCmatlabdir -Recurse -ErrorAction Ignore

# run load_results_script (default datanames is set to one_layer_all_detectors) 
matlab -wait -r "load_results_script; quit"
# cd back to start
cd $vtslevel

Read-Host -Prompt "Press Enter to exit MCCL build process"