using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace NightBlade.Tools.Editor
{
    /// <summary>
    /// SAFE naming Convention auditor for NightBlade codebase.
    /// Scans ONLY NightBlade C# files and identifies violations without modifying anything.
    /// </summary>
    public class SafeNamingConventionAuditor : EditorWindow
    {
        private const string NIGHTBLADE_ASSETS_PATH = "Assets/NightBlade";

        // UI state
        private Vector2 _scrollPosition;
        private bool _scanSubfolders = true;
        private bool _includeTestFiles = false;
        private string _searchPath = NIGHTBLADE_ASSETS_PATH;

        // Results
        private List<NamingViolation> violations = new List<NamingViolation>();
        private bool _scanComplete = false;
        private int _totalFilesScanned = 0;
        private double _scanTime = 0;

        // Violation categories
        private Dictionary<ViolationType, bool> showCategory = new Dictionary<ViolationType, bool>
        {
            { ViolationType.ClassName, true },
            { ViolationType.MethodName, true },
            { ViolationType.PropertyName, true },
            { ViolationType.FieldName, true },
            { ViolationType.ConstantName, true },
            { ViolationType.ParameterName, true },
            { ViolationType.LocalVariable, true },
            { ViolationType.FileName, true }
        };

        [MenuItem("NightBlade/Tools/Safe Naming Convention Audit", false, 100)]
        static void ShowWindow()
        {
            var window = GetWindow<SafeNamingConventionAuditor>("Safe Naming Auditor");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        [MenuItem("NightBlade/Tools/Run Safe Naming Audit (Console)", false, 101)]
        static void RunAuditFromMenu()
        {
            var auditor = new SafeNamingConventionAuditor();
            auditor.RunConsoleAudit();
            Debug.Log("Safe naming Convention audit completed. Check the console for results.");
        }

        public void RunConsoleAudit()
        {
            violations.Clear();
            _scanComplete = false;

            double startTime = EditorApplication.timeSinceStartup;

            try
            {
                string[] csFiles = GetCsFilesToScan();
                _totalFilesScanned = csFiles.Length;

                foreach (string filePath in csFiles)
                {
                    AnalyzeFile(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during scan: {ex.Message}");
            }

            _scanTime = EditorApplication.timeSinceStartup - startTime;
            _scanComplete = true;

            // Sort violations by severity
            violations = violations
                .OrderByDescending(v => v.Severity)
                .ThenBy(v => v.FilePath)
                .ThenBy(v => v.LineNumber)
                .ToList();

            // Output results to console
            OutputConsoleResults();
        }

        private void OutputConsoleResults()
        {
            Debug.Log($"🔍 NightBlade SAFE Naming Convention Audit Complete!");
            Debug.Log($"📊 Files scanned: {_totalFilesScanned}");
            Debug.Log($"⏱️ Scan time: {_scanTime:F2} seconds");
            Debug.Log($"🚨 Violations found: {violations.Count}");

            if (violations.Count == 0)
            {
                Debug.Log("✅ No naming violations found! Great job!");
                return;
            }

            Debug.Log("📋 Top 20 violations by severity:");

            var topViolations = violations.Take(20);
            int count = 1;

            foreach (var violation in topViolations)
            {
                string severityIcon = violation.Severity switch
                {
                    ViolationSeverity.Critical => "🚨",
                    ViolationSeverity.High => "⚠️",
                    ViolationSeverity.Medium => "ℹ️",
                    ViolationSeverity.Low => "💡",
                    _ => "❓"
                };

                Debug.Log($"{count:00}. {severityIcon} {Path.GetFileName(violation.FilePath)}:{violation.LineNumber} - {violation.Message}");
                count++;
            }

            if (violations.Count > 20)
            {
                Debug.Log($"... and {violations.Count - 20} more violations.");
            }

            // Summary by category
            var byCategory = violations.GroupBy(v => v.Type);
            Debug.Log("📈 Violations by category:");
            foreach (var group in byCategory.OrderByDescending(g => g.Count()))
            {
                Debug.Log($"   {group.Key}: {group.Count()}");
            }

            Debug.Log("💡 Use 'NightBlade/Tools/Safe Naming Convention Audit' to see full details!");
            Debug.Log("⚠️  This tool ONLY AUDITS - it does not modify any files!");
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawSettings();
            DrawActionButtons();

            if (_scanComplete)
            {
                DrawResults();
            }
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("🔍 NightBlade SAFE Naming Convention Auditor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Scans NightBlade C# files ONLY (excludes third-party libraries)", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox("⚠️  SAFETY FIRST: This tool ONLY AUDITS. It never modifies your code automatically!", MessageType.Warning);

            if (_scanComplete)
            {
                EditorGUILayout.HelpBox(
                    $"Scan complete! Found {violations.Count} violations in {_totalFilesScanned} files ({_scanTime:F2}s)",
                    violations.Count > 0 ? MessageType.Warning : MessageType.Info
                );
            }

            EditorGUILayout.Space();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("⚙️ Scan Settings", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Search Path:", GUILayout.Width(80));
                if (GUILayout.Button(_searchPath, EditorStyles.textField))
                {
                    string newPath = EditorUtility.OpenFolderPanel("Select folder to scan", _searchPath, "");
                    if (!string.IsNullOrEmpty(newPath))
                    {
                        // Convert absolute path to relative Assets path
                        if (newPath.Contains(Application.dataPath))
                        {
                            _searchPath = "Assets" + newPath.Substring(Application.dataPath.Length);
                        }
                    }
                }
            }

            _scanSubfolders = EditorGUILayout.Toggle("Include Subfolders", _scanSubfolders);
            _includeTestFiles = EditorGUILayout.Toggle("Include Test Files", _includeTestFiles);

            EditorGUILayout.Space();
        }

        private void DrawActionButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("🔍 Scan for Violations", GUILayout.Height(30)))
                {
                    ScanForViolations();
                }

                if (violations.Count > 0)
                {
                    if (GUILayout.Button("📋 Export Report", GUILayout.Height(30)))
                    {
                        ExportReport();
                    }
                }
            }

            EditorGUILayout.Space();
        }

        private void DrawResults()
        {
            EditorGUILayout.LabelField("📊 Results", EditorStyles.boldLabel);

            // Category filters
            EditorGUILayout.LabelField("Filter Categories:", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var category in showCategory.Keys.ToList())
                {
                    showCategory[category] = EditorGUILayout.ToggleLeft(
                        category.ToString(),
                        showCategory[category],
                        GUILayout.Width(120)
                    );
                }
            }

            EditorGUILayout.Space();

            // Results list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            var filteredViolations = violations.Where(v => showCategory[v.Type]).ToList();

            if (filteredViolations.Count == 0)
            {
                EditorGUILayout.HelpBox("No violations found in selected categories!", MessageType.Info);
            }
            else
            {
                // Group by file
                var groupedByFile = filteredViolations.GroupBy(v => v.FilePath);

                foreach (var fileGroup in groupedByFile.OrderBy(g => g.Key))
                {
                    bool fileFoldout = EditorGUILayout.Foldout(true, $"{Path.GetFileName(fileGroup.Key)} ({fileGroup.Count()} violations)", true);

                    if (fileFoldout)
                    {
                        EditorGUI.indentLevel++;

                        foreach (var violation in fileGroup.OrderBy(v => v.LineNumber))
                        {
                            DrawViolation(violation);
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawViolation(NamingViolation violation)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // Severity indicator
                GUI.color = GetSeverityColor(violation.Severity);
                GUILayout.Label("●", GUILayout.Width(15));
                GUI.color = Color.white;

                // Violation details
                EditorGUILayout.LabelField($"Line {violation.LineNumber}: {violation.Message}", EditorStyles.miniLabel);

                // Go to line button
                if (GUILayout.Button("📍 Go To", GUILayout.Width(50)))
                {
                    GoToViolation(violation);
                }
            }
        }

        private Color GetSeverityColor(ViolationSeverity severity)
        {
            return severity switch
            {
                ViolationSeverity.Critical => Color.red,
                ViolationSeverity.High => new Color(1f, 0.5f, 0f), // Orange
                ViolationSeverity.Medium => Color.yellow,
                ViolationSeverity.Low => Color.gray,
                _ => Color.white
            };
        }

        private void ScanForViolations()
        {
            violations.Clear();
            _scanComplete = false;

            double startTime = EditorApplication.timeSinceStartup;

            try
            {
                string[] csFiles = GetCsFilesToScan();
                _totalFilesScanned = csFiles.Length;

                foreach (string filePath in csFiles)
                {
                    AnalyzeFile(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during scan: {ex.Message}");
            }

            _scanTime = EditorApplication.timeSinceStartup - startTime;
            _scanComplete = true;

            // Sort violations by severity then file
            violations = violations
                .OrderByDescending(v => v.Severity)
                .ThenBy(v => v.FilePath)
                .ThenBy(v => v.LineNumber)
                .ToList();

            Repaint();
        }

        private string[] GetCsFilesToScan()
        {
            // CRITICAL SAFETY: Only allow scanning within NightBlade folder
            if (!_searchPath.StartsWith("Assets/NightBlade"))
            {
                Debug.LogWarning($"SECURITY: Search path must be within Assets/NightBlade folder. Current path: {_searchPath}");
                _searchPath = NIGHTBLADE_ASSETS_PATH;
            }

            if (!Directory.Exists(_searchPath))
            {
                Debug.LogWarning($"Search path does not exist: {_searchPath}");
                return new string[0];
            }

            var searchOption = _scanSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(_searchPath, "*.cs", searchOption);

            if (!_includeTestFiles)
            {
                files = files.Where(f => !f.Contains("/Tests/") && !f.Contains("\\Tests\\") && !f.EndsWith("Test.cs")).ToArray();
            }

            // CRITICAL SAFETY: Never touch third-party code
            files = files.Where(f => !f.Contains("/ThirdParty/") && !f.Contains("\\ThirdParty\\")).ToArray();

            // CRITICAL SAFETY: Exclude editor tools to prevent self-corruption
            files = files.Where(f => !f.Contains("/Tools/Editor/") && !f.Contains("\\Tools\\Editor\\")).ToArray();

            // CRITICAL SAFETY: Exclude this auditor tool itself
            files = files.Where(f => !f.Contains("SafeNamingConventionAuditor.cs")).ToArray();

            return files;
        }

        private void AnalyzeFile(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string content = File.ReadAllText(filePath);

                // Analyze class names
                CheckClassNames(filePath, content);

                // Analyze method names
                CheckMethodNames(filePath, content);

                // Analyze property names
                CheckPropertyNames(filePath, content);

                // Analyze field names
                CheckFieldNames(filePath, content);

                // Analyze constants
                CheckConstants(filePath, content);

                // Analyze file name
                CheckFileName(filePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing {filePath}: {ex.Message}");
            }
        }

        private void CheckClassNames(string filePath, string content)
        {
            // Match class, struct, interface declarations
            var classRegex = new Regex(@"\b(?:class|struct|interface)\s+(\w+)");
            var matches = classRegex.Matches(content);

            foreach (Match match in matches)
            {
                string className = match.Groups[1].Value;

                if (!IsValidPascalCase(className))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.ClassName,
                        Severity = ViolationSeverity.High,
                        Message = $"Class/struct/interface '{className}' should be PascalCase",
                        CurrentValue = className,
                        SuggestedValue = ToPascalCase(className),
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
            }
        }

        private void CheckMethodNames(string filePath, string content)
        {
            // Match method declarations (simplified)
            var methodRegex = new Regex(@"(?:public|private|protected|internal)?\s*(?:static\s+)?(?:\w+\s+)+(\w+)\s*\(");
            var matches = methodRegex.Matches(content);

            foreach (Match match in matches)
            {
                string methodName = match.Groups[1].Value;

                // Skip constructors, property getters/setters, etc.
                if (IsConstructor(methodName, content, match.Index) ||
                    IsPropertyAccessor(methodName) ||
                    IsEventAccessor(methodName))
                    continue;

                if (!IsValidPascalCase(methodName))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.MethodName,
                        Severity = ViolationSeverity.High,
                        Message = $"Method '{methodName}' should be PascalCase",
                        CurrentValue = methodName,
                        SuggestedValue = ToPascalCase(methodName),
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
            }
        }

        private void CheckPropertyNames(string filePath, string content)
        {
            // Match property declarations
            var propertyRegex = new Regex(@"(?:public|private|protected|internal)?\s*(?:static\s+)?(?:\w+\s+)+(\w+)\s*\{");
            var matches = propertyRegex.Matches(content);

            foreach (Match match in matches)
            {
                string propertyName = match.Groups[1].Value;

                if (!IsValidPascalCase(propertyName))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.PropertyName,
                        Severity = ViolationSeverity.Medium,
                        Message = $"Property '{propertyName}' should be PascalCase",
                        CurrentValue = propertyName,
                        SuggestedValue = ToPascalCase(propertyName),
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
            }
        }

        private void CheckFieldNames(string filePath, string content)
        {
            // MUCH more specific regex - only match actual field declarations
            // Must start at line beginning (with optional whitespace), have explicit access modifier or [SerializeField]
            var fieldRegex = new Regex(@"^\s*(?:\[SerializeField\]\s*)?(?:public|private|protected|internal)\s+(?:static\s+)?(?:readonly\s+)?(?:const\s+)?[\w<>\[\],\.\s]+?\s+([a-zA-Z_]\w*)\s*(?:=|;)", RegexOptions.Multiline);
            var matches = fieldRegex.Matches(content);

            foreach (Match match in matches)
            {
                string fieldName = match.Groups[1].Value;

                // Skip numeric patterns (literals like 10f, 0f, etc.)
                if (Regex.IsMatch(fieldName, @"^\d") || fieldName.All(c => char.IsDigit(c) || c == 'f' || c == 'F' || c == 'd' || c == 'D'))
                    continue;
                
                // Skip if it looks like a type name (PascalCase AND no underscore AND not a private field)
                // This catches false positives like "BuildingEntity" being detected as a field
                if (char.IsUpper(fieldName[0]) && !fieldName.StartsWith("_") && !IsPrivateField(content, match.Index))
                    continue;

                // Check private fields should start with underscore
                if (IsPrivateField(content, match.Index) && !fieldName.StartsWith("_"))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.FieldName,
                        Severity = ViolationSeverity.Medium,
                        Message = $"Private field '{fieldName}' should start with underscore: '_{fieldName}'",
                        CurrentValue = fieldName,
                        SuggestedValue = $"_{fieldName}",
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
                // Check field naming convention (camelCase after underscore)
                else if (fieldName.StartsWith("_") && fieldName.Length > 1)
                {
                    string fieldNameWithoutUnderscore = fieldName.Substring(1);
                    if (!IsValidCamelCase(fieldNameWithoutUnderscore))
                    {
                        violations.Add(new NamingViolation
                        {
                            FilePath = filePath,
                            LineNumber = GetLineNumber(content, match.Index),
                            Type = ViolationType.FieldName,
                            Severity = ViolationSeverity.Low,
                            Message = $"Field '{fieldName}' should be _camelCase",
                            CurrentValue = fieldName,
                            SuggestedValue = $"_{ToCamelCase(fieldNameWithoutUnderscore)}",
                            CanAutoFix = false // AUDIT ONLY - never auto-fix
                        });
                    }
                }
                else if (!fieldName.StartsWith("_") && !IsValidCamelCase(fieldName))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.FieldName,
                        Severity = ViolationSeverity.Low,
                        Message = $"Field '{fieldName}' should be camelCase",
                        CurrentValue = fieldName,
                        SuggestedValue = ToCamelCase(fieldName),
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
            }
        }

        private void CheckConstants(string filePath, string content)
        {
            // Match const declarations
            var constRegex = new Regex(@"\bconst\s+\w+\s+(\w+)\s*=");
            var matches = constRegex.Matches(content);

            foreach (Match match in matches)
            {
                string constName = match.Groups[1].Value;

                if (!IsValidScreamingSnakeCase(constName))
                {
                    violations.Add(new NamingViolation
                    {
                        FilePath = filePath,
                        LineNumber = GetLineNumber(content, match.Index),
                        Type = ViolationType.ConstantName,
                        Severity = ViolationSeverity.Medium,
                        Message = $"Constant '{constName}' should be SCREAMING_SNAKE_CASE",
                        CurrentValue = constName,
                        SuggestedValue = ToScreamingSnakeCase(constName),
                        CanAutoFix = false // AUDIT ONLY - never auto-fix
                    });
                }
            }
        }

        private void CheckFileName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!IsValidPascalCase(fileName))
            {
                violations.Add(new NamingViolation
                {
                    FilePath = filePath,
                    LineNumber = 1,
                    Type = ViolationType.FileName,
                    Severity = ViolationSeverity.Medium,
                    Message = $"File name '{fileName}.cs' should be PascalCase",
                    CurrentValue = fileName,
                    SuggestedValue = ToPascalCase(fileName),
                    CanAutoFix = false // File rename is complex - AUDIT ONLY
                });
            }
        }

        private void ExportReport()
        {
            string reportPath = EditorUtility.SaveFilePanel(
                "Export Safe Naming Audit Report",
                "",
                $"SafeNamingAudit_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                "txt"
            );

            if (string.IsNullOrEmpty(reportPath)) return;

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("NightBlade SAFE Naming Convention Audit Report");
                writer.WriteLine($"Generated: {DateTime.Now}");
                writer.WriteLine($"Files scanned: {_totalFilesScanned}");
                writer.WriteLine($"Violations found: {violations.Count}");
                writer.WriteLine($"Scan time: {_scanTime:F2} seconds");
                writer.WriteLine();
                writer.WriteLine("⚠️  IMPORTANT: This tool ONLY AUDITS. It never modifies your code!");
                writer.WriteLine();

                var groupedByFile = violations.GroupBy(v => v.FilePath);

                foreach (var fileGroup in groupedByFile.OrderBy(g => g.Key))
                {
                    writer.WriteLine($"📁 {fileGroup.Key}");
                    writer.WriteLine($"   Violations: {fileGroup.Count()}");

                    foreach (var violation in fileGroup.OrderBy(v => v.LineNumber))
                    {
                        writer.WriteLine($"   Line {violation.LineNumber}: [{violation.Type}] {violation.Message}");
                        writer.WriteLine($"     Current: '{violation.CurrentValue}' → Suggested: '{violation.SuggestedValue}'");
                    }

                    writer.WriteLine();
                }
            }

            EditorUtility.RevealInFinder(reportPath);
            Debug.Log($"Safe audit report exported to: {reportPath}");
        }

        private void GoToViolation(NamingViolation violation)
        {
            // Open the file in the default editor
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(violation.FilePath, violation.LineNumber);
        }

        // Helper methods for naming validation
        private bool IsValidPascalCase(string name) => !string.IsNullOrEmpty(name) && char.IsUpper(name[0]);
        private bool IsValidCamelCase(string name) => !string.IsNullOrEmpty(name) && char.IsLower(name[0]);
        private bool IsValidScreamingSnakeCase(string name) => name.All(c => char.IsUpper(c) || char.IsDigit(c) || c == '_');

        private string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToUpper(name[0]) + name.Substring(1);
        }

        private string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            return char.ToLower(name[0]) + name.Substring(1);
        }

        private string ToScreamingSnakeCase(string name)
        {
            // Simple conversion - could be improved
            return name.ToUpper().Replace(" ", "_");
        }

        private int GetLineNumber(string content, int charIndex)
        {
            return content.Substring(0, charIndex).Count(c => c == '\n') + 1;
        }

        private bool IsConstructor(string name, string content, int index)
        {
            // Check if this is a constructor by looking for class name match
            var classRegex = new Regex(@"\bclass\s+(\w+)");
            var classMatch = classRegex.Match(content);
            return classMatch.Success && classMatch.Groups[1].Value == name;
        }

        private bool IsPropertyAccessor(string name) => name == "get" || name == "set";
        private bool IsEventAccessor(string name) => name == "add" || name == "remove";

        private bool IsPrivateField(string content, int index)
        {
            // Look backwards for access modifier
            string before = content.Substring(0, index);
            int lastModifier = Math.Max(
                before.LastIndexOf("private"),
                Math.Max(
                    before.LastIndexOf("protected"),
                    before.LastIndexOf("internal")
                )
            );

            if (lastModifier == -1) return true; // No modifier = private

            string modifier = "";
            if (before.LastIndexOf("private") == lastModifier) modifier = "private";
            else if (before.LastIndexOf("protected") == lastModifier) modifier = "protected";
            else if (before.LastIndexOf("internal") == lastModifier) modifier = "internal";

            return modifier == "private";
        }
    }

    // Data structures
    public enum ViolationType
    {
        ClassName,
        MethodName,
        PropertyName,
        FieldName,
        ConstantName,
        ParameterName,
        LocalVariable,
        FileName
    }

    public enum ViolationSeverity
    {
        Critical,
        High,
        Medium,
        Low
    }

    public class NamingViolation
    {
        public string FilePath;
        public int LineNumber;
        public ViolationType Type;
        public ViolationSeverity Severity;
        public string Message;
        public string CurrentValue;
        public string SuggestedValue;
        public bool CanAutoFix;
    }
}