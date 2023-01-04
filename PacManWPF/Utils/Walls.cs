using System;

namespace PacManWPF.Utils
{
    [Flags]
    public enum Walls
    {
        Nothing =           0b000000000000000,

        Top =               0b000000000000001,
        Left =              0b000000000000010,
        Right =             0b000000000000100,
        Bottom =            0b000000000001000,
        
        SmallTop =          0b000000000010000,
        SmallLeft =         0b000000000100000,
        SmallRight =        0b000000001000000,
        SmallBottom =       0b000000010000000,


        CurveTop =          0b000000100000000,
        CurveLeft =         0b000001000000000,
        CurveRight =        0b000010000000000,
        CurveBottom =       0b000100000000000,


        SmallCurveTop =    0b0001000000000000,
        SmallCurveLeft =   0b0010000000000000,
        SmallCurveRight =  0b0100000000000000,
        SmallCurveBottom = 0b1000000000000000,
    }
}
