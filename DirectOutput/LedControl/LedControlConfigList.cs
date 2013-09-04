﻿using System.Collections.Generic;
using System.Linq;
using DirectOutput.Cab;
using DirectOutput.Cab.Toys;
using DirectOutput.FX.LedControlFX;
using DirectOutput.GlobalConfiguration;
using DirectOutput.Table;
using DirectOutput.Cab.Toys.LWEquivalent;
namespace DirectOutput.LedControl
{

    /// <summary>
    /// List of LedControlConfig objects loaded from LedControl.ini files.
    /// </summary>
    public class LedControlConfigList : List<LedControlConfig>
    {
        
        /// <summary>
        /// Updates the supplied table object with a configurations from ledcontrol files.
        /// </summary>
        /// <param name="Table">The table object to be configured.</param>
        /// <param name="RomName">Name of the rom.</param>
        /// <param name="Cabinet">The cabinet which will receive the tables outputs.</param>
        public void UpdateTableConfig(DirectOutput.Table.Table Table, string RomName, Cabinet Cabinet)
        {

            List<IToy> LedWizEqivalentList = Cabinet.Toys.Where(Toy => Toy is LedWizEquivalent).ToList();
            Dictionary<int, TableConfig> TableConfigDict = GetTableConfigDictonary(RomName);

            if (TableConfigDict.Count > 0)
            {
                
                if (Table.ConfigurationSource == DirectOutput.Table.TableConfigSourceEnum.TableConfigurationFile)
                {
                    Table.ConfigurationSource = DirectOutput.Table.TableConfigSourceEnum.TabbleConfigurationFileAndLedControl;
                }
                else
                {
                    Table.ConfigurationSource = DirectOutput.Table.TableConfigSourceEnum.LedControlIni;
                }
                
                foreach (KeyValuePair<int, TableConfig> KV in TableConfigDict)
                {

                    foreach (IToy Toy in LedWizEqivalentList)
                    {
                        LedWizEquivalent LWE = (LedWizEquivalent)Toy;
                        if (LWE.LedWizNumber == KV.Key)
                        {


                            foreach (TableConfigColumn C in KV.Value.Columns)
                            {
                                foreach (TableConfigSetting S in C)
                                {
                                    bool SetupProblem = false;
                                    LedControlEffect LCE = new LedControlEffect(LWE.Name, C.FirstOutputNumber);

                                    string FXName = "LedControl {0}, Output {1}, {2}, ".Build(new object[] { LWE.LedWizNumber, C.FirstOutputNumber, (S.OutputControl == OutputControlEnum.Controlled ? "Controlled {0}{1}".Build(S.TableElementType, S.TableElementNumber) : "Static") });
                                    if (S.OutputType == OutputTypeEnum.AnalogOutput)
                                    {
                                        FXName += "Intensity {0}, ".Build(S.Intensity);
                                        LCE.Intensity = S.Intensity;
                                    }
                                    else
                                    {
                                        if (S.ColorConfig != null)
                                        {
                                            LCE.RGBColor = new int[3] { S.ColorConfig.Red, S.ColorConfig.Green, S.ColorConfig.Blue };
                                        }
                                        else
                                        {
                                            SetupProblem = true;
                                        }
                                        FXName += "Color {0}, ".Build(S.ColorName);
                                    }
                                    if (S.DimmingUpDurationMs > 0)
                                    {
                                        LCE.DimUpDurationMs = S.DimmingUpDurationMs;
                                        FXName += "DimUpDuration {0}, ".Build(S.DimmingUpDurationMs);
                                    }
                                    if (S.DimmingDownDurationMs > 0)
                                    {
                                        LCE.DimDownDurationMs = S.DimmingDownDurationMs;
                                        FXName += "DimDownDuration {0}, ".Build(S.DimmingDownDurationMs);
                                    }
                                    if (S.Blink > 0)
                                    {
                                        LCE.Blink = S.Blink;
                                        FXName += "Blink {0}, ".Build(S.Blink);
                                    }
                                    else if (S.Blink < 0)
                                    {
                                        LCE.Blink = -1;
                                        FXName += "Blink, ";
                                    }
                                    if (S.Blink != 0 && S.BlinkIntervalMs > 0)
                                    {
                                        LCE.BlinkInterval = S.BlinkIntervalMs;
                                        FXName += "BlinkInterval {0},".Build(S.BlinkIntervalMs);
                                    }
                                    if (S.DurationMs > 0)
                                    {
                                        LCE.Duration = S.DurationMs;
                                        FXName += "Duration {0}, ".Build(S.DurationMs);
                                    }
                                    if (FXName.Right(2) == ", ")
                                    {
                                        FXName = FXName.Left(FXName.Length - 2);
                                    }
                                    LCE.Name = FXName;


                                    if (!SetupProblem)
                                    {


                                        if (!Table.Effects.Contains(LCE.Name))
                                        {
                                            Table.Effects.Add(LCE);
                                        }
                                        else
                                        {
                                            if (Table.Effects[LCE.Name] is LedControlEffect)
                                            {
                                                LCE = (LedControlEffect)Table.Effects[LCE.Name];
                                            }
                                            else
                                            {
                                                LCE = null;
                                            }
                                        }
                                        if (LCE != null)
                                        {
                                            if (S.OutputControl == OutputControlEnum.Controlled)
                                            {
                                                //Add tableelement to table if necessary
                                                if (!Table.TableElements.Contains(S.TableElementType, S.TableElementNumber))
                                                {
                                                    Table.TableElements.Add(S.TableElementType, S.TableElementNumber, -1);
                                                }
                                                TableElement TE = Table.TableElements[S.TableElementType, S.TableElementNumber];
                                                TE.AssignedEffects.Add(new FX.AssignedEffectOrder(LCE.Name));
                                            }
                                            else if (S.OutputControl == OutputControlEnum.FixedOn)
                                            {
                                                Table.AssignedStaticEffects.Add(new FX.AssignedEffectOrder(LCE.Name));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }


        private Dictionary<int, TableConfig> GetTableConfigDictonary(string RomName)
        {
            Dictionary<int, TableConfig> D = new Dictionary<int, TableConfig>();

            bool FoundMatch = false;
            foreach (LedControlConfig LCC in this)
            {

                foreach (TableConfig TC in LCC.TableConfigurations)
                {
                    if (RomName.ToUpper() == TC.ShortRomName.ToUpper())
                    {
                        D.Add(LCC.LedWizNumber, TC);
                        FoundMatch = true;
                        break;
                    }
                }
            }

            if (FoundMatch) return D;

            foreach (LedControlConfig LCC in this)
            {

                foreach (TableConfig TC in LCC.TableConfigurations)
                {
                    if (RomName.ToUpper().StartsWith("{0}_".Build(TC.ShortRomName.ToUpper())))
                    {
                        D.Add(LCC.LedWizNumber, TC);
                        FoundMatch = true;
                        break;
                    }
                }
            }

            if (FoundMatch) return D;

            foreach (LedControlConfig LCC in this)
            {

                foreach (TableConfig TC in LCC.TableConfigurations)
                {
                    if (RomName.StartsWith(TC.ShortRomName))
                    {
                        D.Add(LCC.LedWizNumber, TC);

                        break;
                    }
                }
            }
            return D;
        }


        /// <summary>
        /// Determines whether a config for the spcified RomName exists in the configs.
        /// </summary>
        /// <param name="RomName">Name of the rom.</param>
        /// <returns>
        ///   <c>true</c> if the specified config exists; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsConfig(string RomName)
        {
            return GetTableConfigDictonary(RomName).Count > 0;

        }


        /// <summary>
        /// Loads a list of ledcontrol.ini files.
        /// </summary>
        /// <param name="LedControlFilenames">The list of ledcontrol.ini files</param>
        /// <param name="ThrowExceptions">if set to <c>true</c> throw exceptions on errors.</param>
        public void LoadLedControlFiles(IList<string> LedControlFilenames, bool ThrowExceptions = false)
        {
            for (int i = 0; i < LedControlFilenames.Count; i++)
            {
                LoadLedControlFile(LedControlFilenames[i], i + 1, ThrowExceptions);
            }
        }

        /// <summary>
        /// Loads a list of ledcontrol.ini files.
        /// </summary>
        /// <param name="LedControlIniFiles">The list of ini files to be loaded.</param>
        /// <param name="ThrowExceptions">if set to <c>true</c> throw exceptions on errors.</param>
        public void LoadLedControlFiles(LedControlIniFileList LedControlIniFiles, bool ThrowExceptions = false)
        {
            foreach (LedControlIniFile F in LedControlIniFiles)
            {
                LoadLedControlFile(F.Filename, F.LedWizNumber, ThrowExceptions);
            }
        }


        /// <summary>
        /// Loads a single ledcontrol.ini file.
        /// </summary>
        /// <param name="LedControlFilename">The ledcontrol.ini filename.</param>
        /// <param name="LedWizNumber">The number of the LedWizEquivalent to be used for the output of the configuration in the file.</param>
        /// <param name="ThrowExceptions">if set to <c>true</c> throws exceptions on errors.</param>
        public void LoadLedControlFile(string LedControlFilename, int LedWizNumber, bool ThrowExceptions = false)
        {
            Log.Write("Loading LedControl file {0}".Build(LedControlFilename));

            LedControlConfig LCC = new LedControlConfig(LedControlFilename, LedWizNumber, ThrowExceptions);
            Add(LCC);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LedControlConfigList"/> class.
        /// </summary>
        public LedControlConfigList() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LedControlConfigList"/> class.
        /// </summary>
        /// <param name="LedControlFilenames">The filenames of the ledcontrol.ini files to be loaded.</param>
        /// <param name="ThrowExceptions">if set to <c>true</c> exceptions on loading the files are shown.</param>
        public LedControlConfigList(IList<string> LedControlFilenames, bool ThrowExceptions = false)
            : this()
        {
            LoadLedControlFiles(LedControlFilenames, ThrowExceptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LedControlConfigList"/> class.
        /// </summary>
        /// <param name="LedControlIniFiles">The list of ini files to be loaded.</param>
        /// <param name="ThrowExceptions">if set to <c>true</c> exceptions on loading the files are shown.</param>
        public LedControlConfigList(LedControlIniFileList LedControlIniFiles, bool ThrowExceptions = false)
            : this()
        {
            LoadLedControlFiles(LedControlIniFiles, ThrowExceptions);
        }

    }
}
