using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DirectOutput.Cab.Out;
using DirectOutput.General;
using System.Xml.Serialization;
using DirectOutput.General.Analog;
using DirectOutput.General.Color;

namespace DirectOutput.Cab.Toys.Layer
{
    /// <summary>
    /// This toy handles analog values (0-255) in a layer structure including alpha value (0=completely transparent, 255=fully opaque) and outputs the belended result of the layers on a single output on a led strip.
    /// </summary>
    public class AnalogAlphaToyOnLedStrip : AnalogAlphaToy, IAnalogAlphaToy, ISingleOutputToy
    {
        private RGBOrderEnum _ColorOrder = RGBOrderEnum.RBG;

        /// <summary>
        /// Gets or sets the order of the colors for the leds of the led strip.
        /// Usually colors are represented in RGB (Red - Green - Blue) order, but depending on the type of the used strip the color order might be different (e.g. WS2812 led chips have green - red - blue as their color order).
        /// </summary>
        /// <value>
        /// The color order of the leds on the strip.
        /// </value>
        public RGBOrderEnum ColorOrder
        {
            get { return _ColorOrder; }
            set { _ColorOrder = value; }
        }

        private RGBColor _Color = new RGBColor("#ffffff");
        /// <summary>
        /// Gets or sets the color to be used by the toy on the ledstrip
        /// </summary>
        /// <value>
        /// The color of the led on the strip.
        /// </value>
        public string Color
        {
            get { return _Color.HexColor; }
            set { _Color.HexColor = value; }
        }

        private int _LedNumber = 1;
        /// <summary>
        /// Gets or sets the number of the first led of the strip.
        /// </summary>
        /// <value>
        /// The number of the first led of the strip.
        /// </value>
        public int LedNumber
        {
            get { return _LedNumber; }
            set { _LedNumber = value.Limit(1, int.MaxValue); }
        }


        #region Outputs
        /// <summary>
        /// Gets or sets the name of the output controller to be used.
        /// </summary>
        /// <value>
        /// The name of the output controller.
        /// </value>
        public string OutputControllerName { get; set; }
        private ISupportsSetValues OutputController;

        #endregion
        
        protected override void InitOutputs(Cabinet Cabinet)
        {
            if (Cabinet.OutputControllers.Contains(OutputControllerName) && Cabinet.OutputControllers[OutputControllerName] is ISupportsSetValues)
            {
                OutputController = (ISupportsSetValues)Cabinet.OutputControllers[OutputControllerName];
            }
        }

        //Array for output data is not in GetResultingValues to avoid reinitiaslisation of the array
        byte[] OutputData = new byte[3];

        /// <summary>
        /// Updates the output of the toy.
        /// </summary>
        public override void UpdateOutputs()
        {
            if (OutputController != null && Layers.Count > 0)
            {
                int value = GetResultingValue();
                float ratio = value / 255.0f;
                RGBColor RGB = new RGBColor((int)((_Color.Red * ratio)+0.5f), 
                                            (int)((_Color.Blue * ratio)+0.5f), 
                                            (int)((_Color.Green * ratio)+0.5f));

                int OutputNumber = 0;
                switch (ColorOrder)
                {
                    case RGBOrderEnum.RBG:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Red);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Green);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Blue);
                        break;
                    case RGBOrderEnum.GRB:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Green);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Red);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Blue);
                        break;
                    case RGBOrderEnum.GBR:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Green);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Blue);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Red);
                        break;
                    case RGBOrderEnum.BRG:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Blue);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Red);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Green);
                        break;
                    case RGBOrderEnum.BGR:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Blue);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Green);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Red);
                        break;
                    case RGBOrderEnum.RGB:
                    default:
                        OutputData[OutputNumber] = FadingCurve.MapValue(RGB.Red);
                        OutputData[OutputNumber + 1] = FadingCurve.MapValue(RGB.Green);
                        OutputData[OutputNumber + 2] = FadingCurve.MapValue(RGB.Blue);
                        break;
                }

                OutputController.SetValues((LedNumber - 1) * 3, OutputData);
            };                      
        }


        /// <summary>
        /// Resets the toy and releases all references
        /// </summary>
        public override void Finish()
        {            
            base.Finish();
            OutputController = null;   
        }

        /// <summary>
        /// Resets the toy.<br/>
        /// Clears the Layers object and turn off the output (if available).
        /// Method must be overwritten.
        /// </summary>
        public override void Reset()
        {
            Layers.Clear();
            if (OutputController != null)
            {
                OutputController.SetValues((LedNumber - 1) * 3, new byte[3]);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogAlphaToy"/> class.
        /// </summary>
        public AnalogAlphaToyOnLedStrip()
        {            
        }
    }
}
