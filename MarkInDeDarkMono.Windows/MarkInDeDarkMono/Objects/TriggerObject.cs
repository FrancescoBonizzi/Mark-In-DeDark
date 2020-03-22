using MarkInDeDarkMono.Characters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MarkInDeDarkMono.Objects
{
    public class TriggerObject
    {
        private Rectangle _triggerArea;

        private readonly List<Character> _affectedCharacters;
        private Action<Character, TimeSpan> _onTriggerPressed;
        private Action<Character, TimeSpan> _onTriggerLeft;
        private readonly bool _shouldFireOnce;
        private bool _hasFired = false;

        public TriggerObject(
            Rectangle triggerCollisionRectangle,
            IEnumerable<Character> affectedCharacters,
            Action<Character, TimeSpan> onTriggerPressed,
            Action<Character, TimeSpan> onTriggerLeft,
            bool shouldFireOnce)
        {
            _triggerArea = triggerCollisionRectangle;
            _affectedCharacters = new List<Character>(affectedCharacters);
            _onTriggerPressed = onTriggerPressed;
            _onTriggerLeft = onTriggerLeft;
            _shouldFireOnce = shouldFireOnce;
        }

        public void AddAffectedCharacter(Character character)
        {
            _affectedCharacters.Add(character);
        }

        protected void SetOnTriggerPressed(Action<Character, TimeSpan> onTriggerPressed)
            => _onTriggerPressed = onTriggerPressed;

        protected void SetOnTriggerLeft(Action<Character, TimeSpan> onTriggerLeft)
            => _onTriggerLeft = onTriggerLeft;

        public virtual void Update(TimeSpan elapsed)
        {
            if ((_shouldFireOnce && !_hasFired)
                || !_shouldFireOnce)
            {
                for (int i = _affectedCharacters.Count() - 1; i >= 0; --i)
                {
                    if (_affectedCharacters[i].CollisionRectangle.Intersects(_triggerArea))
                    {
                        _onTriggerPressed(_affectedCharacters[i], elapsed);
                        _hasFired = true;

                        if (_affectedCharacters[i].IsDead || _affectedCharacters[i].IsDying)
                            _affectedCharacters.RemoveAt(i);
                    }
                    else
                    {
                        if (_hasFired)
                        {
                            _onTriggerLeft(_affectedCharacters[i], elapsed);
                            _hasFired = false;
                        }
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
