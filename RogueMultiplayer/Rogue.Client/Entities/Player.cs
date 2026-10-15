using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rogue.Client.Entities
{

    public class Player
    {
        public float X { get; set; } = 3;
        public float Y { get; set; } = 3;

        public float Speed { get; set; } = 4f;

        public float Health { get; set; } = 1000f;
        public float MaxHealth { get; set; } = 100f;

        public float FacingDx { get; set; } = 1f;
        public float FacingDy { get; set; } = 0f;

        public float AttackAnimTimer { get; set; } = 0f;
    }
}
