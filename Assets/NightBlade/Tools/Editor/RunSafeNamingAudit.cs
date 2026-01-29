using UnityEditor;
using NightBlade.Tools.Editor;

/// <summary>
/// Safe script to run the naming convention audit from the Unity console.
/// This tool ONLY AUDITS - it never modifies your code!
/// </summary>
public class RunSafeNamingAudit
{
    [MenuItem("NightBlade/Tools/Quick Safe Audit Run", false, 102)]
    static void ExecuteSafeAudit()
    {
        var auditor = new SafeNamingConventionAuditor();
        auditor.RunConsoleAudit();

        EditorUtility.DisplayDialog(
            "Safe Audit Complete",
            "The naming convention audit has completed safely.\n\n" +
            "⚠️  IMPORTANT: This tool ONLY AUDITS your code.\n" +
            "It never modifies or changes any files automatically.\n\n" +
            "Check the Unity console for detailed results!",
            "OK"
        );
    }
}