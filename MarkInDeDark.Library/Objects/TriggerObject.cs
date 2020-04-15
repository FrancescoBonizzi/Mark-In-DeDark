using MarkInDeDark.Library.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDark.Library.Objects
{
    public class TriggerObject
    {
        protected Rectangle _triggerArea;

        private readonly List<Character> _affectedCharacters;
        private Action<Character, TimeSpan> _onTriggerPressed;
        private Action _onTriggerLeft;
        private readonly bool _shouldFireOnce;
        protected bool _hasFired = false;

        public TriggerObject(
            Rectangle triggerCollisionRectangle,
            IEnumerable<Character> affectedCharacters,
            Action<Character, TimeSpan> onTriggerPressed,
            Action onTriggerLeft,
            bool shouldFireOnce)
        {
            _triggerArea = triggerCollisionRectangle;
            _affectedCharacters = new List<Character>(affectedCharacters);
            _onTriggerPressed = onTriggerPressed;
            _shouldFireOnce = shouldFireOnce;
            _onTriggerLeft = onTriggerLeft;
        }

        public virtual void AddAffectedCharacter(Character character)
        {
            _affectedCharacters.Add(character);
        }

        protected void SetOnTriggerPressed(Action<Character, TimeSpan> onTriggerPressed)
            => _onTriggerPressed = onTriggerPressed;

        protected void SetOnTriggerLeft(Action onTriggerLeft)
            => _onTriggerLeft = onTriggerLeft;

        public virtual void Update(TimeSpan elapsed)
        {
            bool someOneOnTheTrigger = false;
            if ((_shouldFireOnce && !_hasFired)
                || !_shouldFireOnce)
            {

                for (int i = _affectedCharacters.Count() - 1; i >= 0; --i)
                {
                    if (_affectedCharacters[i].CollisionRectangle.Intersects(_triggerArea))
                    {
                        someOneOnTheTrigger = true;
                        _onTriggerPressed(_affectedCharacters[i], elapsed);
                        _hasFired = true;

                        if (_affectedCharacters[i].IsDead || _affectedCharacters[i].IsDying)
                            _affectedCharacters.RemoveAt(i);
                    }

                }
            }

            if (_hasFired && !someOneOnTheTrigger)
            {
                _onTriggerLeft?.Invoke();
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
