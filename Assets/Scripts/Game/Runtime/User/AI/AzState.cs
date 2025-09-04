
using System;
using System.Collections.Generic;
using Game.Field;
using Game.User;

public struct AzState
{
    // owners: -1 / 0 / +1
    public sbyte[,] Owners; // [3,3]
    public sbyte[,] Values; // [3,3] — Owners
    public bool[] RemP1; // length 7: true if available (v=idx+1)
    public bool[] RemP2; // length 7
    public sbyte Current; // +1 or -1
    public bool Done;
    public sbyte Winner; // +1 / -1 / 0

    public static AzState FromUnity(FieldModel field, UserEntitiesModel p1, UserEntitiesModel p2, int unityCurrent)
    {
        var st = new AzState
        {
            Owners = new sbyte[3, 3],
            Values = new sbyte[3, 3],
            RemP1 = new bool[7],
            RemP2 = new bool[7],
            Current = (sbyte) (unityCurrent == 2 ? -1 : +1),
            Done = false,
            Winner = 0
        };

        // field
        foreach (var kvp in field.Entities)
        {
            var pos = kvp.Key;
            var cell = kvp.Value;
            int r = pos.x, c = pos.y;
            int o = cell.Data.Owner.Value; // 0/1/2  -> 0/+1/-1
            int ownerF = (o == 1 ? +1 : o == 2 ? -1 : 0);
            int vAbs = Math.Abs(cell.Data.Merit.Value);
            st.Owners[r, c] = (sbyte) ownerF;
            st.Values[r, c] = (sbyte) vAbs;
        }

        foreach (var e in p1.Entities)
        {
            int v = e.Data.Merit.Value;
            if (1 <= v && v <= 7) st.RemP1[v - 1] = true;
        }

        foreach (var e in p2.Entities)
        {
            int v = e.Data.Merit.Value;
            if (1 <= v && v <= 7) st.RemP2[v - 1] = true;
        }

        return st;
    }

    public List<(int v, int r, int c)> GetValidActions()
    {
        var result = new List<(int, int, int)>(63);
        bool[] rem = (Current == +1) ? RemP1 : RemP2;

        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            int owner = Owners[r, c];
            int cellVal = Values[r, c];
            for (int v = 1; v <= 7; v++)
            {
                if (!rem[v - 1]) continue;
                if (owner == 0 || (owner == -Current && cellVal < v))
                    result.Add((v, r, c));
            }
        }

        return result;
    }

    public bool CheckWinner(int player)
    {
        var b = Owners;
        for (int i = 0; i < 3; i++)
        {
            if (b[i, 0] == player && b[i, 1] == player && b[i, 2] == player) return true;
            if (b[0, i] == player && b[1, i] == player && b[2, i] == player) return true;
        }

        if (b[0, 0] == player && b[1, 1] == player && b[2, 2] == player) return true;
        if (b[0, 2] == player && b[1, 1] == player && b[2, 0] == player) return true;
        return false;
    }

    public bool NoPiecesLeftBoth()
    {
        bool any1 = false, any2 = false;
        for (int i = 0; i < 7; i++)
        {
            any1 |= RemP1[i];
            any2 |= RemP2[i];
        }

        return !any1 && !any2;
    }

    public void StepUnchecked(int v, int r, int c)
    {
        int owner = Owners[r, c];
        if (owner == -Current && Values[r, c] < v)
        {
            Values[r, c] = (sbyte) v;
            Owners[r, c] = Current;
        }
        else
        {
            Values[r, c] = (sbyte) v;
            Owners[r, c] = Current;
        }

        if (Current == +1) RemP1[v - 1] = false;
        else RemP2[v - 1] = false;

        if (CheckWinner(Current))
        {
            Done = true;
            Winner = Current;
            return;
        }

        if (NoPiecesLeftBoth())
        {
            Done = true;
            Winner = 0;
            return;
        }

        Current = (sbyte) (-Current);
    }

    public AzState Clone()
    {
        var s = new AzState
        {
            Owners = (sbyte[,]) Owners.Clone(),
            Values = (sbyte[,]) Values.Clone(),
            RemP1 = (bool[]) RemP1.Clone(),
            RemP2 = (bool[]) RemP2.Clone(),
            Current = Current,
            Done = Done,
            Winner = Winner
        };
        return s;
    }

    public float[] ToFeatures32()
    {
        var boardVals = new float[9];
        var boardOwns = new float[9];
        int k = 0;
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            int owner = Owners[r, c];
            int vAbs = Values[r, c];
            int ownerEgo = owner * Current; // -1/0/+1
            boardOwns[k] = ownerEgo;
            boardVals[k] = vAbs * ownerEgo;
            k++;
        }

        var ownRem = new float[7];
        var oppRem = new float[7];
        bool[] remOwn = (Current == +1) ? RemP1 : RemP2;
        bool[] remOpp = (Current == +1) ? RemP2 : RemP1;
        for (int i = 0; i < 7; i++)
        {
            if (remOwn[i]) ownRem[i] = 1f;
            if (remOpp[i]) oppRem[i] = 1f;
        }

        var state = new float[32];
        Array.Copy(boardVals, 0, state, 0, 9);
        Array.Copy(boardOwns, 0, state, 9, 9);
        Array.Copy(ownRem, 0, state, 18, 7);
        Array.Copy(oppRem, 0, state, 25, 7);
        return state;
    }

    public static int ActionToIndex(int v, int r, int c)
    {
        return (v - 1) * 9 + (r * 3 + c);
    }

    public static (int v, int r, int c) IndexToAction(int idx)
    {
        int v = idx / 9 + 1;
        int pos = idx % 9;
        int r = pos / 3;
        int c = pos % 3;
        return (v, r, c);
    }
}