using UnityEngine;
using System.Collections.Generic;

public static class Dice  {

    static Dictionary<int, Die> dice = new Dictionary<int, Die>();

    static void AddDie(int sides)
    {
        dice.Add(sides, new Die(sides));
    }

    public static int Roll(int sides)
    {
        if (!dice.ContainsKey(sides))
            AddDie(sides);

        return dice[sides].Roll();
    }

    public static int[] Roll(int[] sides)
    {
        var rolls = new int[sides.Length];

        for (int i = 0; i < sides.Length; i++)
        {
            if (sides[i] == 0)
                rolls[i] = 0;
            else
                rolls[i] = Roll(sides[i]);
        }

        return rolls;
    }

    public static int SumRoll(params int[] sides)
    {
        var total = 0;

        for (int i = 0; i < sides.Length; i++) {

            if (sides[i] == 0)
                continue;

            total += Roll(sides[i]);
        }
        return total;
    }

}

class Die
{
    int sides;

    public Die(int sides)
    {
        this.sides = sides;
    }

    public int Roll()
    {
        return Random.Range(1, sides);
    }
}