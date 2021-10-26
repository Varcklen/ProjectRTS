[System.Flags]
public enum ObjectType
{
    Nothing = 0,
    Minion = 1,
    Building = 2,
    Node = 4,
    Collector = 8,
    //Item = 16,
    Self = 32,//Needed to check target selection
    Hero = 64,
    Boss = 128,
    Ally = 256,
    Enemy = 512
    //*2 and etc. Until 32th parameter
}
