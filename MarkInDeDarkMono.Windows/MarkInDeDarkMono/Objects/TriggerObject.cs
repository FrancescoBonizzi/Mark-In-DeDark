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
        private Action<Character, TimeSpan> _onTrigger;
        private readonly bool _shouldFireOnce;
        private bool _hasFired = false;

        public TriggerObject(
            Rectangle triggerCollisionRectangle,
            IEnumerable<Character> affectedCharacters,
            Action<Character, TimeSpan> onTrigger,
            bool shouldFireOnce)
        {
            _triggerArea = triggerCollisionRectangle;
            _affectedCharacters = new List<Character>(affectedCharacters);
            _onTrigger = onTrigger;
            _shouldFireOnce = shouldFireOnce;
        }

        protected void SetOnTriggerFunction(Action<Character, TimeSpan> onTrigger)
            => _onTrigger = onTrigger;

        public virtual void Update(TimeSpan elapsed)
        {
            if ((_shouldFireOnce && !_hasFired)
                || !_shouldFireOnce)
            {
                for (int i = _affectedCharacters.Count() - 1; i >= 0; --i)
                {
                    if (_affectedCharacters[i].CollisionRectangle.Intersects(_triggerArea))
                    {
                        _onTrigger(_affectedCharacters[i], elapsed);
                        _hasFired = true;

                        if (_affectedCharacters[i].IsDead || _affectedCharacters[i].IsDying)
                            _affectedCharacters.RemoveAt(i);
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
