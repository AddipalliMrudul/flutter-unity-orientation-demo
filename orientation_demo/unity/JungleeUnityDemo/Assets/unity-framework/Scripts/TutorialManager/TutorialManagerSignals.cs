using XcelerateGames.IOC;

namespace XcelerateGames.Tutorials
{
    //Asset name, Force play 
    public class SigPlayTutorial : Signal<string/*Tutorial Name*/, bool/*Force Play*/> { }
    public class SigStopTutorial : Signal { }
    public class SigPauseTutorial : Signal<bool> { }
    //Step index
    public class SigSetTutorialStep : Signal<int> { }
    public class SigGoToNextStep : Signal { }
    //This signal will be fired before setting up required stuff for tutorial step
    public class SigOnTutorialStep : Signal<string/*Tutorial Name*/, string/*Step name*/, int/*Step Index*/, bool/*Is Entering step*/> { }
    //This signal will be fired after setting up required stuff for tutorial step
    public class SigOnTutorialStepSetupDone : Signal<string/*Tutorial Name*/, string/*Step name*/, int/*Step Index*/, bool/*Is Entering step*/> { }
    public class SigTutorialStarted : Signal<string/*Tutorial Name*/, TutorialData> { }
    //Tutorial name, was tutorial skipped
    public class SigTutorialComplete : Signal<string/*Tutorial Name*/, bool/*skipped?*/, string /*step name*/> { }
    public class SigOnHandTap : Signal<string/*Step Name*/> { }
    public class SigShowMask : Signal<bool/*Show/Hide*/> { }
    public class SigDestroyTutorials : Signal { }
}
