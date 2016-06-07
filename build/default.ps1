framework '4.6x86'

properties {
    $base_directory = Resolve-Path ..
    $publish_directory = "$base_directory\.build\publish"
    $build_output_root_directory = "$base_directory\.build"
    $build_directory = "$base_directory\build"
    $src_directory = "$base_directory\src"
    $test_directory = "$base_directory\test"
    $samples_directory = "$base_directory\samples"
    $output_directory = "$base_directory\.build\output"
    $packages_directory = "$src_directory\packages"
    $sln_file = "$base_directory\Even.sln"
    $src_projects = gci $src_directory -Include *.csproj -Recurse
    $test_projects = gci $test_directory -Include *.csproj -Recurse
    $sample_projects = gci $samples_directory -Include *.csproj -Recurse
    $projects = $src_projects + $test_projects + $sample_projects  
    $target_config = "Release"
    $framework_version = "v4.0"

    $assemblyInfoFilePath = "$base_directory\CommonAssemblyInfo.cs"

    $xunit_path = "$base_directory\bin\xunit.runners.1.9.1\tools\xunit.console.clr4.exe"
    $ilMergeModule.ilMergePath = "$base_directory\bin\ilmerge-bin\ILMerge.exe"
    $nuget_dir = "$src_directory\.nuget"

    if($build_number -eq $null) {
		$build_number = 0
	}

    if($runPersistenceTests -eq $null) {
    	$runPersistenceTests = $false
    }

	if($offline -eq $null) {
		$offline = $false
	}
}

FormatTaskName (("-"*25) + "[{0}]" + ("-"*30))

task default -depends Build

task Build -depends Clean, version, Compile, Test
task Clean -depends clean-directories, clean-source-projects, clean-test-projects, clean-sample-projects
task Test -depends RunUnitTests

task clean-source-projects {    
    foreach ($project in $src_projects) {
        exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /t:Clean }	            
    }
}

task clean-test-projects {    
    foreach ($project in $test_projects) {
        exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /t:Clean }	            
    }
}

task clean-sample-projects {
    foreach ($project in $sample_projects) {
        exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /t:Clean }	            
    }
}

task compile-source-projects -depends clean-source-projects {
    foreach ($project in $src_projects) {
	    exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.5.1 }        
    }    
}

task compile-test-projects -depends clean-test-projects {
    foreach ($project in $test_projects) {
	    exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.5.1 }        
    }    
}

task compile-sample-projects -depends clean-sample-projects {
    foreach ($project in $sample_projects) {
	    exec { msbuild /nologo /verbosity:quiet $project /p:Configuration=$target_config /p:TargetFrameworkVersion=v4.5.1 }        
    }    
}

task Compile -depends compile-source-projects, compile-test-projects, compile-sample-projects {    
}

task RunUnitTests -depends acquire-testingtools {
	"Unit Tests"
	EnsureDirectory $output_directory
    $xunit_path = $script:xunit_path
    $tests = ls ("{0}/*/bin/$target_config" -f $test_directory) -Include '*.Tests.dll' -Recurse 
    $hasError = $false
    foreach ($test in $tests) {
        $testOutput = [System.IO.Path]::ChangeExtension($test, "testresults.xml")        
        $cmdArgs = @("$test")
        $cmdArgs += "-xml"
        $cmdArgs += '"{0}"' -f $testOutput
        & $xunit_path $cmdArgs
        if( $LASTEXITCODE -ne 0) {
            $hasError = $true
        }             
    }    

    if ($hasError) {
        throw "RunUnitTests: Error encountered while executing xunit tests"
    }
}

task Package -depends Build {
	move $output_directory $publish_directory
}

task clean-directories {
    Clean-Item $publish_directory -ea SilentlyContinue
    Clean-Item $output_directory -ea SilentlyContinue
    Clean-Item $build_output_root_directory
    EnsureDirectory $build_output_root_directory
}

task NuGetPack -depends Package {
    $versionString = Get-Version $assemblyInfoFilePath
	$version = New-Object Version $versionString
	$packageVersion = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString() + "-build" + $build_number.ToString().PadLeft(5,'0')
	gci -r -i *.nuspec "$nuget_dir" |% { .$nuget_dir\nuget.exe pack $_ -basepath $base_directory -o $publish_directory -version $packageVersion }
}

task acquire-testingtools {
    Write-Host  "Acquiring XUnit..."
    $script:xunit_path = Get-XUnitRunnerPath
    Set-XUnitPath $script:xunit_path 
} 

task acquire-buildtools -precondition {-not $offline} {
    Write-Host  "Acquiring GitVersion.CommandLine..."
    $script:GitVersionExe = Get-GitVersionCommandline    
}

task version -depends acquire-buildtools {    
    $versionInfo = Invoke-GitVersion -UpdateAssemblyInfo -AssemblyInfoPaths $assemblyInfoFilePath -GitVersionExePath $script:GitVersionExe
    Write-Host "VersionInfo: $versionInfo"    
}

function EnsureDirectory {
	param($directory)

	if(!(test-path $directory))
	{
		mkdir $directory
	}
}
