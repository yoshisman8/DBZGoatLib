using System;

using Microsoft.Xna.Framework;

using Terraria;

namespace DBZGoatLib.Model {
    public readonly record struct TransformationInfo {
        public readonly string buffKeyName;
        public readonly int buffID;
        public readonly bool stackable;
        public readonly string transformationText;
        public readonly Color tranformtionColor;
        public readonly Predicate<Player> condition;
        public readonly Action<Player> onTransform;
        public readonly Action<Player> postTransform;
        public readonly AnimationData animationData;
        public readonly Gradient KiBarGradient;

        /// <summary>
        /// Create a new Transformation Info record.
        /// </summary>
        /// <param name="_buffId">The Buff ID of the transformation.</param>
        /// <param name="_buffKeyName">The name of the Buff Class.</param>
        /// <param name="_stackable">Whether this form is stackable with other forms or not.</param>
        /// <param name="_transformationText">Text to be displayed when transforming.</param>
        /// <param name="_TransformationColor">Color to use for the Transformation Text.</param>
        /// <param name="_condition">Function which takes in a Player record and returns true or false to determine whether the user can entre that transformation.</param>
        /// <param name="_onTransform">Function which which happens after the user starts this transformation. Use this to trigger flags on the player.</param>
        /// <param name="_postTransform">Function which which happens after the user ends this transformation. Use this to trigger flags on the player.</param>
        /// <param name="_animationData">Transformation's animation data. If this transformation does not have an aura, change in hair nor audo effects, pass a new/empty AnimationData.</param>
        /// <param name="_kiBar">Whether this transformation changes the Ki Bar's color. Leave null to not change the Ki Bar.</param>
        public TransformationInfo(
            int _buffId,
            string _buffKeyName,
            bool _stackable,
            string _transformationText,
            Color _TransformationColor,
            Predicate<Player> _condition,
            Action<Player> _onTransform,
            Action<Player> _postTransform,
            AnimationData _animationData,
            Gradient _kiBar = null) {
            buffID = _buffId;
            transformationText = _transformationText;
            tranformtionColor = _TransformationColor;
            buffKeyName = _buffKeyName;
            condition = _condition;
            onTransform = _onTransform;
            postTransform = _postTransform;
            animationData = _animationData;
            stackable = _stackable;
            KiBarGradient = _kiBar;
        }

        [Obsolete("Please use TransformationInfo(int, string, bool, string, Color, Predicate<Player>, Action<Player>, Action<Player>, AnimationData, Gradient")]
        public TransformationInfo(
            int _buffId, 
            string _buffKeyName,
            string _transformationText, 
            Color _TransformationColor, 
            Predicate<Player> _condition,
            Action<Player> _onTransform, 
            Action<Player> _postTransform, 
            AnimationData _animationData) {
            buffID = _buffId;
            transformationText = _transformationText;
            tranformtionColor = _TransformationColor;
            buffKeyName = _buffKeyName;
            condition = _condition;
            onTransform = _onTransform;
            postTransform = _postTransform;
            animationData = _animationData;
            stackable = false;
            KiBarGradient = null;
        }
    }

    public readonly record struct TransformationChain
    {
        public readonly string TransformationBuffKeyName;
        public readonly string NextTransformationBuffKeyName;
        public readonly string PreviousTransformationBuffKeyName;
        public readonly bool Charging;

        /// <summary>
        /// Create a new Transformation Chain which links a transformation with another one for the sake of stepping up/down forms.
        /// </summary>
        /// <param name="buffKeyName">The BuffKeyName of the transformation.</param>
        /// <param name="charging">Whether the following step up/downs have to be accessed by holding the Charge keybind + Transform keybind (True) or whether just pressing the Transform Keybind alone (false) is enough.</param>
        /// <param name="nextStep">The BuffKeyName of the transformation above the one defined by BuffKeyName.</param>
        /// <param name="previousStep">The BuffKeyName of the transformation below the one defiend by BuffKeyName.</param>
        public TransformationChain(string buffKeyName, bool charging, string nextStep = null, string previousStep = null)
        {
            TransformationBuffKeyName = buffKeyName;
            NextTransformationBuffKeyName = nextStep;
            PreviousTransformationBuffKeyName = previousStep;
            Charging = charging;
        }
    }
}