#define AvoidInternal


using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.PackageInternal
{
    #if AvoidInternal

    public class IntegerField : TextInputBaseField<int>
    {
        public bool isUnsigned { get; set; }
        
        public IntegerField() : this(null, -1, char.MinValue, null)
        {
        }

        protected IntegerField(string label, int maxLength, char maskChar, TextInputBase textInputBase) : base(label, maxLength, maskChar, textInputBase)
        {
        }
    }
    
    #else
    
    /// <summary>
    /// Makes a text field for entering an integer.
    /// </summary>
    public class IntegerField : TextValueField<int>
    {
        public bool isUnsigned { get; set; }

        // This property to alleviate the fact we have to cast all the time
        IntegerInput integerInput => (IntegerInput)textInputBase;

        /// <summary>
        /// Instantiates an <see cref="IntegerField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<IntegerField, UxmlTraits> { }
        /// <summary>
        /// Defines <see cref="UxmlTraits"/> for the <see cref="IntegerField"/>.
        /// </summary>
        public new class UxmlTraits : TextValueFieldTraits<int, UxmlIntAttributeDescription> { }

        /// <summary>
        /// Converts the given integer to a string.
        /// </summary>
        /// <param name="v">The integer to be converted to string.</param>
        /// <returns>The integer as string.</returns>
        protected override string ValueToString(int v)
        {
            return v.ToString(formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <summary>
        /// Converts a string to an integer.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>The integer parsed from the string.</returns>
        protected override int StringToValue(string str)
        {
            if (!int.TryParse(str, out var v))
            {
                v = rawValue;
            }
            return v;
        }

        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public new static readonly string ussClassName = "unity-integer-field";
        /// <summary>
        /// USS class name of labels in elements of this type.
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";
        /// <summary>
        /// USS class name of input elements in elements of this type.
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// Constructor.
        /// </summary>
        public IntegerField()
            : this((string)null) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        public IntegerField(int maxLength)
            : this(null, maxLength) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="maxLength">Maximum number of characters the field can take.</param>
        
        // TODO: internal
        public IntegerField(string label, int maxLength = /*kMaxLengthNone*/-1)
            : base(label, maxLength, new IntegerInput())
        {
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
            // TODO: internal
            //visualInput.AddToClassList(inputUssClassName);
            AddLabelDragger<int>();
        }

        internal override bool CanTryParse(string textString) => int.TryParse(textString, out _);

        /// <summary>
        /// Modify the value using a 3D delta and a speed, typically coming from an input device.
        /// </summary>
        /// <param name="delta">A vector used to compute the value change.</param>
        /// <param name="speed">A multiplier for the value change.</param>
        /// <param name="startValue">The start value.</param>
        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
        {
            integerInput.ApplyInputDeviceDelta(delta, speed, startValue);
        }

        class IntegerInput : TextValueInput
        {
            IntegerField parentIntegerField => (IntegerField)parent;
            bool isUnsigned => parentIntegerField.isUnsigned;

            internal IntegerInput()
            {
                formatString = "#######0";
            }

            protected override string allowedCharacters => isUnsigned ? "0123456789" : "0123456789-";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, int startValue)
            {
                double sensitivity = NumericFieldDraggerUtility.CalculateIntDragSensitivity(startValue);
                float acceleration = NumericFieldDraggerUtility.Acceleration(speed == DeltaSpeed.Fast, speed == DeltaSpeed.Slow);
                long v = StringToValue(text);
                v += (long)Math.Round(NumericFieldDraggerUtility.NiceDelta(delta, acceleration) * sensitivity);

                var intValue = MathUtils.ClampToInt(v);
                if (isUnsigned) intValue = Mathf.Max(0, intValue);

                if (parentIntegerField.isDelayed)
                {
                    text = ValueToString(intValue);
                }
                else
                {
                    parentIntegerField.value = intValue;
                }
            }

            protected override string ValueToString(int v)
            {
                return v.ToString(formatString);
            }

            protected override int StringToValue(string str) => parentIntegerField.StringToValue(str);
        }
    }
    #endif
}
