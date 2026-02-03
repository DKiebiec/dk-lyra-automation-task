using UnrealBuildTool;

public class AltAimHelper : ModuleRules
{
    public AltAimHelper(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(
            new string[] {
                "Core",
                "CoreUObject",
                "Engine",
                "InputCore",
                "GameplayTasks",
                "UMG"
            }
        );

        PrivateDependencyModuleNames.AddRange(new string[] { });
    }
}