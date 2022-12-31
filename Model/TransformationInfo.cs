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
        public TransformationInfo(
            int _buffId,
            string _buffKeyName,
            bool _stackable,
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
            stackable = _stackable;
        }
    }
}