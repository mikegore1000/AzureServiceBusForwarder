#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument<string>("target");

Task("Default")
	.Does(() => 
{
	NuGetRestore("../src/AzureServiceBusForwarder.sln");

	MSBuild("../src/AzureServiceBusForwarder.sln", 
		config => config.SetConfiguration("Release")
	);

	NUnit3("../src/AzureServiceBusForwarder.Tests/bin/Release/AzureServiceBusForwarder.Tests.dll");
	NUnit3("../src/AzureServiceBusForwarder.IntegrationTests/bin/Release/AzureServiceBusForwarder.IntegrationTests.dll");

	NuGetPack("../src/AzureServiceBusForwarder/AzureServiceBusForwarder.csproj", new NuGetPackSettings() {
		ArgumentCustomization = args => args.Append("-Prop Configuration=Release")
	});
});

Task("Deploy")
	.IsDependentOn("Default")
	.Does(() => 
{
	var nugetSource = Argument<string>("nugetSource");
	var nugetApiKey = Argument<string>("nugetApiKey");

	var package = GetFiles("./AzureServiceBusForwarder*.nupkg");

	NuGetPush(package, new NuGetPushSettings {
		Source = nugetSource,
		ApiKey = nugetApiKey
	});
});


RunTarget(target);