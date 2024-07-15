using System.Collections.Generic;
using UnityEngine;
using XcelerateGames;
using XcelerateGames.AssetLoading;

namespace JungleeGames
{
    public class StartupJunglee : Startup
    {
        [SerializeField] protected string[] _Variants = null;

        #region Private Methods
        protected override void Awake()
        {
            ResourceManager.ClearVariants();
            foreach (string variant in _Variants)
            {
                ResourceManager.AddVariant(variant);
            }

            base.Awake();
        }

        protected override List<string> GetCompilerFlags()
        {
            List<string> flags = base.GetCompilerFlags();
#if ENABLE_DEBUG_LOGS
            flags.Add("ENABLE_DEBUG_LOGS");
#endif
#if AUTOMATION_ENABLED
            flags.Add("AUTOMATION_ENABLED");
#endif
#if USING_FLUTTER
            flags.Add("USING_FLUTTER");
#endif
#if UMBRELLA
            flags.Add("UMBRELLA");
#endif
#if POKER_STANDALONE
            flags.Add("POKER_STANDALONE");
#endif
#if SOLITAIRE_GOLD_STANDALONE
            flags.Add("SOLITAIRE_GOLD_STANDALONE");
#endif
#if CARROM_STANDALONE
            flags.Add("CARROM_STANDALONE");
#endif
#if LUDO_STANDALONE
            flags.Add("LUDO_STANDALONE");
#endif
#if CASH_APP
            flags.Add("CASH_APP");
#endif
#if PS_CASH_APP
            flags.Add("PS_CASH_APP");
#endif
#if STANDALONE_GAME
            flags.Add("STANDALONE_GAME");
#endif
#if USE_FLUTTER_TO_MAIL_LOGS
            flags.Add("USE_FLUTTER_TO_MAIL_LOGS");
#endif
#if PERIPHERALS
            flags.Add("PERIPHERALS");
#endif
            return flags;
        }
        #endregion //Private Methods
    }
}
