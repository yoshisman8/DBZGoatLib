using DBZGoatLib.Model;
using DBZGoatLib.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Utilities;

namespace DBZGoatLib.Handlers
{
    public static class TraitHandler
    {
        /// <summary>
        /// List of all registered traits. Can be added to.
        /// </summary>
        public static List<TraitInfo> Traits { get; private set; } = new();

        /// <summary>
        /// Registers a new trait to the handler.
        /// </summary>
        /// <param name="trait">TraitInfo object.</param>
        public static void RegisterTrait(TraitInfo trait) => Traits.Add(trait);

        /// <summary>
        /// Unregisters a trait from the handler.
        /// </summary>
        /// <param name="trait">Trait to remove.</param>
        public static void UnregisterTrait(TraitInfo trait)
        {
            int index = Traits.FindIndex(x => x.Name == trait.Name);
            if (index > -1)
                Traits.RemoveAt(index);
        }

        public static TraitInfo? GetTraitByName(string name)
        {
            var fetch = Traits.Find(x => x.Name == name);
            if (fetch.Equals(new TraitInfo()))
                return null;
            return fetch;
        }

        /// <summary>
        /// Rolls a trait at random.
        /// </summary>
        /// <param name="IncludeTraitless">Whether to include Traitless into the calcuations.</param>
        /// <param name="previous">Name of the existing trait to exclude from the RNG. Leave blank to not exclude any trait.</param>
        /// <returns></returns>
        public static TraitInfo RollTrait(bool IncludeTraitless = true, string previous = null)
        {
            var Weighted = new WeightedRandom<TraitInfo>();

            foreach (var trait in Traits.Where(x => x.Name != previous))
                Weighted.Add(trait, trait.Weight);

            if (IncludeTraitless)
                Weighted.Add(new TraitInfo("Traitless", 1f, null, Traitless, Traitless), 1f);

            TraitInfo rolledTrait = Weighted.Get();

            return rolledTrait;
        }
        private static void Traitless(Player player, TraitInfo trait) { KiBar.ResetColor(); player.GetModPlayer<GPlayer>().ClearDBTTrait(); }
    }
}
