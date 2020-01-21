using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DirectOutput.Cab.Out;
using DirectOutput.General.Color;
using DirectOutput.General;

namespace DirectOutput.Cab.Toys.Layer
{
    /// <summary>
    /// Thie RGBAToy controls RGB leds and other gadgets displaying RGB colors.<br/><br/>
    /// The RGBAToy has multilayer support with alpha channels. This allows the effects targeting RGBAToys to send their data to different layers. 
    /// Values in a layer do also have a alpha/transparency channel which will allow us to blend the colors/values in the various layers (e.g. if  a bottom layer is blue and top is a semi transparent red, you will get some mix of both or if one of the two blinks you get changing colors).<br/>
    /// The following picture might give you a clearer idea how the layers with their alpha channels work:
    /// 
    /// \image html LayersRGBA.png "RGBA Layers"
    /// </summary>
    public class RGBAToyOnLedStrip : RGBAToy, IRGBOutputToy, IRGBAToy
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

        #region Init
        /// <summary>
        /// Initializes the RGBALed On LedStrip toy.
        /// </summary>
        /// <param name="Cabinet"><see cref="Cabinet"/> object to which the <see cref="RGBAToy"/> belongs.</param>
        protected virtual void InitOutputs(Cabinet Cabinet)
        {
            if (Cabinet.OutputControllers.Contains(OutputControllerName) && Cabinet.OutputControllers[OutputControllerName] is ISupportsSetValues)
            {
                OutputController = (ISupportsSetValues)Cabinet.OutputControllers[OutputControllerName];
            }
        }

        #endregion

        #region Finish

        /// <summary>
        /// Finishes the RGBALed toy.<br/>
        /// Resets the the toy and releases all references.
        /// </summary>
        public override void Finish()
        {
            base.Finish();
            OutputController = null;
        }
        #endregion

        //Array for output data is not in GetResultingValues to avoid reinitiaslisation of the array
        byte[] OutputData = new byte[3];

        /// <summary>
        /// Updates the outputs of the RGBAToy.
        /// </summary>
        public override void UpdateOutputs()
        {
            if (OutputController != null && Layers.Count > 0)
            {
                RGBColor RGB = GetResultingData();

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
        /// Clears all layers and sets all outputs to 0 (off).
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
        /// Initializes a new instance of the <see cref="RGBAToy"/> class.
        /// </summary>
        public RGBAToyOnLedStrip()
        {
        }
    }
}
