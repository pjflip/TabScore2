// TabScore2, a wireless bridge scoring program.  Copyright(C) 2026 by Peter Flippant
// Licensed under the Apache License, Version 2.0; you may not use this file except in compliance with the License

namespace TabScore2.Globals
{
    public enum HeaderType
    {
        Location,
        FullPlain,
        FullColoured,
        Round,
    }

    public enum LeadValidationOptions
    {
        Validate,
        Warning,
        NoWarning
    }

    public enum ButtonOptions
    {
        OKEnabled,
        OKEnabledAndBack,
        OKDisabled,
        OKDisabledAndBack
    }

    public enum Direction
    {
        North,
        East,
        South,
        West,
        Sitout,
        Null
    }

    public enum HandRecordPerspectiveButtonOptions
    {
        None,
        NSEW,
        NS,
        EW
    }

    public enum Mode
    {
        Traditional = 0,
        Personal = 1,
        Scorer = 2,
    }
}
