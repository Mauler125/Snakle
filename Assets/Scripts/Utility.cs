//===========================================================================//
//
// Purpose: generic script utilities & tools
//
//===========================================================================//
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    static public string GetDate_Formatted(int daysToAdd = 0)
    {
        return DateTime.Today.AddDays(daysToAdd).ToString("yyyy-MM-dd");
    }

    static public string GetCurrentDate_Formatted()
    {
        return GetDate_Formatted();
    }
}
