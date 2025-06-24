using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAFT.Lib.Utils
{
    /// <summary>
    /// Strongly-typed configuration settings for the SAFT library.
    /// </summary>
    public class SAFTConfiguration
    {
        /// <summary>
        /// Path to the default XSD schema file.
        /// </summary>
        public string SchemaPath { get; set; } = "schema1_04.xsd";

        /// <summary>
        /// Path to the output directory for generated files.
        /// </summary>
        public string OutputDirectory { get; set; } = "./output";

        /// <summary>
        /// Whether to enable strict validation (fail on first error).
        /// </summary>
        public bool StrictValidation { get; set; } = true;

        /// <summary>
        /// Default country code (e.g., "PT").
        /// </summary>
        public string DefaultCountryCode { get; set; } = "PT";

        /// <summary>
        /// Default currency code (e.g., "EUR").
        /// </summary>
        public string DefaultCurrencyCode { get; set; } = "EUR";

        /// <summary>
        /// Default fiscal year (0 = auto-detect).
        /// </summary>
        public int DefaultFiscalYear { get; set; } = 0;

        /// <summary>
        /// Additional custom settings (for extensibility).
        /// </summary>
        public JsonElement? CustomSettings { get; set; }
    }

    /// <summary>
    /// Static manager for loading, saving, and accessing SAFT configuration.
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly string DefaultConfigPath = "saftconfig.json";
        private static SAFTConfiguration? _config;

        /// <summary>
        /// Gets the current configuration (loads defaults if not loaded).
        /// </summary>
        public static SAFTConfiguration Current
        {
            get
            {
                if (_config == null)
                    Load();
                return _config!;
            }
        }

        /// <summary>
        /// Loads configuration from a file or uses defaults if not found.
        /// </summary>
        /// <param name="path">Path to config file (optional)</param>
        public static void Load(string? path = null)
        {
            path ??= DefaultConfigPath;
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                _config = JsonSerializer.Deserialize<SAFTConfiguration>(json) ?? new SAFTConfiguration();
            }
            else
            {
                _config = new SAFTConfiguration();
            }
        }

        /// <summary>
        /// Saves the current configuration to a file.
        /// </summary>
        /// <param name="path">Path to config file (optional)</param>
        public static void Save(string? path = null)
        {
            path ??= DefaultConfigPath;
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Override the current configuration in code.
        /// </summary>
        /// <param name="config">New configuration object</param>
        public static void Set(SAFTConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
    }
} 