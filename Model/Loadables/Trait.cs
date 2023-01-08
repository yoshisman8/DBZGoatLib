using DBZGoatLib.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace DBZGoatLib.Model
{
    public abstract class Trait : IDBZLoadable
    {
        /// <summary>
        /// This trait's name.
        /// </summary>
        public abstract string Name();

        /// <summary>
        /// This trait's weight when rolling character creation. Traitelss has a weight of 1f while Prodigy has a weight of 0.15f.
        /// </summary>
        public abstract float Weight();
        /// <summary>
        /// The color the Ki Bar should change to when having this trait. Return null if you don't wish this trait to change the ki bar's color.
        /// </summary>
        public abstract Gradient KiBarGradient();

        /// <summary>
        /// Delegate that is executed once when the user gains this trait and once each time the player loads into the world.
        /// </summary>
        /// <param name="player">Player with this trait.</param>
        public abstract void OnTrait(Player player);

        /// <summary>
        /// Delegate that is executed once when the user loses this trait (Such as when re-rolling traits).
        /// </summary>
        /// <param name="player">Player who just lost this trait.</param>
        public abstract void OnLoseTrait(Player player);


        public TraitInfo Info => new TraitInfo(Name(), Weight(), KiBarGradient(), OnTrait, OnLoseTrait);
        public void Load(Mod mod)
        {
            TraitHandler.RegisterTrait(Info);
        }

        public void Unload()
        {
            TraitHandler.UnregisterTrait(Info);
        }
    }
}
