#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");

Task("Build")
  .Does(()=>{
	MSBuild("ConsulExec.sln", config=>config.SetConfiguration(configuration));
});

Task("Test")
  .IsDependentOn("Build")
  .Does(()=>{	  
	var assemblies = GetFiles($"./*.Tests/bin/{configuration}/*Test*.dll");
	NUnit3(assemblies);
});


Task("NugetRestore")
  .Does(()=>{
	var solutions = GetFiles("./**/*.sln");
	foreach(var solution in solutions)
	{
		Information("Restoring {0}", solution);
		NuGetRestore(solution);
	}
  });


Task("Pack")
  .IsDependentOn("Test")
  .Does(()=>{
	 ChocolateyPack("nuspec/consulexec.nuspec", new ChocolateyPackSettings { Version = EnvironmentVariable("APPVEYOR_BUILD_VERSION") ?? "1.0.0.0" } );
  });

  
Task("AppVeyor")
  .IsDependentOn("NugetRestore")
  .IsDependentOn("Pack")
  .Does(()=>{
	  
  });

RunTarget(target);
